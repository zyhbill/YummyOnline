using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelDAO.Models {
	public class SetMealClass {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[MaxLength(20)]
		public string Name { get; set; }

		public int Count { get; set; }

		[Required]
		[ForeignKey(nameof(SetMeal))]
		public string SetMealId { get; set; }

		public Menu SetMeal { get; set; }

		public ICollection<SetMealClassMenu> SetMealClassMenus { get; set; }
	}

	public partial class SetMealClassMenu {
		[Key]
		[Column(Order = 0)]
		[ForeignKey(nameof(SetMealClass))]
		public int SetMealClassId { get; set; }

		[Key]
		[Column(Order = 1)]
		[ForeignKey(nameof(Menu))]
		public string MenuId { get; set; }

		public int Count { get; set; }

		public Menu Menu { get; set; }
		public SetMealClass SetMealClass { get; set; }
	}
}
