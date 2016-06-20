using HotelDAO.Models;
using System;
using System.Collections.Generic;
using YummyOnlineDAO.Models;

namespace Protocal {
	public class DineForPrintingProtocal {
		public class DineForPrinting {
			public string Id { get; set; }
			public DineStatus Status { get; set; }
			public DineType Type { get; set; }
			public int HeadCount { get; set; }
			public decimal Price { get; set; }
			public decimal OriPrice { get; set; }
			public decimal Change { get; set; }
			public double Discount { get; set; }
			public string DiscountName { get; set; }
			public DiscountType DiscountType { get; set; }
			public DateTime BeginTime { get; set; }
			public bool IsPaid { get; set; }
			public bool IsOnline { get; set; }
			public string UserId { get; set; }
			public Staff Waiter { get; set; }
			public Staff Clerk { get; set; }
			public List<Remark> Remarks { get; set; }
			public Desk Desk { get; set; }
			public List<DineMenu> DineMenus { get; set; } = new List<DineMenu>();
			public List<DinePaidDetail> DinePaidDetails { get; set; } = new List<DinePaidDetail>();
		}

		public class Staff {
			public string Id { get; set; }
			public string Name { get; set; }
		}
		public class Remark {
			public int Id { get; set; }
			public string Name { get; set; }
			public decimal Price { get; set; }
		}
		public class Desk {
			public string Id { get; set; }
			public string QrCode { get; set; }
			public string Name { get; set; }
			public string Description { get; set; }
			public Printer ReciptPrinter { get; set; }
			public Printer ServePrinter { get; set; }
		}
		public class Printer {
			public int Id { get; set; }
			public string Name { get; set; }
			public string IpAddress { get; set; }
			public bool Usable { get; set; }
		}
		public class DineMenu {
			public int Id { get; set; }
			public DineMenuStatus Status { get; set; }
			public int Count { get; set; }
			public decimal OriPrice { get; set; }
			public decimal Price { get; set; }
			public decimal RemarkPrice { get; set; }
			public List<Remark> Remarks { get; set; } = new List<Remark>();
			public Menu Menu { get; set; }
			public Staff ReturnedWaiter { get; set; }
			public string ReturnedReason { get; set; }
		}
		public class Menu {
			public string Id { get; set; }
			public string Code { get; set; }
			public string Name { get; set; }
			public string NameAbbr { get; set; }
			public string Unit { get; set; }
			public bool IsSetMeal { get; set; }
			public List<SetMealMenu> SetMealMenus { get; set; } = new List<SetMealMenu>();
			public Printer Printer { get; set; }
		}
		public class DinePaidDetail {
			public decimal Price { get; set; }
			public string RecordId { get; set; }
			public PayKind PayKind { get; set; }
		}
		public class PayKind {
			public int Id { get; set; }
			public string Name { get; set; }
			public PayKindType Type { get; set; }
		}

		public class HotelForPrinting {
			public int Id { get; set; }
			public string Name { get; set; }
			public string Address { get; set; }
			public TimeSpan OpenTime { get; set; }
			public TimeSpan CloseTime { get; set; }
			public string Tel { get; set; }
			public bool Usable { get; set; }
		}
		public class UserForPrinting {
			public string Id { get; set; }
			public string Email { get; set; }
			public string UserName { get; set; }
			public string PhoneNumber { get; set; }
		}
		public class SetMeal {
			public string MenuSetId { get; set; }
			public List<SetMealMenu> Menus { get; set; } = new List<SetMealMenu>();
		}
		public class SetMealMenu {
			public string Id { get; set; }
			public string Name { get; set; }
			public int Count { get; set; }
		}

		public HotelForPrinting Hotel { get; set; }
		public DineForPrinting Dine { get; set; }
		public UserForPrinting User { get; set; }
	}
}
