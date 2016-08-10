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
using System.Linq;
using System.Windows.Controls;

namespace AutoPrinter {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();

			textBoxIP.Text = Config.HistoryIPAddress;

			if(!Config.IsIPPrinter) {
				buttonConnectPrinter.Visibility = Visibility.Collapsed;
				buttonConnectPrinters.Visibility = Visibility.Collapsed;

				Grid.SetRowSpan(listViewLog, 2);
				listViewIpPrinter.Visibility = Visibility.Collapsed;
				textBlockIpPrinter.Visibility = Visibility.Collapsed;
				listViewIpPrinterStatus.Visibility = Visibility.Collapsed;
				textBlockIpPrinterStatus.Visibility = Visibility.Collapsed;

				textBlockIP.Visibility = Visibility.Collapsed;
				textBoxIP.Visibility = Visibility.Collapsed;
				comboBox.Visibility = Visibility.Visible;

				List<string> printers = BasePrinter.GetPritners();
				printers.ForEach(p => {
					comboBox.Items.Add(p);
				});
				comboBox.SelectedIndex = 0;
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
				serverLog("服务器连接成功", LogLevel.Success);
				remoteLog(Log.LogLevel.Success, "Printer Connected");
			};
			tcp.CallBackWhenExceptionOccured = (e) => {
				serverLog(e.Message, LogLevel.Error);
				remoteLog(Log.LogLevel.Error, e.Message, e.ToString());
			};

			tcp.Start();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e) {
			await initialize();
		}

		private async void buttonTestRemoteDines_Click(object sender, RoutedEventArgs e) {
			if(Config.IsIPPrinter) {
				await printRemoteTest(getCheckedPrintTypes(), textBoxIP.Text);
			}
			else {
				await printRemoteTest(getCheckedPrintTypes(), comboBox.SelectedItem.ToString());
			}
		}

		private async void buttonTestLocalDines_Click(object sender, RoutedEventArgs e) {
			if(Config.IsIPPrinter) {
				await printLocalTest(getCheckedPrintTypes(), textBoxIP.Text);
			}
			else {
				await printLocalTest(getCheckedPrintTypes(), comboBox.SelectedItem.ToString());
			}

		}

		private async void buttonConnectPrinter_Click(object sender, RoutedEventArgs e) {
			await IPPrinter.GetInstance().Connect(IPAddress.Parse(textBoxIP.Text));
		}

		private async void buttonConnectPrinters_Click(object sender, RoutedEventArgs e) {
			var protocol = await getPrintersForPrinting();
			if(protocol == null) {
				serverLog("获取打印机信息失败，请检查网络设置", LogLevel.Error);
				return;
			}

			foreach(var ip in protocol.Printers.Select(p => p.IpAddress).Distinct()) {
				await IPPrinter.GetInstance().Connect(IPAddress.Parse(ip));
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

		private void textBoxIPorName_TextChanged(object sender, TextChangedEventArgs e) {
			if(!Config.IsIPPrinter) {
				return;
			}

			IPAddress ip;
			if(IPAddress.TryParse(textBoxIP.Text, out ip)) {
				buttonConnectPrinter.IsEnabled = true;
				buttonTestLocalDines.IsEnabled = true;
				buttonTestRemoteDines.IsEnabled = true;

				Config.HistoryIPAddress = textBoxIP.Text;
				Config.SaveConfigs();
			}
			else {
				buttonConnectPrinter.IsEnabled = false;
				buttonTestLocalDines.IsEnabled = false;
				buttonTestRemoteDines.IsEnabled = false;
			}
		}
	}
}
