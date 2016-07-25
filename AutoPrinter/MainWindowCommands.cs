using HotelDAO.Models;
using Newtonsoft.Json;
using Protocol;
using Protocol.PrintingProtocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Utility;

namespace AutoPrinter {
	public partial class MainWindow {
		/// <summary>
		/// 打印订单及菜品信息
		/// </summary>
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
		/// <summary>
		/// 打印远程测试
		/// </summary>
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
		/// <summary>
		/// 打印本地测试
		/// </summary>
		async Task printLocalTest(List<PrintType> printTypes, string ipAddress) {
			DineForPrinting dp = Config.GetTestProtocol(ipAddress);
			try {
				localLog($"开始本地测试, 打印机: {ipAddress}");
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
				HotelId = Config.HotelId,
				DineId = dineId,
				DineMenuIds = dineMenuIds
			};
			string response = await HttpPost.PostAsync(Config.RemoteGetDineForPrintingUrl, postData);
			if(response == null)
				return null;

			DineForPrinting protocol = JsonConvert.DeserializeObject<DineForPrinting>(response);
			if(protocol.Hotel == null)
				return null;
			return protocol;
		}
		async Task<ShiftForPrinting> getShiftsForPrinting(List<int> ids, DateTime dateTime) {
			object postData = new {
				HotelId = Config.HotelId,
				Ids = ids,
				DateTime = dateTime
			};
			string response = await HttpPost.PostAsync(Config.RemoteGetShiftsForPrintingUrl, postData);
			if(response == null)
				return null;

			ShiftForPrinting protocol = JsonConvert.DeserializeObject<ShiftForPrinting>(response);

			return protocol;
		}
		async Task<PrintersForPrinting> getPrintersForPrinting() {
			string response = await HttpPost.PostAsync(Config.RemoteGetPrintersForPrintingUrl, new {
				HotelId = Config.HotelId,
			});
			if(response == null)
				return null;

			PrintersForPrinting protocol = JsonConvert.DeserializeObject<PrintersForPrinting>(response);

			return protocol;
		}

		void printCompleted(string dineId) {
			object postData = new {
				HotelId = Config.HotelId,
				DineId = dineId
			};
			var _ = HttpPost.PostAsync(Config.RemotePrintCompletedUrl, postData);
		}

		void localLog(string message) {
			browser.Dispatcher.Invoke(() => {
				if(!browser.IsLive || !browser.IsDocumentReady)
					return;

				browser.ExecuteJavascript($"internal.addLog('{DateTime.Now}', '{message}');");
			});
		}
		void ipPrinterLog(IPAddress ip, int? hashCode, string message) {
			browser.Dispatcher.Invoke(() => {
				if(!browser.IsLive || !browser.IsDocumentReady)
					return;

				if(message != null) {
					browser.ExecuteJavascript($"internal.addIPPrinterLog('{DateTime.Now}', '{ip}', '{message}', '{hashCode}');");
				}

				List<dynamic> statuses = new List<dynamic>();
				var map = IPPrinter.GetInstance().IPClientBmpMap;
				foreach(IPAddress ipKey in map.Keys) {
					string status = "已断开";
					if(map[ipKey].IsConnecting) {
						status = "正在连接";
					}
					else if(map[ipKey].TcpClientInfo != null) {
						status = "已连接";
					}
					statuses.Add(new {
						IP = ipKey.ToString(),
						Status = status,
						WaitedCount = map[ipKey].Queue.Count,
						IdleTime = map[ipKey].TcpClientInfo?.IdleTime
					});
				}
				string str = JsonConvert.SerializeObject(statuses.ToArray());
				browser.ExecuteJavascript($"internal.refreshIPPrinterStatuses({str});");
			});
		}

		void remoteLog(Log.LogLevel level, string message, string detail = null) {
			object postData = new {
				HotelId = Config.HotelId,
				Level = level,
				Message = message,
				Detail = detail
			};
			var _ = HttpPost.PostAsync(Config.RemoteLogUrl, postData);
		}
	}
}
