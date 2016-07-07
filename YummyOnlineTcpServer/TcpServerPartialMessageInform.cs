using Newtonsoft.Json;
using Protocol;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YummyOnlineDAO.Models;

namespace YummyOnlineTcpServer {
	public partial class TcpServer {
		/// <summary>
		/// 新订单通知
		/// </summary>
		/// <param name="clientInfo"></param>
		/// <param name="protocol"></param>
		private void newDineInform(TcpClientInfo clientInfo, NewDineInformProtocol protocol) {
			NewDineInformClientGuid sender = getSender(clientInfo);
			if(sender == null) {
				log($"{clientInfo.OriginalRemotePoint} Received NewDineInform From Invalid NewDineInformClient", Log.LogLevel.Error);
				clientInfo.Close();
				return;
			}
			
			log($"{clientInfo.OriginalRemotePoint} (NewDineInform): From: {sender.Description}, HotelId: {protocol.HotelId}, DineId: {protocol.DineId}, IsPaid: {protocol.IsPaid}", Log.LogLevel.Success);

			foreach(var p in newDineInformClients) {
				// 不向未连接的客户端与发送方客户端 发送新订单通知信息
				if(p.Value == null || p.Value.Client == clientInfo.Client)
					continue;

				string content = JsonConvert.SerializeObject(protocol);
				var _ = tcp.Send(p.Value.Client, content, null);
			}
		}
		/// <summary>
		/// 请求打印
		/// </summary>
		private void requestPrintDine(TcpClientInfo clientInfo, RequestPrintDineProtocol protocol) {
			NewDineInformClientGuid sender = getSender(clientInfo);
			if(sender == null) {
				log($"{clientInfo.OriginalRemotePoint} Received RequestPrintDine From Invalid NewDineInformClient", Log.LogLevel.Error);
				clientInfo.Close();
				return;
			}
			
			protocol.DineMenuIds = protocol.DineMenuIds ?? new List<int>();
			protocol.PrintTypes = protocol.PrintTypes ?? new List<PrintType>();

			StringBuilder dineMenuStr = new StringBuilder();
			for(int i = 0; i < protocol.DineMenuIds.Count; i++) {
				dineMenuStr.Append(protocol.DineMenuIds[i]);
				if(i != protocol.DineMenuIds.Count - 1)
					dineMenuStr.Append(' ');
			}

			StringBuilder typeStr = new StringBuilder();
			foreach(var type in protocol.PrintTypes) {
				typeStr.Append($"{type.ToString()} ");
			}

			log($"{clientInfo.OriginalRemotePoint} (RequestPrintDine): From: {sender.Description}, HotelId: {protocol.HotelId}, DineId: {protocol.DineId}, DineMenuIds: {dineMenuStr}, PrintTypes: {typeStr}",
				Log.LogLevel.Success);

			PrintDineProtocol p = new PrintDineProtocol(protocol.DineId, protocol.DineMenuIds, protocol.PrintTypes);
			if(printerClients[protocol.HotelId] == null) {
				printerWaitedQueue[protocol.HotelId].Enqueue(p);
				log($"Printer of Hotel {protocol.HotelId} is not connected", Log.LogLevel.Error);
				return;
			}
			sendPrintDineProtocol(protocol.HotelId, p);
		}

		/// <summary>
		/// 向饭店打印机发送打印协议
		/// </summary>
		private void sendPrintDineProtocol(int hotelId, PrintDineProtocol protocol) {
			var _ = tcp.Send(printerClients[hotelId].Client, JsonConvert.SerializeObject(protocol, new JsonSerializerSettings {
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			}), null);
		}

		private void requestPrintShifts(TcpClientInfo clientInfo, RequestPrintShiftsProtocol protocol) {
			NewDineInformClientGuid sender = getSender(clientInfo);
			if(sender == null) {
				log($"{clientInfo.OriginalRemotePoint} Received RequestPrintShifts From Invalid NewDineInformClient", Log.LogLevel.Error);
				clientInfo.Close();
				return;
			}

			protocol.Ids = protocol.Ids ?? new List<int>();

			StringBuilder idStr = new StringBuilder();
			foreach(int id in protocol.Ids) {
				idStr.Append($"{id} ");
			}

			log($"{clientInfo.OriginalRemotePoint} (RequestPrintShifts): From: {sender.Description}, HotelId: {protocol.HotelId}, Ids: {idStr}, DateTime: {protocol.DateTime.ToString("yyyy-MM-dd")}",
				Log.LogLevel.Success);

			PrintShiftsProtocol p = new PrintShiftsProtocol(protocol.Ids, protocol.DateTime);
			if(printerClients[protocol.HotelId] == null) {
				printerWaitedQueue[protocol.HotelId].Enqueue(p);
				log($"Printer of Hotel {protocol.HotelId} is not connected", Log.LogLevel.Error);
				return;
			}
			sendPrintShiftsProtocol(protocol.HotelId, p);
		}
		private void sendPrintShiftsProtocol(int hotelId, PrintShiftsProtocol protocol) {
			var _ = tcp.Send(printerClients[hotelId].Client, JsonConvert.SerializeObject(protocol, new JsonSerializerSettings {
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			}), null);
		}



		private NewDineInformClientGuid getSender(TcpClientInfo clientInfo) {
			return newDineInformClients
				.Where(p => p.Value?.Client == clientInfo.Client)
				.Select(p => p.Key)
				.FirstOrDefault();
		}
	}
}
