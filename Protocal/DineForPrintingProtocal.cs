using HotelDAO.Models;
using System.Collections.Generic;
using YummyOnlineDAO.Models;

namespace Protocal {
	public class DineForPrintingProtocal {
		public class SetMealMenu {
			public string Name { get; set; }
			public int Count { get; set; }
		}

		public Hotel Hotel { get; set; }
		public Dine Dine { get; set; }
		public User User { get; set; }
		public Dictionary<string, List<SetMealMenu>> SetMeals { get; set; } = new Dictionary<string, List<SetMealMenu>>();
	}
}
