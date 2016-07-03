using Protocol;
using System;
using System.Net;
using System.Threading.Tasks;
using YummyOnlineDAO.Models;
using YummyOnlineTcpClient;

namespace YummyOnline {
	public class SystemTcpClient {
		private static TcpClient client;
		public async static Task Initialize(Action callBackWhenConnected, Action<Exception> callBackWhenExceptionOccured) {
			SystemConfig config = await new YummyOnlineManager().GetSystemConfig();
			client = new TcpClient(IPAddress.Parse(config.TcpServerIp), config.TcpServerPort, new SystemConnectProtocol());

			client.CallBackWhenConnected = () => {
				callBackWhenConnected();
			};

			client.CallBackWhenExceptionOccured = e => {
				callBackWhenExceptionOccured(e);
			};

			client.Start();
		}
		public static void SendSystemCommand(SystemCommandType commandType) {
			client.Send(new SystemCommandProtocol(commandType));
		}
	}
}