using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YummyOnlineDAO.Models {
	public class MenuGather {
		/// <summary>
		/// 饭店独立数据库对应的Id
		/// </summary>
		[Key, Column(Order = 0)]
		[MaxLength(6)]
		public string Id { get; set; }
		/// <summary>
		/// 饭店的Id
		/// </summary>
		[Key, Column(Order = 1)]
		public int HotelId { get; set; }
		public Hotel Hotel { get; set; }

		[MaxLength(20)]
		[Required]
		public string Name { get; set; }

		public ICollection<Nutrition> Nutrition { get; set; }
		public ICollection<Flavor> Flavor { get; set; }
	}
	/// <summary>
	/// 菜品营养，多对多表
	/// </summary>
	public class Nutrition {
		[Key]
		public int Id { get; set; }
		[MaxLength(20), Required]
		public string Name { get; set; }

		public ICollection<MenuGather> MenuGathers { get; set; }
	}
	/// <summary>
	/// 菜品口味，多对多表
	/// </summary>
	public class Flavor {
		[Key]
		public int Id { get; set; }
		[MaxLength(20), Required]
		public string Name { get; set; }

		public ICollection<MenuGather> Menus { get; set; }
	}
}
