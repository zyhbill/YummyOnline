using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelDAO.Models {
	public enum TrimZero {
		None = 0,
		Fen = 1,
		Jiao = 2,
		Yuan = 3
	}
	public class HotelConfig {
		/// <summary>
		/// YummyOnline数据库Hotel外键
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int Id { get; set; }

		public bool NeedCodeImg { get; set; }
		/// <summary>
		/// 积分比率（1元 = PointsRatio积分）
		/// </summary>
		public int PointsRatio { get; set; }
		public TrimZero TrimZero { get; set; }
		public bool HasAutoPrinter { get; set; }

		[ForeignKey(nameof(ShiftPrinter))]
		public int? ShiftPrinterId { get; set; }
		public Printer ShiftPrinter { get; set; }
	}
}