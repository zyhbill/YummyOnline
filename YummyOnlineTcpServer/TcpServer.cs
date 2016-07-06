using AsynchronousTcp;
using Newtonsoft.Json;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;

namespace YummyOnlineTcpServer {
	public partial class TcpServer {
		private TcpManager tcp;
		private string ip;
		private int port;
		private Action<string, Log.LogLevel> logDelegate;

		// 用于打印机或新订单通知客户端重复连接时, 等待原有客户端断开日志记录完毕再执行下面代码·
		private ManualResetEvent clientCloseMutex = new ManualResetEvent(true);

		/// <summary>
		/// 已经链接但是等待身份验证的socket
		/// </summary>
		private List<TcpClientInfo> waitingForVerificationClients = new List<TcpClientInfo>();

		private TcpClientInfo systemClient = null;
		/// <summary>
		/// 接收新订单的socket
		/// </summary>
		private Dictionary<NewDineInformClientGuid, TcpClientInfo> newDineInformClients = new Dictionary<NewDineInformClientGuid, TcpClientInfo>();
		/// <summary>
		/// 打印机socket
		/// </summary>
		private Dictionary<int, TcpClientInfo> printerClients = new Dictionary<int, TcpClientInfo>();
		/// <summary>
		/// 打印等待队列
		/// </summary>
		private Dictionary<int, Queue<BaseTcpProtocol>> printerWaitedQueue = new Dictionary<int, Queue<BaseTcpProtocol>>();

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="logDelegate">记录日志回调函数, 参数1: 日志信息, 参数2: 日志等级</param>
		/// <param name="clientsStatusDelegate">socket客户端状态变化回调函数</param>
		public TcpServer(string ip, int port, Action<string, Log.LogLevel> logDelegate) {
			tcp = new TcpManager();
			this.ip = ip;
			this.port = port;
			tcp.MessageReceivedEvent += Tcp_MessageReceivedEvent;
			tcp.ErrorEvent += Tcp_ErrorEvent;
			this.logDelegate = logDelegate;
		}
		public async Task Initialize() {
			YummyOnlineManager manager = new YummyOnlineManager();

			List<Hotel> hotels = await manager.GetHotels();
			hotels.ForEach(h => {
				printerClients.Add(h.Id, null);
				printerWaitedQueue.Add(h.Id, new Queue<BaseTcpProtocol>());
			});
			List<NewDineInformClientGuid> guids = await manager.GetGuids();
			guids.ForEach(g => {
				newDineInformClients.Add(g, null);
			});

			log($"Binding {ip}:{port}", Log.LogLevel.Info);

			tcp.StartListening(IPAddress.Parse(ip), port, client => {
				TcpClientInfo clientInfo = new TcpClientInfo(client);
				lock(waitingForVerificationClients) {
					waitingForVerificationClients.Add(clientInfo);
				}
				log($"{clientInfo.OriginalRemotePoint} Connected, Waiting for verification", Log.LogLevel.Info);
			});


			System.Timers.Timer timer = new System.Timers.Timer(10 * 1000);
			timer.Elapsed += (e, o) => {
				// 30秒之内已连接但是未发送身份信息的socket断开
				lock(waitingForVerificationClients) {
					foreach(var client in waitingForVerificationClients) {
						client.HeartAlive++;
						if(client.HeartAlive > 3) {
							log($"{client.OriginalRemotePoint} Timeout", Log.LogLevel.Warning);
							client.Client.Close();
						}
					}
				}

				//60秒之内没有接收到心跳包的socket断开, 或发送心跳包失败的socket断开
				//if(systemClient != null) {
				//	sendHeartBeat(systemClient);
				//	systemClient.HeartAlive++;
				//	if(systemClient.HeartAlive > 6) {
				//		log($"System {systemClient.OriginalRemotePoint} HeartAlive Timeout", Log.LogLevel.Error);
				//		systemClient.Client.Close();
				//		systemClient = null;
				//		clientsStatusChange();
				//	}
				//}
				//foreach(var pair in newDineInformClients.Where(p => p.Value != null)) {
				//	sendHeartBeat(pair.Value);
				//	pair.Value.HeartAlive++;
				//	if(pair.Value.HeartAlive > 1) {
				//		pair.Value.Client.Close();
				//		newDineInformClients[pair.Key] = null;
				//		log($"({pair.Key.Description}) {pair.Value.OriginalRemotePoint} HeartAlive Timeout", Log.LogLevel.Success);
				//	}
				//}
				foreach(var pair in printerClients.Where(p => p.Value != null)) {
					sendHeartBeat(pair.Value);
					pair.Value.HeartAlive++;
					if(pair.Value.HeartAlive > 6) {
						pair.Value.Client.Close();
						printerClients[pair.Key] = null;
						log($"Printer (Hotel{pair.Key}) {pair.Value.OriginalRemotePoint} HeartAlive Timeout", Log.LogLevel.Error);
					}
				}
			};
			timer.Start();
		}


		private void Tcp_MessageReceivedEvent(TcpClient client, string content) {
			try {
				BaseTcpProtocol baseProtocol = JsonConvert.DeserializeObject<BaseTcpProtocol>(content);
				TcpClientInfo clientInfo = new TcpClientInfo(client);

				switch(baseProtocol.Type) {
					case TcpProtocolType.HeartBeat:
						heartBeat(clientInfo);
						break;
					case TcpProtocolType.SystemConnect:
						systemConnect(clientInfo);
						break;
					case TcpProtocolType.SystemCommand:
						log(content, Log.LogLevel.Debug);
						systemCommand(clientInfo, JsonConvert.DeserializeObject<SystemCommandProtocol>(content));
						break;
					case TcpProtocolType.NewDineInformClientConnect:
						newDineInfromClientConnected(clientInfo, JsonConvert.DeserializeObject<NewDineInformClientConnectProtocol>(content));
						break;
					case TcpProtocolType.PrintDineClientConnect:
						printDineClientConnected(clientInfo, JsonConvert.DeserializeObject<PrintDineClientConnectProtocol>(content));
						break;
					case TcpProtocolType.NewDineInform:
						newDineInform(clientInfo, JsonConvert.DeserializeObject<NewDineInformProtocol>(content));
						break;
					case TcpProtocolType.RequestPrintDine:
						requestPrintDine(clientInfo, JsonConvert.DeserializeObject<RequestPrintDineProtocol>(content));
						break;
					case TcpProtocolType.RequestPrintShifts:
						requestPrintShifts(clientInfo, JsonConvert.DeserializeObject<RequestPrintShiftsProtocol>(content));
						break;
				}
			}
			catch(Exception e) {
				log($"{client.Client.RemoteEndPoint} Receive Error: {e.Message}, Data: {content}", Log.LogLevel.Error);
				client.Close();
			}
		}

		private void Tcp_ErrorEvent(TcpClient client, Exception e) {
			TcpClientInfo clientInfo = waitingForVerificationClients.FirstOrDefault(p => p.Client == client);
			if(clientInfo != null) {
				waitingForVerificationClients.Remove(clientInfo);
				log($"WaitingForVerificationClient {clientInfo.OriginalRemotePoint} Disconnected", Log.LogLevel.Error);
				return;
			}

			if(systemClient?.Client == client) {
				systemClient = null;
				log($"System {systemClient.OriginalRemotePoint} Disconnected", Log.LogLevel.Error);
				return;
			}

			foreach(var pair in newDineInformClients) {
				if(pair.Value?.Client == client) {
					newDineInformClients[pair.Key] = null;
					log($"NewDineInformClient ({pair.Key.Description}) {pair.Value.OriginalRemotePoint} Disconnected", Log.LogLevel.Error);
					clientCloseMutex.Set();
					return;
				}
			}

			foreach(var pair in printerClients) {
				if(pair.Value?.Client == client) {
					printerClients[pair.Key] = null;
					log($"Printer (Hotel{pair.Key}) {pair.Value.OriginalRemotePoint} Disconnected", Log.LogLevel.Error);
					clientCloseMutex.Set();
					return;
				}
			}
		}

		private void log(string log, Log.LogLevel level) {
			logDelegate(log, level);
		}
		public TcpServerStatusProtocol GetTcpServerStatus() {
			TcpServerStatusProtocol protocol = new TcpServerStatusProtocol();

			foreach(var p in waitingForVerificationClients) {
				protocol.WaitingForVerificationClients.Add(getClientStatus(p));
			}
			protocol.SystemClient = getClientStatus(systemClient);
			foreach(var pair in newDineInformClients) {
				protocol.NewDineInformClients.Add(new NewDineInformClientStatus {
					Guid = pair.Key.Guid,
					Description = pair.Key.Description,
					Status = getClientStatus(pair.Value)
				});
			}
			foreach(var pair in printerClients) {
				protocol.PrinterClients.Add(new PrinterClientStatus {
					HotelId = pair.Key,
					WaitedCount = printerWaitedQueue[pair.Key].Count,
					Status = getClientStatus(pair.Value)
				});
			}

			return protocol;
		}

		private ClientStatus getClientStatus(TcpClientInfo info) {
			ClientStatus status = new ClientStatus();
			if(info != null) {
				status.IsConnected = true;
				status.ConnectedTime = info.ConnectedTime;
				status.IpAddress = info.OriginalRemotePoint.Address.ToString();
				status.Port = info.OriginalRemotePoint.Port;
			}
			else {
				status.IsConnected = false;
			}
			return status;
		}
	}
}
