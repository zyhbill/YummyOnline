using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol {
	public class SitePerformance {
		public string Name { get; set; }
		public float CurrentConnections { get; set; }
	}
	public class WebSocketProtocol {
		public DateTime DateTime { get; set; } = DateTime.Now;
		public float CpuTime { get; set; }
		public float DiskIdle { get; set; }
		public float MemoryUsage { get; set; }
		public List<SitePerformance> SitePerformances { get; set; } = new List<SitePerformance>();
	}
}
