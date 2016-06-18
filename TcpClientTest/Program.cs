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
		//static string ip = "122.114.96.157";
		static void Main(string[] args) {

			//TcpClient client = new TcpClient(IPAddress.Parse(ip), 18000, new NewDineInformClientConnectProtocal("{ec3ad9d8-1c48-420d-a33e-c2f83b761738}"));

			HotelContext ctx = new HotelContext("Server=FISHER-PC; Database=YummyOnlineHotel1; Integrated Security=True");
			//ctx.Dines.Add(new Dine {
			//	DeskId = "101",
			//	Discount = 1,
			//	DiscountType = DiscountType.None,
			//	Price = 0,
			//	OriPrice = 0,
			//	WaiterId = "16020201"
			//});
			ctx.Areas.Add(new Area {
				Id = "222",
				Name = "ee",

			});
			ctx.SaveChanges();
			Console.Read();
			//client.CallBackWhenMessageReceived = (s, o) => {
			//	var a = (NewDineInformProtocal)o;
			//	Console.WriteLine(a.HotelId);
			//	Console.WriteLine(a.DineId);
			//};
			//client.CallBackWhenExceptionOccured = e => {
			//	Console.WriteLine(e);
			//};
			//client.CallBackWhenConnected = () => {
			//	Console.WriteLine("Connected");
			//};
			//client.Start();

			//Console.Read();
			//client.Send(new NewDineInformProtocal(1, "testid", true),()=> {
			//	Console.WriteLine("@");
			//});
			//Console.WriteLine("1");
			//while(true) {
			//	Console.Read();
			//}
			
		}
	}
}
