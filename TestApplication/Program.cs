using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelDAO.Models;

namespace TestApplication {
	class Program {
		static void Main(string[] args) {
			HotelContext ctx = new HotelContext("Data Source=www.yummyonline.net;Network Library=DBMSSOCN;Initial Catalog=YummyOnlineHotel2;User ID=RemoteUser;Password=*Rbr%K!p");
			ctx.ReturnedReasons.Add(new ReturnedReason {
				Description = "2"
			});
			ctx.SaveChanges();
		}
	}
}
