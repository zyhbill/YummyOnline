using HotelDAO.Models;
using System;
using System.Collections.Generic;

namespace Protocol.PrintingProtocol {
	public class ShiftDetail {
		public string PayKind { get; set; }
		public decimal ReceivablePrice { get; set; }
		public decimal RealPrice { get; set; }
	}
	public class Shift {
		public List<ShiftDetail> ShiftDetails { get; set; }
		public DateTime DateTime { get; set; }
		public int Id { get; set; }
	}
	public class ShiftForPrinting {
		public List<Shift> Shifts { get; set; }
		public Printer Printer { get; set; }
		public PrinterFormat PrinterFormat { get; set; }
	}
}
