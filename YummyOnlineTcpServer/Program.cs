using Protocol;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;

namespace YummyOnlineTcpServer {
	class Program {

		static void Main(string[] args) {
			Process[] tProcess = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
			if(tProcess.Length > 1) {
				return;
			}

			SystemConfig config = new YummyOnlineManager().GetSystemConfig().Result;
			TcpServer tcpServer = new TcpServer(config.TcpServerIp, config.TcpServerPort, async (log, level) => {
				await new YummyOnlineManager().RecordLog(Log.LogProgram.TcpServer, level, log);
			});
			Task _ = tcpServer.Initialize();


			WebSocketServer webSocket = new WebSocketServer(config.TcpServerIp, config.WebSocketPort, async (log, level) => {
				await new YummyOnlineManager().RecordLog(Log.LogProgram.System, level, log);
			});

			System.Timers.Timer t = new System.Timers.Timer(500);
			t.Elapsed += (e, o) => {
				Console.Clear();
				displayTcpServerStatus(tcpServer.GetTcpServerStatus());
			};
			
			bool isTimerStarted = false;
			while(true) {
				Console.ReadLine();
				if(isTimerStarted)
					t.Close();
				else
					t.Start();
				isTimerStarted = !isTimerStarted;
			}
		}

		static void displayTcpServerStatus(TcpServerStatusProtocol status) {
			Console.WriteLine("WaitingForVerificationClients:");
			foreach(var p in status.WaitingForVerificationClients) {
				displayClientStatus(p);
			}
			Console.WriteLine("========================");
			Console.WriteLine("SystemClient:");
			if(status.SystemClient.IsConnected) {
				displayClientStatus(status.SystemClient);
			}
			else {
				Console.WriteLine("Disconnected");
			}
			Console.WriteLine("========================");
			Console.WriteLine("NewDineInformClients:");
			foreach(var p in status.NewDineInformClients) {
				Console.WriteLine("---------------------");
				string guid = p.Guid.ToString();
				Console.WriteLine($"Guid: {guid.Substring(0, 4)}...{guid.Substring(guid.Length - 4, 4)}, Description: {p.Description}");
				if(p.Status.IsConnected) {
					displayClientStatus(p.Status);
				}
				else {
					Console.WriteLine("Disconnected");
				}
			}
			Console.WriteLine("========================");
			foreach(var p in status.PrinterClients) {
				Console.Write($"Hotel {p.HotelId}: ");
				if(p.Status.IsConnected) {
					displayClientStatus(p.Status);
					Console.WriteLine($"\t({p.WaitedCount} standing by)");
				}
				else {
					Console.WriteLine("Disconnected");
				}
			}
		}

		static void displayClientStatus(ClientStatus status) {
			Console.WriteLine($"{status.IpAddress}:{status.Port} [{status.ConnectedTime}]");
		}
	}
}
