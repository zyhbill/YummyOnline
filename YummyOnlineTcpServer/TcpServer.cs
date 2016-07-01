using AsynchronousTcp;
using Newtonsoft.Json;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;

namespace YummyOnlineTcpServer {
	public class TcpServer {
		public class TcpClientInfo {
			public TcpClientInfo(TcpClient client) {
				Client = client;
				OriginalRemotePoint = (IPEndPoint)client.Client.RemoteEndPoint;
				ConnectedTime = DateTime.Now;
			}

			public TcpClient Client { get; set; }
			public IPEndPoint OriginalRemotePoint { get; set; }
			public DateTime ConnectedTime { get; set; }
			/// <summary>
			/// 心跳数
			/// </summary>
			public int HeartAlive { get; set; }
		}

		private TcpManager tcp;
		private string ip;
		private int port;
		private Action<string, Log.LogLevel> logDelegate;
		private Action<TcpServerStatusProtocol> clientsStatusDelegate;

		// 用于打印机或新订单通知客户端重复连接时, 等待原有客户端断开日志记录完毕再执行下面代码·
		private ManualResetEvent clientCloseMutex = new ManualResetEvent(true);

		/// <summary>
		/// 已经链接但是等待身份验证的socket
		/// </summary>
		public List<TcpClientInfo> WaitingForVerificationClients { get; set; } = new List<TcpClientInfo>();

		/// <summary>
		/// 接收新订单的socket
		/// </summary>
		public Dictionary<NewDineInformClientGuid, TcpClientInfo> NewDineInformClients { get; set; } = new Dictionary<NewDineInformClientGuid, TcpClientInfo>();
		/// <summary>
		/// 打印机socket
		/// </summary>
		public Dictionary<int, TcpClientInfo> PrinterClients { get; set; } = new Dictionary<int, TcpClientInfo>();

		private Dictionary<int, Queue<BaseTcpProtocol>> printerWaitedQueue = new Dictionary<int, Queue<BaseTcpProtocol>>();

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="logDelegate">记录日志回调函数, 参数1: 日志信息, 参数2: 日志等级</param>
		/// <param name="clientsStatusDelegate">socket客户端状态变化回调函数</param>
		public TcpServer(string ip, int port, Action<string, Log.LogLevel> logDelegate, Action<TcpServerStatusProtocol> clientsStatusDelegate) {
			tcp = new TcpManager();
			this.ip = ip;
			this.port = port;
			tcp.MessageReceivedEvent += Tcp_MessageReceivedEvent;
			tcp.ErrorEvent += Tcp_ErrorEvent;
			this.logDelegate = logDelegate;
			this.clientsStatusDelegate = clientsStatusDelegate;
		}
		public async Task Initialize() {
			YummyOnlineManager manager = new YummyOnlineManager();

			List<Hotel> hotels = await manager.GetHotels();
			hotels.ForEach(h => {
				PrinterClients.Add(h.Id, null);
				printerWaitedQueue.Add(h.Id, new Queue<BaseTcpProtocol>());
			});
			List<NewDineInformClientGuid> guids = await manager.GetGuids();
			guids.ForEach(g => {
				NewDineInformClients.Add(g, null);
			});

			log($"Binding {ip}:{port}", Log.LogLevel.Info);

			tcp.StartListening(IPAddress.Parse(ip), port, client => {
				TcpClientInfo clientInfo = new TcpClientInfo(client);
				lock(WaitingForVerificationClients) {
					WaitingForVerificationClients.Add(clientInfo);
				}
				log($"{clientInfo.OriginalRemotePoint} Connected, Waiting for verification", Log.LogLevel.Info);
			});


			System.Timers.Timer timer = new System.Timers.Timer(10 * 1000);
			timer.Elapsed += (e, o) => {
				// 30秒之内已连接但是未发送身份信息的socket断开
				lock(WaitingForVerificationClients) {
					foreach(var client in WaitingForVerificationClients) {
						client.HeartAlive++;
						if(client.HeartAlive > 3) {
							log($"{client.OriginalRemotePoint} Timeout", Log.LogLevel.Warning);
							client.Client.Close();
						}
					}
				}

				// 60秒之内没有接收到心跳包的socket断开, 或发送心跳包失败的socket断开
				foreach(var pair in PrinterClients.Where(p => p.Value != null)) {
					sendHeartBeat(pair.Value);
					pair.Value.HeartAlive++;
					if(pair.Value.HeartAlive > 6) {
						log($"Printer (Hotel{pair.Key}) {pair.Value.OriginalRemotePoint} HeartAlive Timeout", Log.LogLevel.Error);
						pair.Value.Client.Close();
					}
				}
				//foreach(var pair in NewDineInformClients.Where(p => p.Value != null)) {
				//	sendHeartBeat(pair.Value);
				//	pair.Value.HeartAlive++;
				//	if(pair.Value.HeartAlive > 6) {
				//		log($"({pair.Key.Description}) {pair.Value.OriginalRemotePoint} HeartAlive Timeout", Log.LogLevel.Success);
				//		pair.Value.Client.Close();
				//	}
				//}
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
			TcpClientInfo clientInfo = WaitingForVerificationClients.FirstOrDefault(p => p.Client == client);

			if(clientInfo != null) {
				WaitingForVerificationClients.Remove(clientInfo);
				log($"WaitingForVerificationClient {clientInfo.OriginalRemotePoint} Disconnected", Log.LogLevel.Error);
				return;
			}

			foreach(var pair in NewDineInformClients) {
				if(pair.Value?.Client == client) {
					NewDineInformClients[pair.Key] = null;
					log($"NewDineInformClient ({pair.Key.Description}) {pair.Value.OriginalRemotePoint} Disconnected", Log.LogLevel.Error);
					clientCloseMutex.Set();
					return;
				}
			}

			foreach(var pair in PrinterClients) {
				if(pair.Value?.Client == client) {
					PrinterClients[pair.Key] = null;
					log($"Printer (Hotel{pair.Key}) {pair.Value.OriginalRemotePoint} Disconnected", Log.LogLevel.Error);
					return;
				}
			}
		}

		private void heartBeat(TcpClientInfo clientInfo) {
			foreach(var pair in PrinterClients.Where(p => p.Value != null)) {
				if(pair.Value.Client == clientInfo.Client) {
					pair.Value.HeartAlive = 0;
					return;
				}
			}
			foreach(var pair in NewDineInformClients.Where(p => p.Value != null)) {
				if(pair.Value.Client == clientInfo.Client) {
					pair.Value.HeartAlive = 0;
					return;
				}
			}
		}

		/// <summary>
		/// 需要及时收到新订单的客户端连接
		/// </summary>
		private void newDineInfromClientConnected(TcpClientInfo clientInfo, NewDineInformClientConnectProtocol protocol) {
			log($"{clientInfo.OriginalRemotePoint} (NewDineInformClient): Guid: {protocol.Guid} Request Connection", Log.LogLevel.Info);

			if(protocol.Guid == new Guid()) {
				log($"{clientInfo.OriginalRemotePoint} NewDineInformClient Lack Guid", Log.LogLevel.Warning);
				clientInfo.Client.Client.Close();
				return;
			}

			KeyValuePair<NewDineInformClientGuid, TcpClientInfo> pair = NewDineInformClients.FirstOrDefault(p => p.Key.Guid == protocol.Guid);
			if(pair.Key == null) {
				log($"{clientInfo.OriginalRemotePoint} NewDineInformClient Guid {protocol.Guid} Not Matched", Log.LogLevel.Warning);
				clientInfo.Client.Client.Close();
				return;
			}
			else {
				if(pair.Value != null) {
					log($"NewDineInformClient Guid {pair.Key.Guid} Repeated", Log.LogLevel.Warning);
					pair.Value.Client.Client.Close();
					clientCloseMutex.Reset();
					clientCloseMutex.WaitOne();
				}

				NewDineInformClients[pair.Key] = clientInfo;

				log($"{clientInfo.OriginalRemotePoint} ({pair.Key.Description}) Connected", Log.LogLevel.Success);
			}

			_clientVerified(clientInfo);
		}

		/// <summary>
		/// 饭店打印机连接
		/// </summary>
		private void printDineClientConnected(TcpClientInfo clientInfo, PrintDineClientConnectProtocol protocol) {
			log($"{clientInfo.OriginalRemotePoint} (Printer): HotelId: {protocol.HotelId} Request Connection", Log.LogLevel.Info);

			if(!PrinterClients.ContainsKey(protocol.HotelId)) {
				log($"{clientInfo.OriginalRemotePoint} Printer HotelId {protocol.HotelId} Not Matched", Log.LogLevel.Warning);
				clientInfo.Client.Client.Close();
				return;
			}

			KeyValuePair<int, TcpClientInfo> pair = PrinterClients.FirstOrDefault(p => p.Key == protocol.HotelId);

			if(pair.Value != null) {
				log($"Printer HotelId {pair.Key} Repeated", Log.LogLevel.Warning);
				pair.Value.Client.Client.Close();

				clientCloseMutex.Reset();
				clientCloseMutex.WaitOne();
			}

			PrinterClients[pair.Key] = clientInfo;

			log($"{clientInfo.OriginalRemotePoint} (Printer of Hotel {protocol.HotelId}) Connected", Log.LogLevel.Success);

			_clientVerified(PrinterClients[pair.Key]);

			// 打印存储在打印等待队列中的所有请求
			while(printerWaitedQueue[pair.Key].Count > 0) {
				BaseTcpProtocol printProtocol = printerWaitedQueue[pair.Key].Dequeue();
				if(printProtocol.Type == TcpProtocolType.PrintDine) {
					sendPrintDineProtocol(pair.Key, (PrintDineProtocol)printProtocol);
				}
				else if(printProtocol.Type == TcpProtocolType.PrintShifts) {
					sendPrintShiftsProtocol(pair.Key, (PrintShiftsProtocol)printProtocol);
				}
				log($"Send Waited Dine of Hotel {pair.Key}", Log.LogLevel.Success);
			}
		}

		private void _clientVerified(TcpClientInfo clientInfo) {
			lock(WaitingForVerificationClients) {
				TcpClientInfo waitedClientInfo = WaitingForVerificationClients.FirstOrDefault(p => p.Client == clientInfo.Client);
				clientInfo.ConnectedTime = waitedClientInfo.ConnectedTime;
				WaitingForVerificationClients.Remove(waitedClientInfo);
			}
			clientsStatusChange();
		}

		/// <summary>
		/// 新订单通知
		/// </summary>
		/// <param name="clientInfo"></param>
		/// <param name="protocol"></param>
		private void newDineInform(TcpClientInfo clientInfo, NewDineInformProtocol protocol) {
			NewDineInformClientGuid sender = getSender(clientInfo);
			if(sender == null)
				return;


			log($"{clientInfo.OriginalRemotePoint} (NewDineInform): From: {sender.Description}, HotelId: {protocol.HotelId}, DineId: {protocol.DineId}, IsPaid: {protocol.IsPaid}", Log.LogLevel.Success);

			foreach(var p in NewDineInformClients) {
				// 不向未连接的客户端与发送方客户端 发送新订单通知信息
				if(p.Value == null || p.Value.Client == clientInfo.Client)
					continue;

				string content = JsonConvert.SerializeObject(protocol);
				var _ = tcp.Send(p.Value.Client, content, null);
			}
		}

		/// <summary>
		/// 请求打印
		/// </summary>
		private void requestPrintDine(TcpClientInfo clientInfo, RequestPrintDineProtocol protocol) {
			NewDineInformClientGuid sender = getSender(clientInfo);
			if(sender == null)
				return;

			protocol.DineMenuIds = protocol.DineMenuIds ?? new List<int>();
			protocol.PrintTypes = protocol.PrintTypes ?? new List<PrintType>();

			StringBuilder dineMenuStr = new StringBuilder();
			for(int i = 0; i < protocol.DineMenuIds.Count; i++) {
				dineMenuStr.Append(protocol.DineMenuIds[i]);
				if(i != protocol.DineMenuIds.Count - 1)
					dineMenuStr.Append(' ');
			}

			StringBuilder typeStr = new StringBuilder();
			foreach(var type in protocol.PrintTypes) {
				typeStr.Append($"{type.ToString()} ");
			}

			log($"{clientInfo.OriginalRemotePoint} (RequestPrintDine): From: {sender.Description}, HotelId: {protocol.HotelId}, DineId: {protocol.DineId}, DineMenuIds: {dineMenuStr}, PrintTypes: {typeStr}",
				Log.LogLevel.Success);

			PrintDineProtocol p = new PrintDineProtocol(protocol.DineId, protocol.DineMenuIds, protocol.PrintTypes);
			if(PrinterClients[protocol.HotelId] == null) {
				printerWaitedQueue[protocol.HotelId].Enqueue(p);
				log($"Printer of Hotel {protocol.HotelId} is not connected", Log.LogLevel.Error);
				return;
			}
			sendPrintDineProtocol(protocol.HotelId, p);
		}

		/// <summary>
		/// 向饭店打印机发送打印协议
		/// </summary>
		private void sendPrintDineProtocol(int hotelId, PrintDineProtocol protocol) {
			var _ = tcp.Send(PrinterClients[hotelId].Client, JsonConvert.SerializeObject(protocol, new JsonSerializerSettings {
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			}), null);
		}

		private void requestPrintShifts(TcpClientInfo clientInfo, RequestPrintShiftsProtocol protocol) {
			NewDineInformClientGuid sender = getSender(clientInfo);
			if(sender == null)
				return;

			protocol.Ids = protocol.Ids ?? new List<int>();

			StringBuilder idStr = new StringBuilder();
			foreach(int id in protocol.Ids) {
				idStr.Append($"{id} ");
			}

			log($"{clientInfo.OriginalRemotePoint} (RequestPrintShifts): From: {sender.Description}, HotelId: {protocol.HotelId}, Ids: {idStr}, DateTime: {protocol.DateTime.ToString("yyyy-MM-dd")}",
				Log.LogLevel.Success);

			PrintShiftsProtocol p = new PrintShiftsProtocol(protocol.Ids, protocol.DateTime);
			if(PrinterClients[protocol.HotelId] == null) {
				printerWaitedQueue[protocol.HotelId].Enqueue(p);
				log($"Printer of Hotel {protocol.HotelId} is not connected", Log.LogLevel.Error);
				return;
			}
			sendPrintShiftsProtocol(protocol.HotelId, p);
		}
		private void sendPrintShiftsProtocol(int hotelId, PrintShiftsProtocol protocol) {
			var _ = tcp.Send(PrinterClients[hotelId].Client, JsonConvert.SerializeObject(protocol, new JsonSerializerSettings {
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			}), null);
		}

		private void sendHeartBeat(TcpClientInfo clientInfo) {
			var _ = tcp.Send(clientInfo.Client, JsonConvert.SerializeObject(new HeartBeatProtocol()), null);
		}

		private NewDineInformClientGuid getSender(TcpClientInfo clientInfo) {
			return NewDineInformClients
				.Where(p => p.Value?.Client == clientInfo.Client)
				.Select(p => p.Key)
				.FirstOrDefault();
		}

		private void log(string log, Log.LogLevel level) {
			logDelegate(log, level);
			clientsStatusChange();
		}
		private void clientsStatusChange() {
			TcpServerStatusProtocol protocol = new TcpServerStatusProtocol();

			foreach(var p in WaitingForVerificationClients) {
				protocol.WaitingForVerificationClients.Add(getClientStatus(p));
			}
			foreach(var pair in NewDineInformClients) {
				protocol.NewDineInformClients.Add(new NewDineInformClientStatus {
					Guid = pair.Key.Guid,
					Description = pair.Key.Description,
					Status = getClientStatus(pair.Value)
				});
			}
			foreach(var pair in PrinterClients) {
				protocol.PrinterClients.Add(new PrinterClientStatus {
					HotelId = pair.Key,
					WaitedCount = printerWaitedQueue[pair.Key].Count,
					Status = getClientStatus(pair.Value)
				});
			}

			clientsStatusDelegate(protocol);
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
