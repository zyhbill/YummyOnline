using HotelDAO.Models;
using Newtonsoft.Json;
using Protocol;
using Protocol.PrintingProtocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utility;
using YummyOnlineTcpClient;

namespace AutoPrinter {
	partial class Program {
		private static int hotelId = Convert.ToInt32(ConfigurationManager.AppSettings["HotelId"]);
		private static string tcpServerIp = ConfigurationManager.AppSettings["TcpServerIp"].ToString();
		private static int tcpServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["TcpServerPort"]);

		private static string remoteLogUrl = ConfigurationManager.AppSettings["RemoteLogUrl"];
		private static string remotePrintCompletedUrl = ConfigurationManager.AppSettings["RemotePrintCompletedUrl"];
		private static string remoteGetDineForPrintingUrl = ConfigurationManager.AppSettings["RemoteGetDineForPrintingUrl"];
		private static string remoteGetShiftsForPrintingUrl = ConfigurationManager.AppSettings["RemoteGetShiftsForPrintingUrl"];
		private static string remoteGetPrintersForPrintingUrl = ConfigurationManager.AppSettings["RemoteGetPrintersForPrintingUrl"];

		static void Main(string[] args) {
			Console.Title = "YummyOnline自助打印";
			Console.WriteLine($"版本: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
			Process[] tProcess = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
			if(tProcess.Length > 1) {
				localLog("打印程序已经打开，请关闭后再打开打印程序。");
				Console.Read();
				return;
			}

			if(!Directory.Exists($@"{Environment.CurrentDirectory}\failedImgs")) {
				Directory.CreateDirectory($@"{Environment.CurrentDirectory}\failedImgs");
			}

			TcpClient tcp = new TcpClient(
				IPAddress.Parse(tcpServerIp),
				tcpServerPort,
				new PrintDineClientConnectProtocol(hotelId)
			);
			tcp.CallBackWhenMessageReceived = async (t, p) => {
				if(t == TcpProtocolType.PrintDine) {
					PrintDineProtocol protocol = (PrintDineProtocol)p;
					await printDine(protocol.DineId, protocol.DineMenuIds, protocol.PrintTypes);
				}
				else if(t == TcpProtocolType.PrintShifts) {
					PrintShiftsProtocol protocol = (PrintShiftsProtocol)p;
					await printShifts(protocol.Ids, protocol.DateTime);
				}
			};
			tcp.CallBackWhenConnected = () => {
				localLog("服务器连接成功");
				log(Log.LogLevel.Success, "Printer Connected");
			};
			tcp.CallBackWhenExceptionOccured = e => {
				localLog(e.Message);
				log(Log.LogLevel.Error, e.Message, e.ToString());
			};

			tcp.Start();

			while(true) {
				string cmd = Console.ReadLine();
				string[] cmds = cmd.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
				if(cmds.Length == 0) {
					Console.WriteLine("test ip");
					Console.WriteLine("localtest ip");
					Console.WriteLine("testprinters");
					continue;
				}
				switch(cmds[0].ToLower()) {
					case "test":
						if(cmds.Length == 1) {
							Console.WriteLine("缺少IP地址参数");
							continue;
						}
						string ipAddress = cmds[1];
						printTest(new List<PrintType> { PrintType.KitchenOrder, PrintType.Recipt, PrintType.ServeOrder }, ipAddress).Wait();
						break;
					case "localtest":
						if(cmds.Length == 1) {
							Console.WriteLine("缺少IP地址参数");
							continue;
						}
						ipAddress = cmds[1];
						printLocalTest(new List<PrintType> { PrintType.Recipt }, ipAddress);
						break;
					case "testprinters":
						testPrinters().Wait();
						break;
				}
			}
		}

		static async Task printDine(string dineId, List<int> dineMenuIds, List<PrintType> printTypes) {
			DineForPrinting dp = null;
			try {
				dp = await getDineForPrinting(dineId, dineMenuIds);
				if(dp == null) {
					localLog("获取订单信息失败，请检查网络设置");
					return;
				}

				localLog($"发送打印命令 单号: {dineId}");
				DinePrinter dinePrinter = new DinePrinter((ip, e) => {
					localLog($"打印机错误, 请检查打印机设置, {ip} {e.Message}");
				});
				dinePrinter.Print(dp, printTypes, dineMenuIds == null);
				localLog($"发送命令成功 单号: {dineId}");
				// 厨房打印过订单再发送成功打印信息
				if(printTypes.Contains(PrintType.KitchenOrder)) {
					printCompleted(dineId);
				}
			}
			catch(Exception e) {
				localLog($"无法打印 单号: {dineId}, 错误信息: {e}");
				log(Log.LogLevel.Error, $"DineId: {dineId}, {e.Message}", $"Data: {JsonConvert.SerializeObject(dp)}, Error: {e}");
			}
		}
		static async Task printTest(List<PrintType> printTypes, string ipAddress) {
			DineForPrinting dp = null;
			try {
				localLog($"开始测试, 打印机: {ipAddress}");
				dp = await getDineForPrinting("00000000000000");
				if(dp == null) {
					localLog("获取订单信息失败，请检查网络设置");
					return;
				}
				dp.Dine.Desk.ReciptPrinter.IpAddress = ipAddress;
				dp.Dine.Desk.ServePrinter.IpAddress = ipAddress;
				foreach(var dineMenu in dp.Dine.DineMenus) {
					dineMenu.Menu.Printer.IpAddress = ipAddress;
				}

				localLog($"发送测试单命令");
				DinePrinter dinePrinter = new DinePrinter((ip, e) => {
					localLog($"打印机错误, 请检查打印机设置, {ip} {e.Message}");
				});
				dinePrinter.Print(dp, printTypes, true);
				localLog($"发送测试单命令成功");
				printCompleted("00000000000000");
			}
			catch(Exception e) {
				localLog($"无法打印测试单, 错误信息: {e}");
				log(Log.LogLevel.Error, $"Online Test, {e.Message}", $"Data: {JsonConvert.SerializeObject(dp)}, Error: {e}");
			}
		}

		static void printLocalTest(List<PrintType> printTypes, string ipAddress) {
			DineForPrinting dp = null;
			try {
				localLog($"开始本地测试, 打印机: {ipAddress}");
				dp = generateTestProtocol();
				dp.Dine.Desk.ReciptPrinter.IpAddress = ipAddress;
				dp.Dine.Desk.ServePrinter.IpAddress = ipAddress;
				foreach(var dineMenu in dp.Dine.DineMenus) {
					dineMenu.Menu.Printer.IpAddress = ipAddress;
				}

				localLog($"发送本地测试单命令");
				DinePrinter dinePrinter = new DinePrinter((ip, e) => {
					localLog($"打印机错误, 请检查打印机设置, {ip} {e.Message}");
				});

				dinePrinter.Print(dp, printTypes, true);
				localLog($"发送本地测试单命令成功");
			}
			catch(Exception e) {
				localLog($"无法打印本地测试单, 错误信息: {e}");
				log(Log.LogLevel.Error, $"Local Test, {e.Message}", $"Data: {JsonConvert.SerializeObject(dp)}, Error: {e}");
			}
		}

		static async Task printShifts(List<int> Ids, DateTime dateTime) {
			ShiftForPrinting sp = null;
			try {
				sp = await getShiftsForPrinting(Ids, dateTime);
				if(sp == null) {
					localLog("获取交接班信息失败，请检查网络设置");
					return;
				}
				ShiftPrinter shiftPrinter = new ShiftPrinter((ip, e) => {
					localLog($"打印机错误, 请检查打印机设置, {ip} {e.Message}");
				});
				localLog($"发送打印命令 交接班");

				shiftPrinter.Print(sp);

				localLog($"发送命令成功 交接班");
			}
			catch(Exception e) {
				localLog($"无法打印 交接班, 错误信息: {e}");
				log(Log.LogLevel.Error, $"ShiftInfos, {e.Message}", $"Data: {JsonConvert.SerializeObject(sp)}, Error: {e}");
			}
		}

		static async Task testPrinters() {
			PrintersForPrinting pp = null;
			try {
				pp = await getPrintersForPrinting();
				if(pp == null) {
					localLog("获取打印机数据失败，请检查网络设置");
					return;
				}
				foreach(var printer in pp.Printers) {
					localLog($"正在测试{printer.Name} {printer.IpAddress}");
					IPPrinter ipPrinter = new IPPrinter(new IPEndPoint(IPAddress.Parse(printer.IpAddress), 9100), null);
					if(ipPrinter.Test()) {
						localLog($"{printer.Name} {printer.IpAddress} 测试成功");
					}
					else {
						localLog($"{printer.Name} {printer.IpAddress} 测试失败");
					}
				}
				localLog($"测试完成");
			}
			catch(Exception e) {
				localLog($"无法测试, 错误信息: {e}");
			}
		}

		static async Task<DineForPrinting> getDineForPrinting(string dineId, List<int> dineMenuIds = null) {
			object postData = new {
				HotelId = hotelId,
				DineId = dineId,
				DineMenuIds = dineMenuIds
			};
			string response = await HttpPost.PostAsync(remoteGetDineForPrintingUrl, postData);
			if(response == null)
				return null;

			DineForPrinting protocol = JsonConvert.DeserializeObject<DineForPrinting>(response);
			if(protocol.Hotel == null)
				return null;
			return protocol;
		}
		static async Task<ShiftForPrinting> getShiftsForPrinting(List<int> ids, DateTime dateTime) {
			object postData = new {
				HotelId = hotelId,
				Ids = ids,
				DateTime = dateTime
			};
			string response = await HttpPost.PostAsync(remoteGetShiftsForPrintingUrl, postData);
			if(response == null)
				return null;

			ShiftForPrinting protocol = JsonConvert.DeserializeObject<ShiftForPrinting>(response);

			return protocol;
		}
		static async Task<PrintersForPrinting> getPrintersForPrinting() {
			string response = await HttpPost.PostAsync(remoteGetPrintersForPrintingUrl, new {
				HotelId = hotelId,
			});
			if(response == null)
				return null;

			PrintersForPrinting protocol = JsonConvert.DeserializeObject<PrintersForPrinting>(response);

			return protocol;
		}

		static void printCompleted(string dineId) {
			object postData = new {
				HotelId = hotelId,
				DineId = dineId
			};
			var _ = HttpPost.PostAsync(remotePrintCompletedUrl, postData);
		}
		static void localLog(string message) {
			Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] {message}");
		}
		static void log(Log.LogLevel level, string message, string detail = null) {
			object postData = new {
				HotelId = hotelId,
				Level = level,
				Message = message,
				Detail = detail
			};
			var _ = HttpPost.PostAsync(remoteLogUrl, postData);
		}
	}
}
