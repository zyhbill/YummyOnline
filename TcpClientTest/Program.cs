using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsynchronousTcp;
using YummyOnlineTcpClient;
using System.Net;
using Protocal;

namespace TcpClientTest {
	class Program {
		static TcpManager tcp = null;
		static System.Net.Sockets.TcpClient client = null;
		static void Main(string[] args) {
			TcpClient client = new TcpClient(IPAddress.Parse("127.0.0.1"), 18000, new NewDineInformClientConnectProtocal("{0465E2FB-67B9-43EB-AC4A-3621BF83ECB9}"));


			client.CallBackWhenMessageReceived = (s, o) => {
				var a = (NewDineInformProtocal)o;
				Console.WriteLine(a.HotelId);
				Console.WriteLine(a.DineId);
			};
			client.CallBackWhenExceptionOccured = e => {
				Console.WriteLine(e);
			};
			client.Start();
			//Console.WriteLine("Client");
			//tcp = new TcpManager();
			//var _ = tcp.StartConnecting(System.Net.IPAddress.Parse("122.114.96.157"), 18000, Tcp_ConnectedEvent);
			//tcp.MessageReceivedEvent += Tcp_MessageReceivedEvent;
			//tcp.ErrorEvent += Tcp_ErrorEvent;

			//while(true) {
			//	string r = Console.ReadLine();
			//	int k = Convert.ToInt32(r);

			//	r = Console.ReadLine();
			//	int n = Convert.ToInt32(r);

			//	for(int j = 0; j < k; j++) {
			//		string s = "";
			//		for(int i = 0; i < n; i++) {
			//			s += i.ToString() + " ";
			//		}
			//		if(client != null) {
			//			_ = tcp.Send(client, s, null);
			//		}
			//	}


			//}
			Console.Read();
		}

		private static void Tcp_ConnectedEvent(System.Net.Sockets.TcpClient obj) {
			client = obj;
			Console.WriteLine("Connected");
		}

		private static async void Tcp_ErrorEvent(System.Net.Sockets.TcpClient arg1, Exception arg2) {
			Console.WriteLine(arg2);
			await tcp.StartConnecting(System.Net.IPAddress.Parse("122.114.96.157"), 18000, Tcp_ConnectedEvent);
		}

		private static void Tcp_MessageReceivedEvent(System.Net.Sockets.TcpClient arg1, string arg2) {
			Console.WriteLine(arg2);
		}
	}
}
