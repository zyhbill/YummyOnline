using Newtonsoft.Json;
using Protocol;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;

namespace YummyOnlineTcpServer {
	public partial class TcpServer {
		private void systemCommand(TcpClientInfo clientInfo, SystemCommandProtocol protocol) {
			if(clientInfo.Client != systemClient.Client) {
				log($"{clientInfo.OriginalRemotePoint} Received SystemCommand From Invalid SystemClient", Log.LogLevel.Error);
				clientInfo.Client.Close();
				return;
			}

			switch(protocol.CommandType) {
				case SystemCommandType.RefreshNewDineClients:
					log($"{clientInfo.OriginalRemotePoint} Refresh NewDineInformClients", Log.LogLevel.Success);
					var _ = refreshNewDineInformClients();
					break;
			}
		}
		private async Task refreshNewDineInformClients() {
			List<NewDineInformClientGuid> guids = await new YummyOnlineManager().GetGuids();

			guids.ForEach(newGuid => {
				if(!newDineInformClients.Keys.ToList().Exists(p => p.Guid == newGuid.Guid)) {
					newDineInformClients.Add(newGuid, null);
				}
			});

			for(int i = newDineInformClients.Keys.Count - 1; i >= 0; i--) {
				var oldGuid = newDineInformClients.ElementAt(i);

				if(!guids.Exists(p => p.Guid == oldGuid.Key.Guid)) {
					oldGuid.Value?.Client.Close();
					newDineInformClients.Remove(oldGuid.Key);
				}
			}

			clientsStatusChange();
		}
	}
}
