using HotelDAO.Models;
using Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
				localLog("打印程序已经打开，请关闭后再打开打印程序。");
				return;
			}

			TcpClient tcp = new TcpClient(
				IPAddress.Parse(tcpServerIp),
				tcpServerPort,
				new PrintDineClientConnectProtocol(hotelId)
			);
			//tcp.CallBackWhenMessageReceived = async (t, p) => {
			//	if(t == TcpProtocolType.PrintDine) {
			//		PrintDineProtocol protocol = (PrintDineProtocol)p;
			//		await printDine(protocol.DineId, protocol.DineMenuIds, protocol.PrintTypes);
			//	}
			//	else if(t == TcpProtocolType.PrintShifts) {
			//		PrintShiftsProtocol protocol = (PrintShiftsProtocol)p;
			//		await printShifts(protocol.Ids, protocol.DateTime);
			//	}
			//};
			tcp.CallBackWhenConnected = () => {
				localLog("服务器连接成功");
				log(Log.LogLevel.Success, "Printer Connected");
			};
			tcp.CallBackWhenExceptionOccured = e => {
				localLog(e.Message);
				log(Log.LogLevel.Error, e.Message, e.ToString());
			};

			tcp.Start();
		}

		void localLog(string message) {
			listViewLog.Dispatcher.Invoke(() => {
				listViewLog.Items.Add(new {
					DateTime = DateTime.Now.ToString("HH:mm:ss"),
					Message = message
				});
			});
		}
		void log(Log.LogLevel level, string message, string detail = null) {
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
