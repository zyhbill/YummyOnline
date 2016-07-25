using Awesomium.Core;
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

			browser.Source = new Uri($@"{Config.BaseDir}\web\main.html");

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
			tcp.CallBackWhenExceptionOccured = (e) => {
				localLog(e.Message);
				remoteLog(Log.LogLevel.Error, e.Message, e.ToString());
			};

			tcp.Start();

			IPPrinter.GetInstance().OnLog += (ip, bmp, message) => {
				ipPrinterLog(ip, bmp?.GetHashCode(), message);
			};
		}

		private List<PrintType> getCheckedPrintTypes(bool recipt, bool serveOrder, bool kitchenOrder) {
			List<PrintType> types = new List<PrintType>();
			if(recipt) {
				types.Add(PrintType.Recipt);
			}
			if(serveOrder) {
				types.Add(PrintType.ServeOrder);
			}
			if(kitchenOrder) {
				types.Add(PrintType.KitchenOrder);
			}
			return types;
		}

		private void browser_NativeViewInitialized(object sender, WebViewEventArgs e) {
			JSObject external = browser.CreateGlobalJavascriptObject("external");

			if(external == null)
				return;

			using(external) {
				JSObject app = browser.CreateGlobalJavascriptObject("external.app");

				if(app == null)
					return;

				using(app) {
					app.BindAsync("testLocalDines", async v => {
						await printLocalTest(getCheckedPrintTypes(v[1], v[2], v[3]), v[0]);
					});
					app.BindAsync("testRemoteDines", async v => {
						await printRemoteTest(getCheckedPrintTypes(v[1], v[2], v[3]), v[0]);
					});
					app.BindAsync("connectPrinter", async v => {
						await IPPrinter.GetInstance().Connect(IPAddress.Parse(v[0]));
					});
					app.BindAsync("connectPrinters", async v => {
						var protocol = await getPrintersForPrinting();
						if(protocol == null) {
							localLog("获取打印机信息失败，请检查网络设置");
							return;
						}
						foreach(var p in protocol.Printers) {
							await IPPrinter.GetInstance().Connect(IPAddress.Parse(p.IpAddress));
						}
					});
				}
			}
		}
	}
}
