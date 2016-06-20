using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsynchronousTcp {
	public class TcpManager {
		/// <summary>
		/// 新消息到达事件
		/// </summary>
		public event Action<TcpClient, string> MessageReceivedEvent;
		/// <summary>
		/// 套接字关闭事件
		/// </summary>
		public event Action<TcpClient, Exception> ErrorEvent;

		private byte[] headerIdentification = { 0x00, 0xFF, 0x11, 0xEE };
		private int headerLengthSize = 6;

		/// <summary>
		/// 开始监听本地端口
		/// </summary>
		/// <param name="address">本地Ip地址</param>
		/// <param name="port">本地端口</param>
		/// <param name="callBack">回调函数</param>
		public void StartListening(IPAddress address, int port, Action<TcpClient> callBack) {
			Task.Run(async () => {
				IPEndPoint localEP = new IPEndPoint(address, port);
				TcpListener listener = new TcpListener(localEP);
				listener.Start(int.MaxValue);
				while(true) {
					TcpClient client = await listener.AcceptTcpClientAsync();
					callBack?.Invoke(client);
					var _ = startReceiving(client);
				}
			});
		}

		/// <summary>
		/// 开始连接到远程设备
		/// </summary>
		/// <param name="address">远程Ip地址</param>
		/// <param name="port">远程端口</param>
		/// <param name="callBack">成功连接回调函数</param>
		public async Task StartConnecting(IPAddress address, int port, Action<TcpClient> callBack) {
			TcpClient client = new TcpClient();
			// Connect to the remote endpoint.
			try {
				await client.ConnectAsync(address, port);

				callBack?.Invoke(client);

				var _ = startReceiving(client);
			}
			catch(Exception e) {
				handleError(client, e);
			}
		}

		/// <summary>
		/// 向套接字发送数据
		/// </summary>
		/// <param name="client">已连接套接字</param>
		/// <param name="data">数据</param>
		/// <param name="callBack">发送完成回调函数</param>
		/// <returns></returns>
		public async Task Send(TcpClient client, string data, Action callBack) {
			if(client == null)
				return;

			byte[] dataBytes = Encoding.UTF8.GetBytes(data);

			// 待发送的传输字节
			byte[] bytesWrite = new byte[headerIdentification.Length + headerLengthSize + dataBytes.Length];

			// 复制标识字节
			Array.Copy(headerIdentification, bytesWrite, headerIdentification.Length);

			// 获得数据字节的长度并复制
			byte[] lengthBytes = BitConverter.GetBytes(dataBytes.Length);
			Array.Copy(lengthBytes, 0, bytesWrite, headerIdentification.Length, lengthBytes.Length);

			// 复制数据字节
			Array.Copy(dataBytes, 0, bytesWrite, headerIdentification.Length + headerLengthSize, dataBytes.Length);

			try {
				NetworkStream networkStream = client.GetStream();
				await networkStream.WriteAsync(bytesWrite, 0, bytesWrite.Length);

				callBack?.Invoke();
			}
			catch(Exception e) {
				handleError(client, e);
			}
		}

		private async Task startReceiving(TcpClient client) {
			if(client == null)
				return;

			byte[] receivedBytes = new byte[0];
			int shouldReadLength = headerIdentification.Length + headerLengthSize;
			bool packageCompleted = true;

			try {
				NetworkStream networkStream = client.GetStream();
				int receiveZeroCount = 0;

				while(true) {
					byte[] buffer = new byte[shouldReadLength];

					int lengthRead = await networkStream.ReadAsync(buffer, 0, shouldReadLength);

					// 如果持续受到0十次以上，则远程socket连接关闭
					if(lengthRead == 0) {
						receiveZeroCount++;
						if(receiveZeroCount >= 10) {
							networkStream.Close();
							handleError(client, new Exception("远程连接被关闭"));
							break;
						}
						continue;
					}
					receiveZeroCount = 0;

					if(packageCompleted) {
						if(lengthRead < headerIdentification.Length + headerLengthSize) {
							continue;
						}

						byte[] receivedHeaderIdentification = new byte[headerIdentification.Length];
						byte[] receivedHeaderLength = new byte[headerLengthSize];

						Array.Copy(buffer, 0, receivedHeaderIdentification, 0, headerIdentification.Length);
						Array.Copy(buffer, headerIdentification.Length, receivedHeaderLength, 0, headerLengthSize);

						// 验证标识字节 
						if(!bytesArrayEquals(receivedHeaderIdentification, headerIdentification)) {
							continue;
						}

						shouldReadLength = BitConverter.ToInt32(receivedHeaderLength, 0);
						packageCompleted = false;
					}
					else {
						byte[] tempBuffer = new byte[receivedBytes.Length + lengthRead];
						Array.Copy(receivedBytes, tempBuffer, receivedBytes.Length);
						Array.Copy(buffer, 0, tempBuffer, receivedBytes.Length, lengthRead);
						receivedBytes = tempBuffer;

						shouldReadLength -= lengthRead;
					}

					if(shouldReadLength == 0) {
						MessageReceivedEvent?.Invoke(client, Encoding.UTF8.GetString(receivedBytes));

						receivedBytes = new byte[0];
						shouldReadLength = headerIdentification.Length + headerLengthSize;
						packageCompleted = true;
					}
				}
			}
			catch(Exception e) {
				handleError(client, e);
			}
		}

		private void handleError(TcpClient client, Exception e) {
			client?.Close();
			ErrorEvent?.Invoke(client, e);
		}

		/// <summary>
		/// 比较两个字节数组是否相等
		/// </summary>
		/// <param name="b1">byte数组1</param>
		/// <param name="b2">byte数组2</param>
		/// <returns>是否相等</returns>
		private bool bytesArrayEquals(byte[] b1, byte[] b2) {
			if(b1.Length != b2.Length)
				return false;
			if(b1 == null || b2 == null)
				return false;
			for(int i = 0; i < b1.Length; i++) {
				if(b1[i] != b2[i]) {
					return false;
				}
			}
			return true;
		}
	}
}
