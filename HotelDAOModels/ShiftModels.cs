using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelDAO.Models {
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
}
