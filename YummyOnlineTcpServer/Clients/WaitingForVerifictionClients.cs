using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using YummyOnlineDAO.Models;

namespace YummyOnlineTcpServer {
	/// <summary>
	/// 等待验证客户端
	/// </summary>
	public class WaitingForVerificationClients : BaseClients {
		public WaitingForVerificationClients(Action<string, Log.LogLevel> log, Action<TcpClient, object> send)
			: base(log, send) { }

		public List<TcpClientInfo> Clients { get; } = new List<TcpClientInfo>();


		public void Add(TcpClientInfo clientInfo) {
			lock(this) {
				Clients.Add(clientInfo);
			}
		}

		public override void HandleTimeOut() {
			lock(this) {
				foreach(var client in Clients) {
					client.HeartAlive++;
					if(client.HeartAlive > 3) {
						log($"{client.OriginalRemotePoint} Timeout", Log.LogLevel.Warning);
						client.Close();
					}
				}
			}
		}
		public override void HandleError(TcpClient client, Exception e) {
			lock(this) {
				TcpClientInfo clientInfo = Clients.FirstOrDefault(p => p.Client == client);
				if(clientInfo != null) {
					Clients.Remove(clientInfo);
					log($"WaitingForVerificationClient {clientInfo.OriginalRemotePoint} Disconnected", Log.LogLevel.Error);
					return;
				}
			}
		}

		public void ClientConnected(TcpClientInfo clientInfo) {
			lock(this) {
				TcpClientInfo waitedClientInfo = Clients.FirstOrDefault(p => p == clientInfo);
				clientInfo.ConnectedTime = waitedClientInfo.ConnectedTime;
				Clients.Remove(waitedClientInfo);
			}
		}
	}
}
