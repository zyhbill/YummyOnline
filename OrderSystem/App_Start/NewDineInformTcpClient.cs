using System.Net;
using System.Threading.Tasks;
using Protocol;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;
using YummyOnlineTcpClient;
using System.Collections.Generic;
using System.Configuration;
using System;

namespace OrderSystem {
	public class NewDineInformTcpClient {
		private static TcpClient client;
		public async static Task Initialize(Action callBackWhenConnected, Action<Exception> callBackWhenExceptionOccured) {
			SystemConfig config = await new YummyOnlineManager().GetSystemConfig();
			string guid = ConfigurationManager.AppSettings["NewDineInformClientGuid"];
			client = new TcpClient(IPAddress.Parse(config.TcpServerIp), config.TcpServerPort, new NewDineInformClientConnectProtocol(guid));

			client.CallBackWhenConnected = () => {
				callBackWhenConnected();
			};

			client.CallBackWhenExceptionOccured = e => {
				callBackWhenExceptionOccured(e);
			};

			client.Start();
		}
		public static void SendNewDineInfrom(int hotelId, string dineId, bool isPaid) {
			client.Send(new NewDineInformProtocol(hotelId, dineId, isPaid));
		}
		public static void SendRequestPrintDine(int hotelId, string dineId, List<PrintType> printTypes) {
			client.Send(new RequestPrintDineProtocol(hotelId, dineId, null, printTypes));
		}
	}
}