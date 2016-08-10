using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelDAO.Models {
	public enum PayKindType {
		Online = 0,
		Offline = 1,
		Points = 2,
		Other = 3,
		Cash = 4,
		RandomPreference = 5
	}
	/// <summary>
	/// 支付类型
	/// </summary>
	public class PayKind {
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[MaxLength(20), Required]
		public string Name { get; set; }
		public PayKindType Type { get; set; }
		[MaxLength(128)]
		public string Description { get; set; }
		public bool Usable { get; set; }
		public double Discount { get; set; }
		/// <summary>
		/// 支付跳转地址
		/// </summary>
		[MaxLength(128)]
		public string RedirectUrl { get; set; }
		/// <summary>
		/// 支付完成跳转地址
		/// </summary>
		[MaxLength(128)]
		public string CompleteUrl { get; set; }
		/// <summary>
		/// 支付成功异步通知地址
		/// </summary>
		[MaxLength(128)]
		public string NotifyUrl { get; set; }
	}

	public class TimeDiscount {
		[Key, Column(Order = 0)]
		public TimeSpan From { get; set; }
		[Key, Column(Order = 1)]
		public TimeSpan To { get; set; }
		[Key, Column(Order = 2)]
		public DayOfWeek Week { get; set; }
		public double Discount { get; set; }
		[MaxLength(25)]
		public string Name { get; set; }
	}
	public class VipDiscount {
		[Key, ForeignKey(nameof(Level))]
		public int Id { get; set; }
		public VipLevel Level { get; set; }
		public double Discount { get; set; }
		[MaxLength(25)]
		public string Name { get; set; }
	}
}