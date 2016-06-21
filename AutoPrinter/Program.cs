using HotelDAO.Models;
using Newtonsoft.Json;
using Protocal;
using Protocal.DineForPrintingProtocal;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utility;
using YummyOnlineTcpClient;

namespace AutoPrinter {
	class Program {
		private static int hotelId = Convert.ToInt32(ConfigurationManager.AppSettings["HotelId"]);
		private static string tcpServerIp = ConfigurationManager.AppSettings["TcpServerIp"].ToString();
		private static int tcpServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["TcpServerPort"]);

		static void Main(string[] args) {
			Console.Title = "YummyOnline自助打印";
			Console.WriteLine($"版本: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
			Process[] tProcess = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
			if(tProcess.Length > 1) {
				Console.WriteLine("打印程序已经打开，请关闭后再打开打印程序。");
				Console.Read();
				return;
			}

			TcpClient tcp = new TcpClient(
				IPAddress.Parse(tcpServerIp),
				tcpServerPort,
				new PrintDineClientConnectProtocal(hotelId)
			);
			tcp.CallBackWhenMessageReceived = async (t, p) => {
				if(t == TcpProtocalType.PrintDine) {
					PrintDineProtocal protocal = (PrintDineProtocal)p;
					await printDine(protocal.DineId, protocal.DineMenuIds, protocal.PrintTypes);
				}

			};
			tcp.CallBackWhenConnected = () => {
				Console.WriteLine("服务器连接成功");
				log(Log.LogLevel.Success, "Printer Connected");
			};
			tcp.CallBackWhenExceptionOccured = e => {
				Console.WriteLine(e.Message);
				log(Log.LogLevel.Error, e.Message, e.ToString());
			};

			tcp.Start();

			while(true) {
				string cmd = Console.ReadLine();
				string[] cmds = cmd.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
				switch(cmds[0].ToLower()) {
					case "test":
						string printerName = "Microsoft XPS Document Writer";
						if(cmds.Length > 1) {
							printerName = cmds[1];
						}
						printTest("00000000000000", new List<PrintType> { PrintType.KitchenOrder, PrintType.Recipt, PrintType.ServeOrder }, printerName).Wait();
						break;
					case "list":
						List<string> printers = DinePrinter.ListPrinters();
						for(int i = 0; i < printers.Count; i++) {
							Console.WriteLine($"{i + 1} {printers[i]}");
						}
						break;
				}
			}
		}

		static async Task printDine(string dineId, List<int> dineMenuIds, List<PrintType> printTypes) {
			DineForPrinting dp = null;
			try {
				dp = await getDineForPrinting(dineId, dineMenuIds);
				if(dp == null) {
					Console.WriteLine("获取订单信息失败，请检查网络设置");
					return;
				}
				DinePrinter dinePrinter = new DinePrinter();
				Console.WriteLine($"正在打印 单号: {dineId}");
				StringBuilder dineMenuStr = new StringBuilder();

				dinePrinter.PrintDine(dp, printTypes);
				Console.WriteLine($"打印成功 单号: {dineId}");
				printCompleted(dineId);
			}
			catch(Exception e) {
				Console.WriteLine("无法打印, 请检查打印机设置");
				Console.WriteLine($"单号: {dineId}, 错误信息: {e}");
				log(Log.LogLevel.Error, $"DineId: {dineId}, {e.Message}", $"Data: {JsonConvert.SerializeObject(dp)}, Error: {e}");
				return;
			}
		}

		static async Task printTest(string dineId, List<PrintType> printTypes, string printerName) {
			DineForPrinting dp = null;
			try {
				Console.WriteLine($"开始测试, 打印机: {printerName}");
				dp = await getDineForPrinting(dineId);
				if(dp == null) {
					Console.WriteLine("获取订单信息失败，请检查网络设置");
					return;
				}
				dp.Dine.Desk.ReciptPrinter.Name = printerName;
				dp.Dine.Desk.ServePrinter.Name = printerName;
				foreach(var dineMenu in dp.Dine.DineMenus) {
					dineMenu.Menu.Printer.Name = printerName;
				}
				DinePrinter dinePrinter = new DinePrinter();
				Console.WriteLine($"正在打印测试单");
				dinePrinter.PrintDine(dp, printTypes);
				Console.WriteLine($"测试单打印成功");
				printCompleted(dineId);
			}
			catch(Exception e) {
				Console.WriteLine("无法打印测试单, 请检查打印机设置");
				Console.WriteLine($"错误信息: {e}");
				log(Log.LogLevel.Error, $"DineId: (test){dineId}, {e.Message}", $"Data: {JsonConvert.SerializeObject(dp)}, Error: {e}");
				return;
			}
		}
		static async Task<DineForPrinting> getDineForPrinting(string dineId, List<int> dineMenuIds = null) {
			object postData = new {
				HotelId = hotelId,
				DineId = dineId,
				DineMenuIds = dineMenuIds
			};
			string response = await HttpPost.PostAsync(ConfigurationManager.AppSettings["RemoteGetDineForPrintingUrl"].ToString(), postData);
			if(response == null)
				return null;

			DineForPrinting protocal = JsonConvert.DeserializeObject<DineForPrinting>(response);
			if(protocal.Hotel == null)
				return null;
			return protocal;
		}

		static void printCompleted(string dineId) {
			object postData = new {
				HotelId = hotelId,
				DineId = dineId
			};
			var _ = HttpPost.PostAsync(ConfigurationManager.AppSettings["RemotePrintCompletedUrl"].ToString(), postData);
		}

		static void log(Log.LogLevel level, string message, string detail = null) {
			object postData = new {
				HotelId = hotelId,
				Level = level,
				Message = message,
				Detail = detail
			};
			var _ = HttpPost.PostAsync(ConfigurationManager.AppSettings["RemoteLogUrl"].ToString(), postData);
		}
	}
}
