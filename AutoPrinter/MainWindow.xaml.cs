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
using System.Threading.Tasks;
using System.Windows;
using Utility;
using YummyOnlineTcpClient;

namespace AutoPrinter {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {

		private static int hotelId = Convert.ToInt32(ConfigurationManager.AppSettings["HotelId"]);
		private static string tcpServerIp = ConfigurationManager.AppSettings["TcpServerIp"].ToString();
		private static int tcpServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["TcpServerPort"]);

		private static string remoteLogUrl = ConfigurationManager.AppSettings["RemoteLogUrl"];
		private static string remotePrintCompletedUrl = ConfigurationManager.AppSettings["RemotePrintCompletedUrl"];
		private static string remoteGetDineForPrintingUrl = ConfigurationManager.AppSettings["RemoteGetDineForPrintingUrl"];
		private static string remoteGetShiftsForPrintingUrl = ConfigurationManager.AppSettings["RemoteGetShiftsForPrintingUrl"];
		private static string remoteGetPrintersForPrintingUrl = ConfigurationManager.AppSettings["RemoteGetPrintersForPrintingUrl"];

		public MainWindow() {
			InitializeComponent();
			Title = $"YummyOnline自助打印 {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";

			if(!Directory.Exists($@"{Environment.CurrentDirectory}\failedImgs")) {
				Directory.CreateDirectory($@"{Environment.CurrentDirectory}\failedImgs");
			}

			Process[] tProcess = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
			if(tProcess.Length > 1) {
				Application.Current.Shutdown();
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
				remoteLog(Log.LogLevel.Success, "Printer Connected");
			};
			tcp.CallBackWhenExceptionOccured = e => {
				localLog(e.Message);
				remoteLog(Log.LogLevel.Error, e.Message, e.ToString());
			};

			tcp.Start();

			IPPrinter.GetInstance((ip, bmp, message) => {
				ipPrinterLog(ip, bmp?.GetHashCode(), message);
			});
		}

		async Task printDine(string dineId, List<int> dineMenuIds, List<PrintType> printTypes) {
			DineForPrinting dp = null;
			try {
				dp = await getDineForPrinting(dineId, dineMenuIds);
				if(dp == null) {
					localLog("获取订单信息失败，请检查网络设置");
					return;
				}

				localLog($"发送打印命令 单号: {dineId}");
				DinePrinter dinePrinter = new DinePrinter();
				await dinePrinter.Print(dp, printTypes, dineMenuIds == null);
				localLog($"发送命令成功 单号: {dineId}");
				printCompleted(dineId);
			}
			catch(Exception e) {
				localLog($"无法打印 单号: {dineId}, 错误信息: {e}");
				remoteLog(Log.LogLevel.Error, $"DineId: {dineId}, {e.Message}", $"Data: {JsonConvert.SerializeObject(dp)}, Error: {e}");
			}
		}
		async Task printRemoteTest(List<PrintType> printTypes, string ipAddress) {
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
				DinePrinter dinePrinter = new DinePrinter();
				await dinePrinter.Print(dp, printTypes, true);
				localLog($"发送测试单命令成功");
				printCompleted("00000000000000");
			}
			catch(Exception e) {
				localLog($"无法打印测试单, 错误信息: {e}");
				remoteLog(Log.LogLevel.Error, $"Online Test, {e.Message}", $"Data: {JsonConvert.SerializeObject(dp)}, Error: {e}");
			}
		}

		async Task printLocalTest(List<PrintType> printTypes, string ipAddress) {
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
				DinePrinter dinePrinter = new DinePrinter();

				await dinePrinter.Print(dp, printTypes, true);
				localLog($"发送本地测试单命令成功");
			}
			catch(Exception e) {
				localLog($"无法打印本地测试单, 错误信息: {e}");
				remoteLog(Log.LogLevel.Error, $"Local Test, {e.Message}", $"Data: {JsonConvert.SerializeObject(dp)}, Error: {e}");
			}
		}

		async Task printShifts(List<int> Ids, DateTime dateTime) {
			ShiftForPrinting sp = null;
			try {
				sp = await getShiftsForPrinting(Ids, dateTime);
				if(sp == null) {
					localLog("获取交接班信息失败，请检查网络设置");
					return;
				}
				ShiftPrinter shiftPrinter = new ShiftPrinter();
				localLog($"发送打印命令 交接班");

				await shiftPrinter.Print(sp);

				localLog($"发送命令成功 交接班");
			}
			catch(Exception e) {
				localLog($"无法打印 交接班, 错误信息: {e}");
				remoteLog(Log.LogLevel.Error, $"ShiftInfos, {e.Message}", $"Data: {JsonConvert.SerializeObject(sp)}, Error: {e}");
			}
		}

		async Task<DineForPrinting> getDineForPrinting(string dineId, List<int> dineMenuIds = null) {
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
		async Task<ShiftForPrinting> getShiftsForPrinting(List<int> ids, DateTime dateTime) {
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
		async Task<PrintersForPrinting> getPrintersForPrinting() {
			string response = await HttpPost.PostAsync(remoteGetPrintersForPrintingUrl, new {
				HotelId = hotelId,
			});
			if(response == null)
				return null;

			PrintersForPrinting protocol = JsonConvert.DeserializeObject<PrintersForPrinting>(response);

			return protocol;
		}

		void printCompleted(string dineId) {
			object postData = new {
				HotelId = hotelId,
				DineId = dineId
			};
			var _ = HttpPost.PostAsync(remotePrintCompletedUrl, postData);
		}

		void localLog(string message) {
			listViewLog.Dispatcher.Invoke(() => {
				listViewLog.Items.Add(new {
					DateTime = DateTime.Now,
					Message = message
				});

				listViewLog.SelectedIndex = listViewLog.Items.Count - 1;
				listViewLog.ScrollIntoView(listViewLog.SelectedItem);
			});
		}
		void ipPrinterLog(IPAddress ip, int? hashCode, string message) {
			if(message != null) {
				listViewIpPrinter.Dispatcher.Invoke(() => {

					listViewIpPrinter.Items.Add(new {
						DateTime = DateTime.Now,
						IP = ip,
						Message = message,
						HashCode = hashCode
					});

					listViewIpPrinter.SelectedIndex = listViewIpPrinter.Items.Count - 1;
					listViewIpPrinter.ScrollIntoView(listViewIpPrinter.SelectedItem);

				});
			}

			listViewIpPrinterStatus.Dispatcher.Invoke(() => {
				listViewIpPrinterStatus.Items.Clear();
				var map = IPPrinter.GetInstance().IPClientBmpMap;
				foreach(IPAddress ipKey in map.Keys) {
					string status = "已断开";
					if(map[ipKey].IsConnecting) {
						status = "正在连接";
					}
					else if(map[ipKey].TcpClientInfo != null) {
						status = "已连接";
					}
					listViewIpPrinterStatus.Items.Add(new {
						IP = ipKey,
						Status = status,
						WaitedCount = map[ipKey].Queue.Count,
						IdleTime = map[ipKey].TcpClientInfo?.IdleTime
					});
				}
			});
		}

		void remoteLog(Log.LogLevel level, string message, string detail = null) {
			object postData = new {
				HotelId = hotelId,
				Level = level,
				Message = message,
				Detail = detail
			};
			var _ = HttpPost.PostAsync(remoteLogUrl, postData);
		}
		
		private async void buttonTestRemoteDines_Click(object sender, RoutedEventArgs e) {
			string ipAddress = textBoxIp.Text;

			await printRemoteTest(getCheckedPrintTypes(), ipAddress);
		}

		private async void buttonTestLocalDines_Click(object sender, RoutedEventArgs e) {
			string ipAddress = textBoxIp.Text;
			await printLocalTest(getCheckedPrintTypes(), ipAddress);
		}

		private async void buttonConnectPrinter_Click(object sender, RoutedEventArgs e) {
			await IPPrinter.GetInstance().Connect(IPAddress.Parse(textBoxIp.Text));
		}

		private async void buttonConnectPrinters_Click(object sender, RoutedEventArgs e) {
			var protocol = await getPrintersForPrinting();
			if(protocol == null) {
				localLog("获取打印机信息失败，请检查网络设置");
				return;
			}
			foreach(var p in protocol.Printers) {
				await IPPrinter.GetInstance().Connect(IPAddress.Parse(p.IpAddress));
			}
		}

		private List<PrintType> getCheckedPrintTypes() {
			List<PrintType> types = new List<PrintType>();
			if(checkBoxRecipt.IsChecked.Value) {
				types.Add(PrintType.Recipt);
			}
			if(checkBoxServeOrder.IsChecked.Value) {
				types.Add(PrintType.ServeOrder);
			}
			if(checkBoxKitchenOrder.IsChecked.Value) {
				types.Add(PrintType.KitchenOrder);
			}
			return types;
		}
	}
}
