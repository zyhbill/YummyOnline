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
		// 122.114.96.157
		static string ip = "127.0.0.1";
		static void Main(string[] args) {
			TcpClient client = new TcpClient(IPAddress.Parse(ip), 18000, new NewDineInformClientConnectProtocol("0465E2FB-67B9-43EB-AC4A-3621BF83ECB9"));

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
			client.Send(new RequestPrintDineProtocol(1, "16071400000001", null, new List<PrintType> { PrintType.Recipt }), () => {
				Console.WriteLine("发送成功");
			});
			Console.WriteLine("点击发送");
			while(true) {
				Console.Read();
			}
		}
	}
}
