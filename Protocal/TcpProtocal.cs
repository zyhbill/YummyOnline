﻿using System;
using System.Collections.Generic;

namespace Protocal {
	public static class TcpProtocalType {
		/// <summary>
		/// 需要及时收到新订单消息的客户端连接
		/// </summary>
		public const string NewDineInformClientConnect = "{053A168C-D4B8-409A-A058-7E2208B57CDA}";
		/// <summary>
		/// 新订单消息
		/// </summary>
		public const string NewDineInform = "{6309155D-B9D9-4417-B1BF-C985F2EA6630}";
		/// <summary>
		/// 饭店打印服务器连接
		/// </summary>
		public const string PrintDineClientConnect = "{2617871B-88A7-43A4-8E2F-23C319B61750}";
		/// <summary>
		/// 请求饭店打印
		/// </summary>
		public const string RequestPrintDine = "{FCAC99D2-1807-4FD0-8B5C-71D00B91A927}";
		/// <summary>
		/// 饭店打印
		/// </summary>
		public const string PrintDine = "{8A39D55F-CC16-4798-990E-062A9260496C}";
		/// <summary>
		/// 请求饭店打印交接班信息
		/// </summary>
		public const string RequestPrintShifts = "{4E6D44F1-9FD6-4DAD-BAE6-545577701149}";
		/// <summary>
		/// 饭店打印交接班信息
		/// </summary>
		public const string PrintShifts = "{EBFC25BD-2F2A-4ED2-8DEF-3206C113913A}";
	}
	public class BaseTcpProtocal {
		public BaseTcpProtocal(string type) {
			Type = type;
		}
		public string Type { get; set; }
	}

	public class NewDineInformClientConnectProtocal : BaseTcpProtocal {
		public NewDineInformClientConnectProtocal() : base(TcpProtocalType.NewDineInformClientConnect) { }
		public NewDineInformClientConnectProtocal(string guidStr) : base(TcpProtocalType.NewDineInformClientConnect) {
			Guid = new Guid(guidStr);
		}
		public Guid Guid { get; set; }
	}
	public class PrintDineClientConnectProtocal : BaseTcpProtocal {
		public PrintDineClientConnectProtocal(int hotelId)
			: base(TcpProtocalType.PrintDineClientConnect) {
			HotelId = hotelId;
		}
		public int HotelId { get; set; }
	}

	public class NewDineInformProtocal : BaseTcpProtocal {
		public NewDineInformProtocal(int hotelId, string dineId, bool isPaid)
			: base(TcpProtocalType.NewDineInform) {
			HotelId = hotelId;
			DineId = dineId;
			IsPaid = isPaid;
		}
		public int HotelId { get; set; }
		public string DineId { get; set; }
		public bool IsPaid { get; set; }
	}

	public enum PrintType {
		/// <summary>
		/// 收银条
		/// </summary>
		Recipt = 0,
		/// <summary>
		/// 厨房单
		/// </summary>
		KitchenOrder = 1,
		/// <summary>
		/// 传菜单
		/// </summary>
		ServeOrder = 2
	}

	public class PrintDineProtocal : BaseTcpProtocal {
		public PrintDineProtocal(string dineId, List<int> dineMenuIds, List<PrintType> printTypes)
			: base(TcpProtocalType.PrintDine) {
			DineId = dineId;
			DineMenuIds = dineMenuIds;
			PrintTypes = printTypes;
		}
		public string DineId { get; set; }
		public List<int> DineMenuIds { get; set; } = new List<int>();
		public List<PrintType> PrintTypes { get; set; } = new List<PrintType>();
	}
	public class RequestPrintDineProtocal : PrintDineProtocal {
		public RequestPrintDineProtocal(int hotelId, string dineId, List<int> dineMenuIds, List<PrintType> printTypes)
			: base(dineId, dineMenuIds, printTypes) {
			Type = TcpProtocalType.RequestPrintDine;
			HotelId = hotelId;
		}
		public int HotelId { get; set; }
	}

	public class PrintShiftsProtocal : BaseTcpProtocal {
		public PrintShiftsProtocal(List<int> ids, DateTime dateTime)
			: base(TcpProtocalType.PrintShifts) {
			Ids = ids;
			DateTime = dateTime;
		}
		public List<int> Ids { get; set; }
		public DateTime DateTime { get; set; }
	}
	public class RequestPrintShiftsProtocal : PrintShiftsProtocal {
		public RequestPrintShiftsProtocal(int hotelId, List<int> ids, DateTime dateTime)
			: base(ids, dateTime) {
			Type = TcpProtocalType.RequestPrintShifts;
			HotelId = hotelId;
		}
		public int HotelId { get; set; }
	}
}
