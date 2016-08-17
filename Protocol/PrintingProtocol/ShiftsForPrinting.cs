using HotelDAO.Models;
using System;
using System.Collections.Generic;

namespace Protocol.PrintingProtocol {
	public class Shift {
		public int Id { get; set; }
		public DateTime DateTime { get; set; }
		public decimal OriPrice { get; set; }
		public decimal Price { get; set; }
		public decimal ToStayPrice { get; set; }
		public decimal ToGoPrice { get; set; }
		public decimal PreferencePrice { get; set; }
		public decimal GiftPrice { get; set; }
		public decimal ReturnedPrice { get; set; }
		public decimal AveragePrice { get; set; }
		public int DeskCount { get; set; }
		public int CustomerCount { get; set; }
	}

	public class PayKindShiftDetail {
		public string PayKind { get; set; }
		public decimal ReceivablePrice { get; set; }
		public decimal RealPrice { get; set; }
	}
	public class PayKindShift {
		public List<PayKindShiftDetail> PayKindShiftDetails { get; set; }
		public DateTime DateTime { get; set; }
		public int Id { get; set; }
	}

	public class MenuClassShiftDetail {
		public string MenuClass { get; set; }
		public decimal Price { get; set; }
	}
	public class MenuClassShift {
		public List<MenuClassShiftDetail> MenuClassShiftDetails { get; set; }
		public DateTime DateTime { get; set; }
		public int Id { get; set; }
	}

	public class ShiftForPrinting {
		public List<Shift> Shifts { get; set; }
		public List<PayKindShift> PayKindShifts { get; set; }
		public List<MenuClassShift> MenuClassShifts { get; set; }
		public Printer Printer { get; set; }
		public PrinterFormat PrinterFormat { get; set; }
	}
}
