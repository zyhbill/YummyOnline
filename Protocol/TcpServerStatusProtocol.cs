using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Protocol {
	public class ClientStatus {
		public string IpAddress { get; set; }
		public int Port { get; set; }
		public DateTime ConnectedTime { get; set; }
		public bool IsConnected { get; set; }
	}
	public class NewDineInformClientStatus {
		public ClientStatus Status { get; set; }
		public Guid Guid { get; set; }
		public string Description { get; set; }
	}
	public class PrinterClientStatus {
		public ClientStatus Status { get; set; }
		public int HotelId { get; set; }
		public int WaitedCount { get; set; }
	}
	public class TcpServerStatusProtocol {
		public List<ClientStatus> WaitingForVerificationClients { get; set; } = new List<ClientStatus>();
		public ClientStatus SystemClient { get; set; }
		public List<NewDineInformClientStatus> NewDineInformClients { get; set; } = new List<NewDineInformClientStatus>();
		public List<PrinterClientStatus> PrinterClients { get; set; } = new List<PrinterClientStatus>();
	}
}
