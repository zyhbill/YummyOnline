using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YummyOnlineDAO.Models {
	public enum OrderSystemStyle {
		Simple = 0,
		Fashion = 1
	}
	public class Hotel {
		[Key]
		public int Id { get; set; }

		public string ConnectionString { get; set; }
		public string AdminConnectionString { get; set; }
		[Required]
		public string CssThemePath { get; set; }
		public OrderSystemStyle OrderSystemStyle { get; set; }
		/// <summary>
		/// 酒店名称
		/// </summary>
		[MaxLength(20)]
		[Required]
		public string Name { get; set; }

		/// <summary>
		/// 酒店地址
		/// </summary>
		[MaxLength(128)]
		[Required]
		public string Address { get; set; }

		/// <summary>
		/// 酒店电话
		/// </summary>
		[MaxLength(20)]
		[Required]
		public string Tel { get; set; }

		/// <summary>
		/// 酒店营业时间
		/// </summary>
		public TimeSpan OpenTime { get; set; }
		public TimeSpan CloseTime { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public DateTime CreateDate { get; set; }

		public bool Usable { get; set; }

		public ICollection<MenuGather> MenuGathers { get; set; }
		public ICollection<Staff> Staffs { get; set; }
	}
}