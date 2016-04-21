using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelDAO.Models {
	/// <summary>
	/// 饭店个性化用户
	/// </summary>
	public class Customer {
		/// <summary>
		/// YummyOnline数据库用户外键
		/// </summary>
		[Key, MaxLength(10)]
		public string Id { get; set; }
		/// <summary>
		/// 用户积分
		/// </summary>
		public int Points { get; set; }

		public int? VipLevelId { get; set; }
		public VipLevel VipLevel { get; set; }
	}
	/// <summary>
	/// 会员等级
	/// </summary>
	public class VipLevel {
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[MaxLength(10)]
		public string Name { get; set; }

		public VipDiscount VipDiscount { get; set; }
		public ICollection<Customer> Customers { get; set; }
	}
}
