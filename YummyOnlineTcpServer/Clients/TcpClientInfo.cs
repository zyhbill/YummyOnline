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
		public TcpClientInfo ReadyToReplaceClient { get; set; }
		public IPEndPoint OriginalRemotePoint { get; set; }
		public DateTime ConnectedTime { get; set; }
		/// <summary>
		/// 心跳数
		/// </summary>
		public int HeartAlive { get; set; }

		public void Close() {
			Client.Close();
		}

		public static bool operator ==(TcpClientInfo c1, TcpClientInfo c2) {
			return c1?.Client == c2?.Client;
		}
		public static bool operator !=(TcpClientInfo c1, TcpClientInfo c2) {
			return c1?.Client != c2?.Client;
		}

		public override bool Equals(object obj) {
			if(obj == null || GetType() != obj.GetType()) {
				return false;
			}
			var o = obj as TcpClientInfo;
			return Client == o.Client;
		}
		public override int GetHashCode() {
			return base.GetHashCode();
		}
	}
}
