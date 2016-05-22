using AsynchronousTcp;
using Newtonsoft.Json;
using Protocal;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace YummyOnlineTcpClient {
	public class TcpClient {
		private IPAddress ip;
		private int port;
		private BaseTcpProtocal connectSender;

		private TcpManager tcp;
		private System.Net.Sockets.TcpClient client = null;

		private Queue<BaseTcpProtocal> waitedQueue = new Queue<BaseTcpProtocal>();

		/// <summary>
		/// 连接成功回调函数
		/// </summary>
		public Action CallBackWhenConnected = null;
		/// <summary>
		/// 异常发生回调函数
		/// </summary>
		public Action<Exception> CallBackWhenExceptionOccured = null;
		/// <summary>
		/// 接收到信息回调函数(TcpProtocalType, Protocal)
		/// </summary>
		public Action<string, object> CallBackWhenMessageReceived = null;
		/// <summary>
		/// 重新连接的等待时间（秒）
		/// 默认5秒
		/// </summary>
		public int ReconnectInterval { get; set; } = 5;

		/// <summary>
		/// TcpClient构造函数
		/// </summary>
		/// <param name="ip">ip地址</param>
		/// <param name="port">端口</param>
		/// <param name="connectSender">连接完成发送的身份信息</param>
		public TcpClient(IPAddress ip, int port, BaseTcpProtocal connectSender) {
			this.ip = ip;
			this.port = port;
			this.connectSender = connectSender;

			tcp = new TcpManager();
			tcp.MessageReceivedEvent += (client, content) => {
				try {
					BaseTcpProtocal p = JsonConvert.DeserializeObject<BaseTcpProtocal>(content);

					object obj = null;
					switch(p.Type) {
						case TcpProtocalType.NewDineInform:
							obj = JsonConvert.DeserializeObject<NewDineInformProtocal>(content);
							break;
						case TcpProtocalType.PrintDine:
							obj = JsonConvert.DeserializeObject<PrintDineProtocal>(content);
							break;
					}

					CallBackWhenMessageReceived?.Invoke(p.Type, obj);
				}
				catch(Exception e) {
					exceptionOccured(e);
				}
			};
			tcp.ErrorEvent += async (s, e) => {
				client = null;
				exceptionOccured(e);
				// 重新连接
				await Task.Delay(ReconnectInterval * 1000);
				Start();
			};
		}
		public void Start() {
			var _ = tcp.StartConnecting(ip, port, p => {
				CallBackWhenConnected?.Invoke();

				client = p;
				string content = JsonConvert.SerializeObject(connectSender);
				var t = tcp.Send(p, content, null);

				while(waitedQueue.Count > 0) {
					Send(waitedQueue.Dequeue());
				}
			});
		}

		private void exceptionOccured(Exception e) {
			CallBackWhenExceptionOccured?.Invoke(e);
		}

		/// <summary>
		/// 发送tcp协议
		/// </summary>
		/// <param name="p">协议</param>
		public void Send(BaseTcpProtocal p, Action callBack = null) {
			if(client == null) {
				waitedQueue.Enqueue(p);
				return;
			}
			var _ = tcp.Send(client, JsonConvert.SerializeObject(p), () => {
				callBack?.Invoke();
			});
		}
	}
}
