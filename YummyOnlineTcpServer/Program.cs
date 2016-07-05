using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;
using Newtonsoft.Json;
using Protocol;
using System.Threading;

namespace YummyOnlineTcpServer {
	class Program {

		static void Main(string[] args) {
			Process[] tProcess = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
			if(tProcess.Length > 1) {
				return;
			}

			SystemConfig config = new YummyOnlineManager().GetSystemConfig().Result;
			TcpServer tcp = new TcpServer(config.TcpServerIp, config.TcpServerPort, async (log, level) => {
				await new YummyOnlineManager().RecordLog(Log.LogProgram.TcpServer, level, log);
			});
			Task _ = tcp.Initialize();


			WebSocketServer webSocket = new WebSocketServer(config.TcpServerIp, config.WebSocketPort, async (log, level) => {
				await new YummyOnlineManager().RecordLog(Log.LogProgram.System, level, log);
			});

			while(true) {
				string cmd = Console.ReadLine();
				displayTcpServerStatus(tcp.GetTcpServerStatus());
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
