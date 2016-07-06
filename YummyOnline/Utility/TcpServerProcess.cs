using Newtonsoft.Json;
using Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;

namespace YummyOnline.Utility {
	public static class TcpServerProcess {
		private static string processName = "YummyOnlineTcpServer";
		private static PerformanceCounter curtime = new PerformanceCounter("Process", "% Processor Time", processName);
		private static PerformanceCounter curpcp = new PerformanceCounter("Process", "Working Set - Private", processName);

		private static Process getTcpServerProcess() {
			Process[] processes = Process.GetProcessesByName(processName);
			if(processes.Length == 0) {
				return null;
			}
			return processes[0];
		}


		public static async Task<bool> StartTcpServer() {
			Process process = getTcpServerProcess();
			if(process != null) {
				return false;
			}

			try {
				SystemConfig config = await new YummyOnlineManager().GetSystemConfig();
				process = new Process();
				process.StartInfo.FileName = config.TcpServerDir + @"\YummyOnlineTcpServer.exe";
				process.StartInfo.CreateNoWindow = true;
				process.Start();
				return true;
			}
			catch {
				return false;
			}

		}

		public static bool StopTcpServer() {
			Process process = getTcpServerProcess();
			if(process == null) {
				return false;
			}

			process.Kill();
			return true;
		}

		public static dynamic GetTcpServerInfo() {
			Process process = getTcpServerProcess();
			if(process == null) {
				return new {
					IsStarted = false,
				};
			}

			TcpServerStatusProtocol status = SystemTcpClient.GetTcpServerStatus();

			return new {
				IsStarted = true,
				ProcessName = process.ProcessName,
				StartTime = process.StartTime,
				Memory = curpcp.NextValue() / (1024 * 1024),
				Cpu = curtime.NextValue() / Environment.ProcessorCount,
				Status = status
			};
		}
	}
}