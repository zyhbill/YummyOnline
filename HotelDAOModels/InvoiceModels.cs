using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelDAO.Models {
	public class Invoice {
		[Key]
		public int Id { get; set; }
		[ForeignKey(nameof(Dine))]
		public string DineId { get; set; }
		public Dine Dine { get; set; }
		public string Title { get; set; }
		public decimal Price { get; set; }
	}
}
