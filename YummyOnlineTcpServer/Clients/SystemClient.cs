using Protocol;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;

namespace YummyOnlineTcpServer {
	public class SystemClient : BaseClients {
		public SystemClient(Action<string, Log.LogLevel> log, Action<TcpClient, object> send, Func<TcpServerStatusProtocol> getTcpServerStatus)
			: base(log, send) {
			this.getTcpServerStatus = getTcpServerStatus;
		}

		public TcpClientInfo ClientInfo {
			get;
			private set;
		}

		private Func<TcpServerStatusProtocol> getTcpServerStatus;

		public override void HandleTimeOut() {
			if(ClientInfo != null) {
				send(ClientInfo.Client, new HeartBeatProtocol());
				ClientInfo.HeartAlive++;
				if(ClientInfo.HeartAlive > 6) {
					log($"System {ClientInfo.OriginalRemotePoint} HeartAlive Timeout", Log.LogLevel.Error);
					ClientInfo.Close();
				}
			}
		}
		public override void HandleError(TcpClient client, Exception e) {
			if(ClientInfo?.Client == client) {
				ClientInfo = null;
				log($"System {ClientInfo.OriginalRemotePoint} Disconnected", Log.LogLevel.Error);
				return;
			}
		}

		public void ClientConnected(TcpClientInfo clientInfo) {
			log($"{clientInfo.OriginalRemotePoint} (System) Connected", Log.LogLevel.Success);
			ClientInfo = clientInfo;
		}

		public void HeartBeat(TcpClientInfo clientInfo) {
			if(ClientInfo == clientInfo) {
				ClientInfo.HeartAlive = 0;
			}
		}

		public void SystemCommand(TcpClientInfo clientInfo, SystemCommandProtocol protocol, NewDineInformClients newDineInformClients) {
			if(ClientInfo != clientInfo) {
				log($"{clientInfo.OriginalRemotePoint} Received SystemCommand From Invalid SystemClient", Log.LogLevel.Error);
				clientInfo.Close();
				return;
			}

			switch(protocol.CommandType) {
				case SystemCommandType.RefreshNewDineClients:
					log($"{clientInfo.OriginalRemotePoint} Refresh NewDineInformClients", Log.LogLevel.Success);
					var _ = refreshNewDineInformClients(newDineInformClients);
					break;
				case SystemCommandType.RequestTcpServerStatus:
					log($"{clientInfo.OriginalRemotePoint} Request TcpServerStatus", Log.LogLevel.Success);
					requestTcpServerStatus();
					break;
			}
		}
		private async Task refreshNewDineInformClients(NewDineInformClients newDineInformClients) {
			List<NewDineInformClientGuid> guids = await new YummyOnlineManager().GetGuids();
			newDineInformClients.RefreshClients(guids);
		}

		private void requestTcpServerStatus() {
			TcpServerStatusProtocol tcpServerStatus = getTcpServerStatus();
			send(ClientInfo.Client, new TcpServerStatusInformProtocol(tcpServerStatus));
		}
	}
}
