using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelDAO.Models {
	public class Log {
		public enum LogLevel {
			Debug = 0,
			Error = 1,
			Info = 2,
			Success = 3,
			Warning = 4
		}
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public DateTime DateTime { get; set; }
		public LogLevel Level { get; set; }
		public string Message { get; set; }
	}
}
