using HotelDAO.Models;
using Newtonsoft.Json;
using Protocol;
using System.Collections.Generic;
using System.Diagnostics;
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
		public MainWindow() {
			InitializeComponent();

			Process[] tProcess = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
			if(tProcess.Length > 1) {
				MessageBox.Show("已经开启一个程序，请关闭后重试", "重复启动", MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown();
			}

			IPPrinter.GetInstance().OnLog += (ip, bmp, message, style) => {
				ipPrinterLog(ip, bmp?.GetHashCode(), message, style);
			};
		}

		private async Task initialize() {
			string resultStr = await HttpPost.PostAsync(Config.RemoteGetHotelConfigUrl, null);

			var hotel = JsonConvert.DeserializeObject<YummyOnlineDAO.Models.Hotel>(resultStr);
			Config.HotelId = hotel.Id;
			Title += $" {hotel.Name}";

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
				localLog("服务器连接成功", AutoPrinter.Style.Success);
				remoteLog(Log.LogLevel.Success, "Printer Connected");
			};
			tcp.CallBackWhenExceptionOccured = (e) => {
				localLog(e.Message, AutoPrinter.Style.Danger);
				remoteLog(Log.LogLevel.Error, e.Message, e.ToString());
			};

			tcp.Start();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e) {
			await initialize();
		}

		private async void buttonTestRemoteDines_Click(object sender, RoutedEventArgs e) {
			await printRemoteTest(getCheckedPrintTypes(), textBoxIp.Text);
		}

		private async void buttonTestLocalDines_Click(object sender, RoutedEventArgs e) {
			await printLocalTest(getCheckedPrintTypes(), textBoxIp.Text);
		}

		private async void buttonConnectPrinter_Click(object sender, RoutedEventArgs e) {
			await IPPrinter.GetInstance().Connect(IPAddress.Parse(textBoxIp.Text));
		}

		private async void buttonConnectPrinters_Click(object sender, RoutedEventArgs e) {
			var protocol = await getPrintersForPrinting();
			if(protocol == null) {
				localLog("获取打印机信息失败，请检查网络设置", AutoPrinter.Style.Danger);
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

		private void textBoxIp_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
			IPAddress ip;
			if(IPAddress.TryParse(textBoxIp.Text, out ip)) {
				buttonConnectPrinter.IsEnabled = true;
				buttonTestLocalDines.IsEnabled = true;
				buttonTestRemoteDines.IsEnabled = true;
			}
			else {
				buttonConnectPrinter.IsEnabled = false;
				buttonTestLocalDines.IsEnabled = false;
				buttonTestRemoteDines.IsEnabled = false;
			}
		}
	}
}
