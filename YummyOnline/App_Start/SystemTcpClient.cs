using Protocol;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YummyOnlineDAO.Models;
using YummyOnlineTcpClient;

namespace YummyOnline {
	public class SystemTcpClient {
		private static TcpClient client;

		private static ManualResetEvent getTcpServerStatusFinished = new ManualResetEvent(true);
		private static TcpServerStatusProtocol tcpServerStatusProtocol = null;

		public async static Task Initialize(Action callBackWhenConnected, Action<Exception> callBackWhenExceptionOccured) {
			SystemConfig config = await new YummyOnlineManager().GetSystemConfig();
			client = new TcpClient(IPAddress.Parse(config.TcpServerIp), config.TcpServerPort, new SystemConnectProtocol());

			client.CallBackWhenMessageReceived = (t, p) => {
				if(t == TcpProtocolType.TcpServerStatusInform) {
					TcpServerStatusInformProtocol protocol = (TcpServerStatusInformProtocol)p;
					tcpServerStatusProtocol = protocol.Status;
					getTcpServerStatusFinished.Set();
				}
			};

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
		public static TcpServerStatusProtocol GetTcpServerStatus() {
			client.Send(new SystemCommandProtocol(SystemCommandType.RequestTcpServerStatus));

			getTcpServerStatusFinished.Reset();

			System.Timers.Timer t = new System.Timers.Timer(5000);
			t.Elapsed += (o, e) => {
				getTcpServerStatusFinished.Set();
			};
			t.Start();
			getTcpServerStatusFinished.WaitOne();
			t.Stop();

			return tcpServerStatusProtocol;
		}
	}
}