using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelDAO.Models {
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

		public ICollection<Department> Departments { get; set; }
	}

	public class PrinterFormat {
		[Key]
		public int Id { get; set; }

		public int PaperSize { get; set; }
		[MaxLength(10)]
		public string Font { get; set; }
		public int ColorDepth { get; set; }

		public int ReciptBigFontSize { get; set; }
		public int ReciptFontSize { get; set; }
		public int ReciptSmallFontSize { get; set; }

		public int KitchenOrderFontSize { get; set; }
		public int KitchenOrderSmallFontSize { get; set; }

		public int ServeOrderFontSize { get; set; }
		public int ServeOrderSmallFontSize { get; set; }

		public int ShiftBigFontSize { get; set; }
		public int ShiftFontSize { get; set; }
		public int ShiftSmallFontSize { get; set; }

		public int PaddingRight { get; set; }
	}
}