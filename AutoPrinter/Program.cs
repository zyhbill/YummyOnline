using HotelDAO.Models;
using Newtonsoft.Json;
using Protocal;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using YummyOnlineTcpClient;
using System.Text;
using System.Diagnostics;

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
				if(t != TcpProtocalType.PrintDine) {
					return;
				}
				PrintDineProtocal protocal = (PrintDineProtocal)p;

				try {
					DineForPrintingProtocal dp = await getDineForPrinting(protocal.DineId);
					if(dp == null) {
						Console.WriteLine("获取订单信息失败，请检查网络设置");
						return;
					}
					DinePrinter dinePrinter = new DinePrinter(dp, protocal.PrintTypes);
					dinePrinter.Print();
					Console.WriteLine($"正在打印 单号: {dp.Dine.Id}");
				}
				catch(Exception e) {
					Console.WriteLine("无法打印, 请检查打印机设置");
					Console.WriteLine($"订单号: {protocal.DineId}, 错误信息: {e}");
					log(Log.LogLevel.Error, $"DineId: {protocal.DineId}, {e.Message}");
					return;
				}
				Console.WriteLine($"打印成功 单号: {protocal.DineId}");
				printCompleted(protocal.DineId);
			};
			tcp.CallBackWhenConnected = () => {
				Console.WriteLine("服务器连接成功");
				log(Log.LogLevel.Success, "Printer Connected");
			};
			tcp.CallBackWhenExceptionOccured = e => {
				Console.WriteLine(e.Message);
				log(Log.LogLevel.Error, e.Message);
			};

			tcp.Start();

			while(true) {
				Console.Read();
			}
		}

		static async Task<DineForPrintingProtocal> getDineForPrinting(string dineId) {
			object postData = new {
				HotelId = hotelId,
				DineId = dineId
			};
			string response = await postHttp(ConfigurationManager.AppSettings["RemoteGetDineForPrintingUrl"].ToString(), postData);
			if(response == null) {
				return null;
			}
			return JsonConvert.DeserializeObject<DineForPrintingProtocal>(response);
		}

		static void printCompleted(string dineId) {
			object postData = new {
				HotelId = hotelId,
				DineId = dineId
			};
			var _ = postHttp(ConfigurationManager.AppSettings["RemotePrintCompletedUrl"].ToString(), postData);
		}

		static void log(Log.LogLevel level, string message) {
			object postData = new {
				HotelId = hotelId,
				Level = level,
				Message = message
			};
			var _ = postHttp(ConfigurationManager.AppSettings["RemoteLogUrl"].ToString(), postData);
		}

		static async Task<string> postHttp(string url, object postData, string contentType = "application/json") {
			try {
				HttpClient client = new HttpClient();
				StringContent content = new StringContent(JsonConvert.SerializeObject(postData));
				content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
				HttpResponseMessage response = await client.PostAsync(url, content);
				if(response != null) {
					if(response.StatusCode == HttpStatusCode.OK) {
						return await response.Content.ReadAsStringAsync();
					}
				}
			}
			catch { }
			return null;
		}
	}
}
