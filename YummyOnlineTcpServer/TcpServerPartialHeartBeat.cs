using Newtonsoft.Json;
using Protocol;
using System.Linq;

namespace YummyOnlineTcpServer {
	public partial class TcpServer {
		private void heartBeat(TcpClientInfo clientInfo) {
			foreach(var pair in printerClients.Where(p => p.Value != null)) {
				if(pair.Value.Client == clientInfo.Client) {
					pair.Value.HeartAlive = 0;
					return;
				}
			}
			foreach(var pair in newDineInformClients.Where(p => p.Value != null)) {
				if(pair.Value.Client == clientInfo.Client) {
					pair.Value.HeartAlive = 0;
					return;
				}
			}
		}

		private void sendHeartBeat(TcpClientInfo clientInfo) {
			var _ = tcp.Send(clientInfo.Client, JsonConvert.SerializeObject(new HeartBeatProtocol()), null);
		}
	}
}
