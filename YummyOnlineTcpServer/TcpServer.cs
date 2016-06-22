using AsynchronousTcp;
using Newtonsoft.Json;
using Protocal;
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
		}

		private TcpManager tcp;
		private string ip;
		private int port;
		private Action<string, Log.LogLevel> logDelegate;
		private Action<TcpServerStatusProtocal> clientsStatusDelegate;

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

		private Dictionary<int, Queue<BaseTcpProtocal>> printerWaitedQueue = new Dictionary<int, Queue<BaseTcpProtocal>>();

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="logDelegate">记录日志回调函数, 参数1: 日志信息, 参数2: 日志等级</param>
		/// <param name="clientsStatusDelegate">socket客户端状态变化回调函数</param>
		public TcpServer(string ip, int port, Action<string, Log.LogLevel> logDelegate, Action<TcpServerStatusProtocal> clientsStatusDelegate) {
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
				printerWaitedQueue.Add(h.Id, new Queue<BaseTcpProtocal>());
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
					case TcpProtocalType.RequestPrintShifts:
						requestPrintShifts(clientInfo, JsonConvert.DeserializeObject<RequestPrintShiftsProtocal>(content));
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
		private void printDineClientConnected(TcpClientInfo clientInfo, PrintDineClientConnectProtocal protocal) {
			log($"{clientInfo.OriginalRemotePoint} (Printer): HotelId: {protocal.HotelId} Request Connection", Log.LogLevel.Info);

			if(!PrinterClients.ContainsKey(protocal.HotelId)) {
				log($"{clientInfo.OriginalRemotePoint} Printer HotelId {protocal.HotelId} Not Matched", Log.LogLevel.Warning);
				clientInfo.Client.Client.Close();
				return;
			}

			KeyValuePair<int, TcpClientInfo> pair = PrinterClients.FirstOrDefault(p => p.Key == protocal.HotelId);

			if(pair.Value != null) {
				log($"Printer HotelId {pair.Key} Repeated", Log.LogLevel.Warning);
				pair.Value.Client.Client.Close();

				clientCloseMutex.Reset();
				clientCloseMutex.WaitOne();
			}

			PrinterClients[pair.Key] = clientInfo;

			log($"{clientInfo.OriginalRemotePoint} (Printer of Hotel {protocal.HotelId}) Connected", Log.LogLevel.Success);

			_clientVerified(PrinterClients[pair.Key]);

			// 打印存储在打印等待队列中的所有请求
			while(printerWaitedQueue[pair.Key].Count > 0) {
				BaseTcpProtocal printProtocal = printerWaitedQueue[pair.Key].Dequeue();
				if(printProtocal.Type == TcpProtocalType.PrintDine) {
					sendPrintDineProtocal(pair.Key, (PrintDineProtocal)printProtocal);
				}
				else if(printProtocal.Type == TcpProtocalType.PrintShifts) {
					sendPrintShiftsProtocal(pair.Key, (PrintShiftsProtocal)printProtocal);
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
			protocal.DineMenuIds = protocal.DineMenuIds ?? new List<int>();
			protocal.PrintTypes = protocal.PrintTypes ?? new List<PrintType>();

			StringBuilder dineMenuStr = new StringBuilder();
			for(int i = 0; i < protocal.DineMenuIds.Count; i++) {
				dineMenuStr.Append(protocal.DineMenuIds[i]);
				if(i != protocal.DineMenuIds.Count - 1)
					dineMenuStr.Append(' ');
			}

			StringBuilder typeStr = new StringBuilder();
			foreach(var type in protocal.PrintTypes) {
				typeStr.Append($"{type.ToString()} ");
			}

			log($"{clientInfo.OriginalRemotePoint} (RequestPrintDine): HotelId: {protocal.HotelId}, DineId: {protocal.DineId}, DineMenuIds: {dineMenuStr}, PrintTypes: {typeStr}",
				Log.LogLevel.Success);

			PrintDineProtocal p = new PrintDineProtocal(protocal.DineId, protocal.DineMenuIds, protocal.PrintTypes);
			if(PrinterClients[protocal.HotelId] == null) {
				printerWaitedQueue[protocal.HotelId].Enqueue(p);
				log($"Printer of Hotel {protocal.HotelId} is not connected", Log.LogLevel.Error);
				return;
			}
			sendPrintDineProtocal(protocal.HotelId, p);
		}

		/// <summary>
		/// 向饭店打印机发送打印协议
		/// </summary>
		private void sendPrintDineProtocal(int hotelId, PrintDineProtocal protocal) {
			var _ = tcp.Send(PrinterClients[hotelId].Client, JsonConvert.SerializeObject(protocal, new JsonSerializerSettings {
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			}), null);
		}

		private void requestPrintShifts(TcpClientInfo clientInfo, RequestPrintShiftsProtocal protocal) {
			protocal.Ids = protocal.Ids ?? new List<int>();

			StringBuilder idStr = new StringBuilder();
			foreach(int id in protocal.Ids) {
				idStr.Append($"{id} ");
			}

			log($"{clientInfo.OriginalRemotePoint} (RequestPrintShifts): HotelId: {protocal.HotelId}, Ids: {idStr}, DateTime: {protocal.DateTime.ToString("yyyy-MM-dd")}",
				Log.LogLevel.Success);

			PrintShiftsProtocal p = new PrintShiftsProtocal(protocal.Ids, protocal.DateTime);
			if(PrinterClients[protocal.HotelId] == null) {
				printerWaitedQueue[protocal.HotelId].Enqueue(p);
				log($"Printer of Hotel {protocal.HotelId} is not connected", Log.LogLevel.Error);
				return;
			}
			sendPrintShiftsProtocal(protocal.HotelId, p);
		}
		private void sendPrintShiftsProtocal(int hotelId, PrintShiftsProtocal protocal) {
			var _ = tcp.Send(PrinterClients[hotelId].Client, JsonConvert.SerializeObject(protocal, new JsonSerializerSettings {
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
			foreach(var pair in PrinterClients) {
				protocal.PrinterClients.Add(new PrinterClientStatus {
					HotelId = pair.Key,
					WaitedCount = printerWaitedQueue[pair.Key].Count,
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
