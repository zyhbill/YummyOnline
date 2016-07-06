using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsynchronousTcp;
using YummyOnlineTcpClient;
using System.Net;
using Protocol;
using HotelDAO.Models;


namespace TcpClientTest {
	class Program {
		static string ip = "127.0.0.1";
		static void Main(string[] args) {
			TcpClient client = new TcpClient(IPAddress.Parse(ip), 18000, new NewDineInformClientConnectProtocol("ec3ad9d8-1c48-420d-a33e-c2f83b761738"));

			client.CallBackWhenMessageReceived = (s, o) => {
				var a = (NewDineInformProtocol)o;
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
			client.Send(new RequestPrintDineProtocol(2, "16062900000008", null, new List<PrintType> { PrintType.Recipt }), () => {
				Console.WriteLine("@");
			});
			Console.WriteLine("1");
			while(true) {
				Console.Read();
			}
		}
	}
}
