using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YummyOnlineDAO.Models {
	public class Staff {
		[Key, MaxLength(8)]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public string Id { get; set; }
		[MaxLength(20), Required]
		public string SigninName { get; set; }
		[MaxLength(11)]
		public string PhoneNumber { get; set; }
		[MaxLength(128)]
		public string Email { get; set; }
		[Required]
		public string PasswordHash { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public DateTime CreateDate { get; set; }
		
		public int HotelId { get; set; }
		public Hotel Hotel { get; set; }
	}
}
