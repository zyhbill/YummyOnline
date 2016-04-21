using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelDAO.Models {
	/// <summary>
	/// 菜品备注，多对多表
	/// </summary>
	public class Remark {
		[Key]
		public int Id { get; set; }
		[MaxLength(10), Required]
		public string Name { get; set; }
		public decimal Price { get; set; }

		public ICollection<Menu> Menus { get; set; }
		public ICollection<Dine> Dines { get; set; }
		public ICollection<DineMenu> DineMenus { get; set; }
	}
}