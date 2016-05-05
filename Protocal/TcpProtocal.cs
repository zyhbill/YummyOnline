using HotelDAO.Models;
using System;
using System.Collections.Generic;
using YummyOnlineDAO.Models;

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
	public class RequestPrintDineProtocal : BaseTcpProtocal {
		public RequestPrintDineProtocal(int hotelId, string dineId, List<PrintType> printTypes)
			: base(TcpProtocalType.RequestPrintDine) {
			HotelId = hotelId;
			DineId = dineId;
			PrintTypes = printTypes;
		}
		public int HotelId { get; set; }
		public string DineId { get; set; }
		public List<PrintType> PrintTypes { get; set; }
	}
	public class PrintDineProtocal : BaseTcpProtocal {
		public PrintDineProtocal(string dineId, List<PrintType> printTypes)
			: base(TcpProtocalType.PrintDine) {
			DineId = dineId;
			PrintTypes = printTypes;
		}
		public string DineId { get; set; }
		public List<PrintType> PrintTypes { get; set; }
	}
}
