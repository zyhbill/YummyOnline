using System;
using System.Net;
using System.Net.Sockets;

namespace YummyOnlineTcpServer {
	public class TcpClientInfo {
		public TcpClientInfo(TcpClient client) {
			Client = client;
			OriginalRemotePoint = (IPEndPoint)client.Client.RemoteEndPoint;
			ConnectedTime = DateTime.Now;
		}

		public TcpClient Client { get; set; }
		public IPEndPoint OriginalRemotePoint { get; set; }
		public DateTime ConnectedTime { get; set; }
		/// <summary>
		/// 心跳数
		/// </summary>
		public int HeartAlive { get; set; }
	}
}
