using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelDAO.Models {
	/// <summary>
	/// 预定
	/// </summary>
	public class Reserve {
		[Key]
		public int Id { get; set; }

		[MaxLength(128)]
		public string PhoneNumber { get; set; }
		/// <summary>
		/// 人数
		/// </summary>
		public int HeadCount { get; set; }
		public DateTime Time { get; set; }
		public DateTime CreateTime { get; set; }
		
		public string DeskId { get; set; }
		public Desk Desk { get; set; }
	}
}