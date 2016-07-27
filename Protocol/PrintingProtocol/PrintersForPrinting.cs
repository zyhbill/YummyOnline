using HotelDAO.Models;
using System;
using System.Collections.Generic;

namespace Protocol.PrintingProtocol {
	public class PrintersForPrinting {
		public List<Printer> Printers { get; set; } = new List<Printer>();
	}

	public class Printer {
		public int Id { get; set; }
		public string Name { get; set; }
		public string IpAddress { get; set; }
		public bool Usable { get; set; }
	}
}
