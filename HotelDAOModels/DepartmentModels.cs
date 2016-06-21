using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelDAO.Models {
	/// <summary>
	/// 部门
	/// </summary>
	public class Department {
		[Key]
		public int Id { get; set; }
		[MaxLength(20), Required]
		public string Name { get; set; }
		[MaxLength(50)]
		public string Description { get; set; }
		public bool Usable { get; set; }
		
		public int? PrinterId { get; set; }
		public Printer Printer { get; set; }
	}
}