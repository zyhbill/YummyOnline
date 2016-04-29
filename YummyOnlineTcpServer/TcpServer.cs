using AsynchronousTcp;
using Newtonsoft.Json;
using Protocal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;
using System.IO;

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
		}

		private TcpManager tcp;
		private Action<string, Log.LogLevel> logDelegate;
		private Action<TcpServerStatusProtocal> clientsStatusDelegate;

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
		public Dictionary<int, TcpClientInfo> PrintDineClients { get; set; } = new Dictionary<int, TcpClientInfo>();

		private Dictionary<int, Queue<PrintDineProtocal>> printDineWaitedQueue = new Dictionary<int, Queue<PrintDineProtocal>>();

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="logDelegate">记录日志回调函数, 参数1: 日志信息, 参数2: 日志等级</param>
		/// <param name="clientsStatusDelegate">socket客户端状态变化回调函数</param>
		public TcpServer(Action<string, Log.LogLevel> logDelegate, Action<TcpServerStatusProtocal> clientsStatusDelegate) {
			tcp = new TcpManager();
			tcp.MessageReceivedEvent += Tcp_MessageReceivedEvent;
			tcp.ErrorEvent += Tcp_ErrorEvent;
			this.logDelegate = logDelegate;
			this.clientsStatusDelegate = clientsStatusDelegate;
		}
		public async Task Initialize() {
			YummyOnlineManager manager = new YummyOnlineManager();
			SystemConfig config = await manager.GetSystemConfig();

			List<Hotel> hotels = await manager.GetHotels();
			hotels.ForEach(h => {
				PrintDineClients.Add(h.Id, null);
				printDineWaitedQueue.Add(h.Id, new Queue<PrintDineProtocal>());
			});
			List<NewDineInformClientGuid> guids = await manager.GetGuids();
			guids.ForEach(g => {
				NewDineInformClients.Add(g, null);
			});

			log($"Binding {config.TcpServerIp}:{config.TcpServerPort}", Log.LogLevel.Info);

			tcp.StartListening(IPAddress.Parse(config.TcpServerIp), config.TcpServerPort, client => {
				TcpClientInfo clientInfo = new TcpClientInfo(client);
				lock(WaitingForVerificationClients) {
					WaitingForVerificationClients.Add(clientInfo);
				}
				log($"{clientInfo.OriginalRemotePoint} Connected, Waiting for verification", Log.LogLevel.Info);
			});

			// 30秒之内断开已连接但是未发送身份信息的socket
			System.Timers.Timer timer = new System.Timers.Timer(10 * 1000);
			timer.Elapsed += (e, o) => {
				List<TcpClientInfo> clientInfos = WaitingForVerificationClients.FindAll(p => (DateTime.Now - p.ConnectedTime).Seconds >= 30);
				clientInfos.ForEach(c => {
					log($"{c.OriginalRemotePoint} Timeout", Log.LogLevel.Warning);
					c.Client.Close();
				});
			};
			timer.Start();
		}


		private void Tcp_MessageReceivedEvent(TcpClient client, string content) {
			try {
				BaseTcpProtocal baseProtocal = JsonConvert.DeserializeObject<BaseTcpProtocal>(content);
				TcpClientInfo clientInfo = new TcpClientInfo(client);

				switch(baseProtocal.Type) {
					case TcpProtocalType.NewDineInformClientConnect:
						newDineInfromClientConnected(clientInfo, JsonConvert.DeserializeObject<NewDineInformClientConnectProtocal>(content));
						break;
					case TcpProtocalType.PrintDineClientConnect:
						printDineClientConnected(clientInfo, JsonConvert.DeserializeObject<PrintDineClientConnectProtocal>(content));
						break;
					case TcpProtocalType.NewDineInform:
						newDineInform(clientInfo, JsonConvert.DeserializeObject<NewDineInformProtocal>(content));
						break;
					case TcpProtocalType.RequestPrintDine:
						requestPrintDine(clientInfo, JsonConvert.DeserializeObject<RequestPrintDineProtocal>(content));
						break;
				}
			}
			catch(Exception e) {
				log($"{client.Client.RemoteEndPoint} Receive Error: {e.Message} {content}", Log.LogLevel.Error);
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

			foreach(KeyValuePair<NewDineInformClientGuid, TcpClientInfo> pair in NewDineInformClients) {
				if(pair.Value?.Client == client) {
					NewDineInformClients[pair.Key] = null;
					log($"NewDineInformClient ({pair.Key.Description}) {pair.Value.OriginalRemotePoint} Disconnected", Log.LogLevel.Error);
					return;
				}
			}

			foreach(var pair in PrintDineClients) {
				if(pair.Value?.Client == client) {
					PrintDineClients[pair.Key] = null;
					log($"Printer (Hotel{pair.Key}) {pair.Value.OriginalRemotePoint} Disconnected", Log.LogLevel.Error);
					return;
				}
			}
		}

		/// <summary>
		/// 需要及时收到新订单的客户端连接
		/// </summary>
		private void newDineInfromClientConnected(TcpClientInfo clientInfo, NewDineInformClientConnectProtocal protocal) {
			log($"{clientInfo.OriginalRemotePoint} (NewDineInformClient): Guid: {protocal.Guid} Request Connection", Log.LogLevel.Info);

			if(protocal.Guid == new Guid()) {
				log($"{clientInfo.OriginalRemotePoint} NewDineInformClient Lack Guid", Log.LogLevel.Warning);
				clientInfo.Client.Client.Close();
				return;
			}

			KeyValuePair<NewDineInformClientGuid, TcpClientInfo> pair = NewDineInformClients.FirstOrDefault(p => p.Key.Guid == protocal.Guid);
			if(pair.Key == null) {
				log($"{clientInfo.OriginalRemotePoint} NewDineInformClient Guid {protocal.Guid} Not Matched", Log.LogLevel.Warning);
				clientInfo.Client.Client.Close();
				return;
			}
			else {
				if(pair.Value != null) {
					log($"NewDineInformClient Guid {pair.Key.Guid} Repeated", Log.LogLevel.Warning);
					pair.Value.Client.Client.Close();
				}

				NewDineInformClients[pair.Key] = clientInfo;

				log($"{clientInfo.OriginalRemotePoint} ({pair.Key.Description}) Connected", Log.LogLevel.Success);
			}

			_clientVerified(clientInfo);
		}

		/// <summary>
		/// 饭店打印机连接
		/// </summary>
		private void printDineClientConnected(TcpClientInfo clientInfo, PrintDineClientConnectProtocal protocal) {
			log($"{clientInfo.OriginalRemotePoint} (Printer): HotelId: {protocal.HotelId} Request Connection", Log.LogLevel.Info);

			if(!PrintDineClients.ContainsKey(protocal.HotelId)) {
				log($"{clientInfo.OriginalRemotePoint} Printer HotelId {protocal.HotelId} Not Matched", Log.LogLevel.Warning);
				clientInfo.Client.Client.Close();
				return;
			}

			KeyValuePair<int, TcpClientInfo> pair = PrintDineClients.FirstOrDefault(p => p.Key == protocal.HotelId);

			if(pair.Value != null) {
				log($"Printer HotelId {pair.Key} Repeated", Log.LogLevel.Warning);
				pair.Value.Client.Client.Close();
			}

			PrintDineClients[pair.Key] = clientInfo;

			log($"{clientInfo.OriginalRemotePoint} (Printer of Hotel {protocal.HotelId}) Connected", Log.LogLevel.Success);

			_clientVerified(PrintDineClients[pair.Key]);

			while(printDineWaitedQueue[pair.Key].Count > 0) {
				sendPrintDineProtocal(pair.Key, printDineWaitedQueue[pair.Key].Dequeue());
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
		/// <param name="protocal"></param>
		private void newDineInform(TcpClientInfo clientInfo, NewDineInformProtocal protocal) {
			NewDineInformClientGuid sender = NewDineInformClients
				.Where(p => p.Value?.Client == clientInfo.Client)
				.Select(p => p.Key)
				.FirstOrDefault();

			if(sender == null) {
				log($"{clientInfo.OriginalRemotePoint} (NewDineInform): HotelId: {protocal.HotelId}, DineId: {protocal.DineId}, IsPaid: {protocal.IsPaid} No Such Client", Log.LogLevel.Warning);
				return;
			}
			log($"{clientInfo.OriginalRemotePoint} (NewDineInform): From: {sender.Description}, HotelId: {protocal.HotelId}, DineId: {protocal.DineId}, IsPaid: {protocal.IsPaid}", Log.LogLevel.Success);

			foreach(var p in NewDineInformClients) {
				// 不向未连接的客户端与发送方客户端 发送新订单通知信息
				if(p.Value == null || p.Value.Client == clientInfo.Client)
					continue;

				string content = JsonConvert.SerializeObject(protocal);
				var _ = tcp.Send(p.Value.Client, content, null);
			}
		}

		/// <summary>
		/// 请求打印
		/// </summary>
		private void requestPrintDine(TcpClientInfo clientInfo, RequestPrintDineProtocal protocal) {
			log($"{clientInfo.OriginalRemotePoint} (RequestPrintDine): HotelId: {protocal.HotelId}, DineId: {protocal.DineId}", Log.LogLevel.Success);

			PrintDineProtocal p = new PrintDineProtocal(protocal.DineId, protocal.PrintTypes);
			if(PrintDineClients[protocal.HotelId] == null) {
				printDineWaitedQueue[protocal.HotelId].Enqueue(p);
				log($"Printer of Hotel {protocal.HotelId} is not connected", Log.LogLevel.Error);
				return;
			}
			sendPrintDineProtocal(protocal.HotelId, p);
		}
		/// <summary>
		/// 向饭店打印机发送打印协议
		/// </summary>
		private void sendPrintDineProtocal(int hotelId, PrintDineProtocal protocal) {
			var _ = tcp.Send(PrintDineClients[hotelId].Client, JsonConvert.SerializeObject(protocal, new JsonSerializerSettings {
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			}), null);
		}


		private void log(string log, Log.LogLevel level) {
			logDelegate(log, level);
			clientsStatusChange();
		}
		private void clientsStatusChange() {
			TcpServerStatusProtocal protocal = new TcpServerStatusProtocal();

			foreach(var p in WaitingForVerificationClients) {
				protocal.WaitingForVerificationClients.Add(getClientStatus(p));
			}
			foreach(var pair in NewDineInformClients) {
				protocal.NewDineInformClients.Add(new NewDineInformClientStatus {
					Guid = pair.Key.Guid,
					Description = pair.Key.Description,
					Status = getClientStatus(pair.Value)
				});
			}
			foreach(var pair in PrintDineClients) {
				protocal.PrintDineClients.Add(new PrintDineClientStatus {
					HotelId = pair.Key,
					WaitedCount = printDineWaitedQueue[pair.Key].Count,
					Status = getClientStatus(pair.Value)
				});
			}

			clientsStatusDelegate(protocal);
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
