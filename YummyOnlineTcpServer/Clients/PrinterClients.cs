using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using YummyOnlineDAO.Models;

namespace YummyOnlineTcpServer {
	/// <summary>
	/// 打印机客户端
	/// </summary>
	public class PrinterClients : BaseClients {
		public PrinterClients(Action<string, Log.LogLevel> log, Action<TcpClient, object> send,
			List<Hotel> hotels)
			: base(log, send) {
			lock(this) {
				hotels.ForEach(h => {
					Clients.Add(h.Id, null);
					WaitedQueue.Add(h.Id, new Queue<BaseTcpProtocol>());
				});
			}
		}

		public Dictionary<int, TcpClientInfo> Clients { get; } = new Dictionary<int, TcpClientInfo>();
		/// <summary>
		/// 打印等待队列
		/// </summary>
		public Dictionary<int, Queue<BaseTcpProtocol>> WaitedQueue {
			get;
			private set;
		} = new Dictionary<int, Queue<BaseTcpProtocol>>();

		public override void HandleTimeOut() {
			lock(this) {
				foreach(var pair in Clients.Where(p => p.Value != null)) {
					send(pair.Value.Client, new HeartBeatProtocol());
					pair.Value.HeartAlive++;
					if(pair.Value.HeartAlive > 6) {
						log($"Printer (Hotel{pair.Key}) {pair.Value.OriginalRemotePoint} HeartAlive Timeout", Log.LogLevel.Error);
						pair.Value.Close();
					}
				}
			}
		}
		public override void HandleError(TcpClient client, Exception e) {
			lock(this) {
				foreach(var pair in Clients) {
					if(pair.Value?.Client == client) {
						Clients[pair.Key] = Clients[pair.Key].ReadyToReplaceClient;
						log($"Printer (Hotel{pair.Key}) {pair.Value.OriginalRemotePoint} Disconnected", Log.LogLevel.Error);
						return;
					}
				}
			}
		}

		public void ClientConnected(TcpClientInfo clientInfo, PrintDineClientConnectProtocol protocol,
				WaitingForVerificationClients waitingForVerificationClients) {
			lock(this) {
				log($"{clientInfo.OriginalRemotePoint} (Printer): HotelId: {protocol.HotelId} Request Connection", Log.LogLevel.Info);

				if(!Clients.ContainsKey(protocol.HotelId)) {
					log($"{clientInfo.OriginalRemotePoint} Printer HotelId {protocol.HotelId} Not Matched", Log.LogLevel.Warning);
					clientInfo.Close();
					return;
				}

				KeyValuePair<int, TcpClientInfo> pair = Clients.FirstOrDefault(p => p.Key == protocol.HotelId);

				if(pair.Value != null) {
					log($"Printer HotelId {pair.Key} Repeated", Log.LogLevel.Warning);
					pair.Value.ReadyToReplaceClient = clientInfo;
					pair.Value.Close();
				}
				else {
					Clients[pair.Key] = clientInfo;
				}

				log($"{clientInfo.OriginalRemotePoint} (Printer of Hotel {protocol.HotelId}) Connected", Log.LogLevel.Success);

				// 打印存储在打印等待队列中的所有请求
				while(WaitedQueue[pair.Key].Count > 0) {
					BaseTcpProtocol printProtocol = WaitedQueue[pair.Key].Dequeue();
					if(printProtocol.Type == TcpProtocolType.PrintDine) {
						sendPrintDineProtocol(pair.Key, (PrintDineProtocol)printProtocol);
					}
					else if(printProtocol.Type == TcpProtocolType.PrintShifts) {
						sendPrintShiftsProtocol(pair.Key, (PrintShiftsProtocol)printProtocol);
					}
					log($"Send Waited Dine of Hotel {pair.Key}", Log.LogLevel.Success);
				}

				waitingForVerificationClients.ClientConnected(clientInfo);
			}
		}

		public void HeartBeat(TcpClientInfo clientInfo) {
			lock(this) {
				foreach(var pair in Clients) {
					if(pair.Value == clientInfo) {
						pair.Value.HeartAlive = 0;
						return;
					}
				}
			}
		}

		/// <summary>
		/// 请求打印订单
		/// </summary>
		public void RequestPrintDine(TcpClientInfo clientInfo, RequestPrintDineProtocol protocol, NewDineInformClientGuid sender) {
			lock(this) {
				if(sender == null) {
					log($"{clientInfo.OriginalRemotePoint} Received RequestPrintDine From Invalid NewDineInformClient", Log.LogLevel.Error);
					clientInfo.Close();
					return;
				}

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
				if(Clients[protocol.HotelId] == null) {
					WaitedQueue[protocol.HotelId].Enqueue(p);
					log($"Printer of Hotel {protocol.HotelId} is not connected", Log.LogLevel.Error);
					return;
				}
				sendPrintDineProtocol(protocol.HotelId, p);
			}
		}
		/// <summary>
		/// 向饭店打印机发送打印订单协议
		/// </summary>
		private void sendPrintDineProtocol(int hotelId, PrintDineProtocol protocol) {
			send(Clients[hotelId].Client, protocol);
		}

		/// <summary>
		/// 请求打印交接班
		/// </summary>
		public void RequestPrintShifts(TcpClientInfo clientInfo, RequestPrintShiftsProtocol protocol, NewDineInformClientGuid sender) {
			lock(this) {
				if(sender == null) {
					log($"{clientInfo.OriginalRemotePoint} Received RequestPrintShifts From Invalid NewDineInformClient", Log.LogLevel.Error);
					clientInfo.Close();
					return;
				}

				protocol.Ids = protocol.Ids ?? new List<int>();

				StringBuilder idStr = new StringBuilder();
				foreach(int id in protocol.Ids) {
					idStr.Append($"{id} ");
				}

				log($"{clientInfo.OriginalRemotePoint} (RequestPrintShifts): From: {sender.Description}, HotelId: {protocol.HotelId}, Ids: {idStr}, DateTime: {protocol.DateTime.ToString("yyyy-MM-dd")}",
					Log.LogLevel.Success);

				PrintShiftsProtocol p = new PrintShiftsProtocol(protocol.Ids, protocol.DateTime);
				if(Clients[protocol.HotelId] == null) {
					WaitedQueue[protocol.HotelId].Enqueue(p);
					log($"Printer of Hotel {protocol.HotelId} is not connected", Log.LogLevel.Error);
					return;
				}
				sendPrintShiftsProtocol(protocol.HotelId, p);
			}
		}
		/// <summary>
		/// 向饭店打印机发送打印交接班协议
		/// </summary>
		private void sendPrintShiftsProtocol(int hotelId, PrintShiftsProtocol protocol) {
			send(Clients[hotelId].Client, protocol);
		}
	}
}
