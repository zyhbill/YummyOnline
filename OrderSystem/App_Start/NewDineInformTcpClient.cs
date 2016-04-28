using System.Net;
using System.Threading.Tasks;
using Protocal;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;
using YummyOnlineTcpClient;
using System.Collections.Generic;
using System.Configuration;

namespace OrderSystem {
	public class NewDineInformTcpClient {
		private static TcpClient client;
		public async static Task Initialize() {
			YummyOnlineManager manager = new YummyOnlineManager();
			SystemConfig config = await manager.GetSystemConfig();
			string guid = ConfigurationManager.AppSettings["NewDineInformClientGuid"];
			client = new TcpClient(IPAddress.Parse(config.TcpServerIp), config.TcpServerPort, new NewDineInformClientConnectProtocal(guid));

			client.CallBackWhenConnected = async () => {
				await manager.RecordLog(Log.LogProgram.OrderSystem, Log.LogLevel.Success, "TcpServer Connected");
			};

			client.CallBackWhenExceptionOccured = async e => {
				await manager.RecordLog(Log.LogProgram.OrderSystem, Log.LogLevel.Error, e.Message);
			};

			client.Start();
		}
		public static void SendNewDineInfrom(int hotelId, string dineId, bool isPaid) {
			client.Send(new NewDineInformProtocal(hotelId, dineId, isPaid));
		}
		public static void SendRequestPrintDine(int hotelId, string dineId) {
			client.Send(new RequestPrintDineProtocal(hotelId, dineId, new List<PrintType>() { PrintType.Recipt, PrintType.KitchenOrder, PrintType.ServeOrder }));
		}
	}
}