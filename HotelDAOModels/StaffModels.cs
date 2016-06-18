using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelDAO.Models {
	/// <summary>
	/// 饭店职员
	/// </summary>
	public class Staff {
		/// <summary>
		/// YummyOnline数据库用户外键
		/// </summary>
		[Key, MaxLength(8)]
		public string Id { get; set; }

		[MaxLength(10)]
		public string Name { get; set; }

		public int DineCount { get; set; }
		public decimal DinePrice { get; set; }
		public TimeSpan WorkTimeFrom { get; set; }
		public TimeSpan WorkTimeTo { get; set; }

		public ICollection<StaffRole> StaffRoles { get; set; }
	}
	/// <summary>
	/// 职员角色
	/// </summary>
	public class StaffRole {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[MaxLength(5), Required]
		public string Name { get; set; }

		public ICollection<Staff> Staffs { get; set; }
		public ICollection<StaffRoleSchema> Schemas { get; set; }
	}

	/// <summary>
	/// 架构
	/// </summary>
	public enum Schema {
		/// <summary>
		/// 读取面向服务员的菜品信息
		/// </summary>
		ReadWaiterData = 0,
		/// <summary>
		/// 提交面向服务员的支付
		/// </summary>
		SubmitWaiterPay = 1,
		/// <summary>
		/// 职员支付
		/// </summary>
		StaffPay = 2,
		/// <summary>
		/// 职员退菜
		/// </summary>
		StaffReturn = 3,
		/// <summary>
		/// 职员修改基础信息
		/// </summary>
		StaffEdit = 4
	}
	/// <summary>
	/// 角色对应架构
	/// </summary>
	public class StaffRoleSchema {
		[Key, Column(Order = 0)]
		public int StaffRoleId { get; set; }
		public StaffRole StaffRole { get; set; }
		[Key, Column(Order = 1)]
		public Schema Schema { get; set; }
	}
}
