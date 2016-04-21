using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelDAO.Models {
	/// <summary>
	/// 区域
	/// </summary>
	public class Area {
		[Key, MaxLength(3)]
		public string Id { get; set; }
		[MaxLength(20), Required]
		public string Name { get; set; }
		[MaxLength(128)]
		public string Description { get; set; }
		public bool Usable { get; set; }
		
		/// <summary>
		/// 收银部门
		/// </summary>
		public int? DepartmentReciptId { get; set; }
		public Department DepartmentRecipt { get; set; }

		/// <summary>
		/// 传菜单部门
		/// </summary>
		public int? DepartmentServeId { get; set; }
		public Department DepartmentServe { get; set;}

		public ICollection<Desk> Desks { get; set; }
	}
	public enum DeskStatus {
		/// <summary>
		/// 空
		/// </summary>
		StandBy = 0,
		/// <summary>
		/// 正在使用
		/// </summary>
		Used = 1,
		/// <summary>
		/// 被预定
		/// </summary>
		Reserved = 2
	}
	/// <summary>
	/// 桌子
	/// </summary>
	public class Desk {
		[Key, MaxLength(4)]
		public string Id { get; set; }
		/// <summary>
		/// 二维码
		/// </summary>
		[MaxLength(5)]
		public string QrCode { get; set; }
		[MaxLength(20), Required]
		public string Name { get; set; }
		[MaxLength(128)]
		public string Description { get; set; }
		public DeskStatus Status { get; set; }
		/// <summary>
		/// 桌子排序
		/// </summary>
		public int Order { get; set; }
		public int HeadCount { get; set; }
		/// <summary>
		/// 最低消费
		/// </summary>
		public decimal MinPrice { get; set; }
		public bool Usable { get; set; } = true;
		
		public string AreaId { get; set; }
		public Area Area { get; set; }
	}
}