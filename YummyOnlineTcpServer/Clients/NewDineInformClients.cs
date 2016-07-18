using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using YummyOnlineDAO.Models;

namespace YummyOnlineTcpServer {
	/// <summary>
	/// 新订单通知客户端
	/// </summary>
	public class NewDineInformClients : BaseClients {
		public NewDineInformClients(Action<string, Log.LogLevel> log, Action<TcpClient, object> send,
			List<NewDineInformClientGuid> guids)
			: base(log, send) {
			lock(this) {
				guids.ForEach(g => {
					Clients.Add(g, null);
				});
			}
		}

		public Dictionary<NewDineInformClientGuid, TcpClientInfo> Clients { get; } = new Dictionary<NewDineInformClientGuid, TcpClientInfo>();

		public override void HandleTimeOut() {
			lock(this) {
				foreach(var pair in Clients.Where(p => p.Value != null)) {
					send(pair.Value.Client, new HeartBeatProtocol());
					pair.Value.HeartAlive++;
					if(pair.Value.HeartAlive > 1) {
						log($"({pair.Key.Description}) {pair.Value.OriginalRemotePoint} HeartAlive Timeout", Log.LogLevel.Error);
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
						log($"NewDineInformClient ({pair.Key.Description}) {pair.Value.OriginalRemotePoint} Disconnected", Log.LogLevel.Error);
						return;
					}
				}
			}
		}

		public void ClientConnected(TcpClientInfo clientInfo, NewDineInformClientConnectProtocol protocol,
				WaitingForVerificationClients waitingForVerificationClients) {
			lock(this) {
				log($"{clientInfo.OriginalRemotePoint} (NewDineInformClient): Guid: {protocol.Guid} Request Connection", Log.LogLevel.Info);

				if(protocol.Guid == new Guid()) {
					log($"{clientInfo.OriginalRemotePoint} NewDineInformClient Lack Guid", Log.LogLevel.Warning);
					clientInfo.Close();
					return;
				}

				KeyValuePair<NewDineInformClientGuid, TcpClientInfo> pair = Clients.FirstOrDefault(p => p.Key.Guid == protocol.Guid);
				if(pair.Key == null) {
					log($"{clientInfo.OriginalRemotePoint} NewDineInformClient Guid {protocol.Guid} Not Matched", Log.LogLevel.Warning);
					clientInfo.Close();
					return;
				}
				else {
					if(pair.Value != null) {
						log($"NewDineInformClient Guid {pair.Key.Guid} Repeated", Log.LogLevel.Warning);
						pair.Value.ReadyToReplaceClient = clientInfo;
						pair.Value.Close();
					}
					else {
						Clients[pair.Key] = clientInfo;
					}

					log($"{clientInfo.OriginalRemotePoint} ({pair.Key.Description}) Connected", Log.LogLevel.Success);
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

		public void NewDineInform(TcpClientInfo clientInfo, NewDineInformProtocol protocol) {
			lock(this) {
				NewDineInformClientGuid sender = GetSender(clientInfo);
				if(sender == null) {
					log($"{clientInfo.OriginalRemotePoint} Received NewDineInform From Invalid NewDineInformClient", Log.LogLevel.Error);
					clientInfo.Close();
					return;
				}

				log($"{clientInfo.OriginalRemotePoint} (NewDineInform): From: {sender.Description}, HotelId: {protocol.HotelId}, DineId: {protocol.DineId}, IsPaid: {protocol.IsPaid}", Log.LogLevel.Success);

				foreach(var p in Clients) {
					// 不向未连接的客户端与发送方客户端 发送新订单通知信息
					if(p.Value == null || p.Value.Client == clientInfo.Client)
						continue;

					send(p.Value.Client, protocol);
				}
			}
		}

		public void RefreshClients(List<NewDineInformClientGuid> guids) {
			lock(this) {
				guids.ForEach(newGuid => {
					if(!Clients.Keys.ToList().Exists(p => p.Guid == newGuid.Guid)) {
						Clients.Add(newGuid, null);
					}
				});

				for(int i = Clients.Keys.Count - 1; i >= 0; i--) {
					var oldGuid = Clients.ElementAt(i);

					if(!guids.Exists(p => p.Guid == oldGuid.Key.Guid)) {
						oldGuid.Value?.Close();
						Clients.Remove(oldGuid.Key);
					}
				}
			}
		}

		public NewDineInformClientGuid GetSender(TcpClientInfo clientInfo) {
			lock(this) {
				return Clients
				.Where(p => p.Value?.Client == clientInfo.Client)
				.Select(p => p.Key)
				.FirstOrDefault();
			}
		}
	}
}
