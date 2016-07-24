using HotelDAO.Models;
using Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Deployment.Application;
using System.Diagnostics;
using System.Net;
using System.Windows;
using YummyOnlineTcpClient;

namespace AutoPrinter {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
			try {
				Title = $"YummyOnline自助打印 {ApplicationDeployment.CurrentDeployment.CurrentVersion}";
			}
			catch {
				Title = $"YummyOnline自助打印 内部调试版本";
			}

			Process[] tProcess = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
			if(tProcess.Length > 1) {
				Application.Current.Shutdown();
			}

			TcpClient tcp = new TcpClient(
				IPAddress.Parse(Config.TcpServerIp),
				Config.TcpServerPort,
				new PrintDineClientConnectProtocol(Config.HotelId)
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

			IPPrinter.GetInstance().OnLog += (ip, bmp, message) => {
				ipPrinterLog(ip, bmp?.GetHashCode(), message);
			};
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
