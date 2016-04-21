using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YummyOnlineDAO.Models {
	public class Hotel {
		[Key]
		public int Id { get; set; }

		[Required]
		public string ConnectionString { get; set; }

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
		//经纬度

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

		public ICollection<MenuGather> MenuGathers { get; set; }
		public ICollection<Staff> Staffs { get; set;}
	}
}