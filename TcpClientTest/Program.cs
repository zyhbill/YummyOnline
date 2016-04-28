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
		static string ip = "127.0.0.1";
		static TcpManager tcp = null;
		static System.Net.Sockets.TcpClient client = null;
		static void Main(string[] args) {
			TcpClient client = new TcpClient(IPAddress.Parse(ip), 18000, new NewDineInformClientConnectProtocal("{ec3ad9d8-1c48-420d-a33e-c2f83b761738}"));


			client.CallBackWhenMessageReceived = (s, o) => {
				var a = (NewDineInformProtocal)o;
				Console.WriteLine(a.HotelId);
				Console.WriteLine(a.DineId);
			};
			client.CallBackWhenExceptionOccured = e => {
				Console.WriteLine(e);
			};
			client.CallBackWhenConnected = () => {
				Console.WriteLine("Connected");
			};
			client.Start();

			Console.Read();
			client.Send(new NewDineInformProtocal(1, "testid", true));
			Console.WriteLine("1");
			while(true) {
				Console.Read();
			}
			
		}

		private static void Tcp_ConnectedEvent(System.Net.Sockets.TcpClient obj) {
			client = obj;
			Console.WriteLine("Connected");
		}

		private static async void Tcp_ErrorEvent(System.Net.Sockets.TcpClient arg1, Exception arg2) {
			Console.WriteLine(arg2);
			await tcp.StartConnecting(System.Net.IPAddress.Parse(ip), 18000, Tcp_ConnectedEvent);
		}

		private static void Tcp_MessageReceivedEvent(System.Net.Sockets.TcpClient arg1, string arg2) {
			Console.WriteLine(arg2);
		}
	}
}
