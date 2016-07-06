using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using YummyOnlineDAO.Models;

namespace YummyOnlineTcpServer {
	public partial class TcpServer {
		private void systemConnect(TcpClientInfo clientInfo) {
			log($"{clientInfo.OriginalRemotePoint} (System) Connected", Log.LogLevel.Success);
			systemClient = clientInfo;

			_clientVerified(clientInfo);
		}
		/// <summary>
		/// 需要及时收到新订单的客户端连接
		/// </summary>
		private void newDineInfromClientConnected(TcpClientInfo clientInfo, NewDineInformClientConnectProtocol protocol) {
			log($"{clientInfo.OriginalRemotePoint} (NewDineInformClient): Guid: {protocol.Guid} Request Connection", Log.LogLevel.Info);

			if(protocol.Guid == new Guid()) {
				log($"{clientInfo.OriginalRemotePoint} NewDineInformClient Lack Guid", Log.LogLevel.Warning);
				clientInfo.Client.Client.Close();
				return;
			}

			KeyValuePair<NewDineInformClientGuid, TcpClientInfo> pair = newDineInformClients.FirstOrDefault(p => p.Key.Guid == protocol.Guid);
			if(pair.Key == null) {
				log($"{clientInfo.OriginalRemotePoint} NewDineInformClient Guid {protocol.Guid} Not Matched", Log.LogLevel.Warning);
				clientInfo.Client.Client.Close();
				return;
			}
			else {
				if(pair.Value != null) {
					log($"NewDineInformClient Guid {pair.Key.Guid} Repeated", Log.LogLevel.Warning);
					pair.Value.Client.Client.Close();

					clientCloseMutex.Reset();
					clientCloseMutex.WaitOne();
				}

				newDineInformClients[pair.Key] = clientInfo;

				log($"{clientInfo.OriginalRemotePoint} ({pair.Key.Description}) Connected", Log.LogLevel.Success);
			}

			_clientVerified(clientInfo);
		}

		/// <summary>
		/// 饭店打印机连接
		/// </summary>
		private void printDineClientConnected(TcpClientInfo clientInfo, PrintDineClientConnectProtocol protocol) {
			log($"{clientInfo.OriginalRemotePoint} (Printer): HotelId: {protocol.HotelId} Request Connection", Log.LogLevel.Info);

			if(!printerClients.ContainsKey(protocol.HotelId)) {
				log($"{clientInfo.OriginalRemotePoint} Printer HotelId {protocol.HotelId} Not Matched", Log.LogLevel.Warning);
				clientInfo.Client.Client.Close();
				return;
			}

			KeyValuePair<int, TcpClientInfo> pair = printerClients.FirstOrDefault(p => p.Key == protocol.HotelId);

			if(pair.Value != null) {
				log($"Printer HotelId {pair.Key} Repeated", Log.LogLevel.Warning);
				pair.Value.Client.Client.Close();

				clientCloseMutex.Reset();
				clientCloseMutex.WaitOne();
			}

			printerClients[pair.Key] = clientInfo;

			log($"{clientInfo.OriginalRemotePoint} (Printer of Hotel {protocol.HotelId}) Connected", Log.LogLevel.Success);

			_clientVerified(printerClients[pair.Key]);

			// 打印存储在打印等待队列中的所有请求
			while(printerWaitedQueue[pair.Key].Count > 0) {
				BaseTcpProtocol printProtocol = printerWaitedQueue[pair.Key].Dequeue();
				if(printProtocol.Type == TcpProtocolType.PrintDine) {
					sendPrintDineProtocol(pair.Key, (PrintDineProtocol)printProtocol);
				}
				else if(printProtocol.Type == TcpProtocolType.PrintShifts) {
					sendPrintShiftsProtocol(pair.Key, (PrintShiftsProtocol)printProtocol);
				}
				log($"Send Waited Dine of Hotel {pair.Key}", Log.LogLevel.Success);
			}
		}

		private void _clientVerified(TcpClientInfo clientInfo) {
			lock(waitingForVerificationClients) {
				TcpClientInfo waitedClientInfo = waitingForVerificationClients.FirstOrDefault(p => p.Client == clientInfo.Client);
				clientInfo.ConnectedTime = waitedClientInfo.ConnectedTime;
				waitingForVerificationClients.Remove(waitedClientInfo);
			}
		}
	}
}
