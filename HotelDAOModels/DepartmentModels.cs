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

		ICollection<Area> Areas { get; set; }
	}
	/// <summary>
	/// 打印机
	/// </summary>
	public class Printer {
		[Key]
		public int Id { get; set; }
		public bool Usable { get; set; }
		/// <summary>
		/// 打印机的IP地址
		/// </summary>
		[MaxLength(15)]
		public string IpAddress { get; set; }

		public string Name { get; set; }

		ICollection<Department> Departments { get; set; }
	}

	public class PrinterFormat {
		[Key]
		public int Id { get; set; }

		[MaxLength(10)]
		public string Font { get; set; }
		public bool IsCenter { get; set; }
		public int PaperSize { get; set; }
	}
}