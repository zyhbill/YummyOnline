using AsynchronousTcp;
using Newtonsoft.Json;
using Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace YummyOnlineTcpClient {
	public class TcpClient {
		private IPAddress ip;
		private int port;
		private BaseTcpProtocol connectSender;

		private TcpManager tcp;
		private System.Net.Sockets.TcpClient client = null;
		private int heartAlive = 0;

		private Queue<BaseTcpProtocol> waitedQueue = new Queue<BaseTcpProtocol>();

		/// <summary>
		/// 连接成功回调函数
		/// </summary>
		public Action CallBackWhenConnected = null;
		/// <summary>
		/// 异常发生回调函数
		/// </summary>
		public Action<Exception> CallBackWhenExceptionOccured = null;
		/// <summary>
		/// 接收到信息回调函数(TcpProtocolType, Protocol)
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
		public TcpClient(IPAddress ip, int port, BaseTcpProtocol connectSender) {
			this.ip = ip;
			this.port = port;
			this.connectSender = connectSender;

			tcp = new TcpManager();
			tcp.MessageReceivedEvent += (client, content) => {
				try {
					BaseTcpProtocol p = JsonConvert.DeserializeObject<BaseTcpProtocol>(content);

					object obj = null;
					switch(p.Type) {
						case TcpProtocolType.HeartBeat:
							// 如果接收到心跳包, 则发送心跳包
							heartAlive = 0;
							var _ = tcp.Send(client, JsonConvert.SerializeObject(new HeartBeatProtocol()), null);
							return;
						case TcpProtocolType.NewDineInform:
							obj = JsonConvert.DeserializeObject<NewDineInformProtocol>(content);
							break;
						case TcpProtocolType.PrintDine:
							obj = JsonConvert.DeserializeObject<PrintDineProtocol>(content);
							break;
						case TcpProtocolType.PrintShifts:
							obj = JsonConvert.DeserializeObject<PrintShiftsProtocol>(content);
							break;
						case TcpProtocolType.TcpServerStatusInform:
							obj = JsonConvert.DeserializeObject<TcpServerStatusInformProtocol>(content);
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

			Timer timer = new Timer(10 * 1000);
			timer.Elapsed += Timer_Elapsed;
			//timer.Start();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e) {
			if(client == null)
				return;

			heartAlive++;
			if(heartAlive > 6) {
				CallBackWhenExceptionOccured(new Exception("远程服务器长时间未响应"));
				client?.Close();
			}
		}

		public void Start() {
			var _ = tcp.StartConnecting(ip, port, p => {
				CallBackWhenConnected?.Invoke();

				heartAlive = 0;
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
		public void Send(BaseTcpProtocol p, Action callBack = null) {
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
