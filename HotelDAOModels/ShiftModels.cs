using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelDAO.Models {
	public class Shift {
		[Key, Column(Order = 0)]
		public int Id { get; set; }
		[Key, Column(Order = 1)]
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

	public class PayKindShift {
		[Key, Column(Order = 0)]
		public int Id { get; set; }
		[Key, Column(Order = 1)]
		public DateTime DateTime { get; set; }
		[Key, Column(Order = 2), ForeignKey(nameof(PayKind))]
		public int PayKindId { get; set; }
		public PayKind PayKind { get; set; }
		public decimal ReceivablePrice { get; set; }
		public decimal RealPrice { get; set; }
	}
	public class MenuClassShift {
		[Key, Column(Order = 0)]
		public int Id { get; set; }
		[Key, Column(Order = 1)]
		public DateTime DateTime { get; set; }
		[Key, Column(Order = 2), ForeignKey(nameof(MenuClass))]
		public string MenuClassId { get; set; }
		public MenuClass MenuClass { get; set; }
		public decimal Price { get; set; }
	}
}
