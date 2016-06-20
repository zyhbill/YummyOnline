using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsynchronousTcp;
using YummyOnlineTcpClient;
using System.Net;
using Protocal;
using HotelDAO.Models;


namespace TcpClientTest {
	class Program {
		static string ip = "127.0.0.1";
		static void Main(string[] args) {
			TcpClient client = new TcpClient(IPAddress.Parse(ip), 18000, new NewDineInformClientConnectProtocal("0465E2FB-67B9-43EB-AC4A-3621BF83ECB9"));

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
			client.Send(new RequestPrintMenuProtocal(2, "16050300000005", new List<int> { 373, 374 }, new List<PrintType> { PrintType.ServeOrder, PrintType.KitchenOrder }), () => {
				Console.WriteLine("@");
			});
			Console.WriteLine("1");
			while(true) {
				Console.Read();
			}
		}
	}
}
