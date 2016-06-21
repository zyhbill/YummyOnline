using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace OrderSystem.Controllers {
	using HotelDAO.Models;
	using YummyOnlineDAO.Identity;

	[RequireHotel]
	[HotelAvailable]
	public class OrderController : BaseOrderSystemController {
		// GET: Order
		public ActionResult Index() {
			return View();
		}

		public ActionResult _ViewMenu() {
			return View();
		}
		public ActionResult _ViewCart() {
			return View();
		}
		public ActionResult _ViewPayment() {
			return View();
		}
		public ActionResult _ViewHistory() {
			return View();
		}

		public JsonResult GetCurrentDesk() {
			return Json(Session["CurrentDesk"]);
		}

		public async Task<JsonResult> GetMenuInfos() {
			var t1 = new HotelManager(CurrHotel.ConnectionString).GetFormatedMenuClasses();
			var t2 = new HotelManager(CurrHotel.ConnectionString).GetFormatedMenus();
			var t3 = new HotelManager(CurrHotel.ConnectionString).GetFormatedMenuOnSales();
			var t4 = new HotelManager(CurrHotel.ConnectionString).GetFormatedMenuSetMeals();
			var t5 = new HotelManager(CurrHotel.ConnectionString).GetFormatedPayKinds(new List<PayKindType> { PayKindType.Online, PayKindType.Other });
			var t6 = new HotelManager(CurrHotel.ConnectionString).GetHotelConfig();
			var t7 = new HotelManager(CurrHotel.ConnectionString).GetTimeDiscounts();
			var t8 = new HotelManager(CurrHotel.ConnectionString).GetVipDiscounts();

			var result = new {
				MenuClasses = await t1,
				Menus = await t2,
				MenuOnSales = await t3,
				MenuSetMeals = await t4,
				PayKinds = await t5,
				DiscountMethods = new {
					TimeDiscounts = await t7,
					VipDiscounts = await t8
				},
				Hotel = DynamicsCombination.CombineDynamics(await t6, new {
					CurrHotel.Name,
					CurrHotel.Address,
					CurrHotel.Tel,
					CurrHotel.OpenTime,
					CurrHotel.CloseTime
				})
			};
			return Json(result);
		}

		public async Task<JsonResult> GetHistoryDines() {
			return Json(await HotelManager.GetFormatedHistoryDines(User.Identity.GetUserId()));
		}
	}
}

namespace OrderSystem.Controllers {
	using Protocal.DineForPrintingProtocal;
	using YummyOnlineDAO.Identity;
	public class OrderForPrintingController : BaseOrderSystemController {
		public async Task<JsonResult> GetDineForPrinting(int hotelId, string dineId, List<int> dineMenuIds) {
			if(dineId == "00000000000000") {
				return Json(generateTestProtocal());
			}

			string connStr = await YummyOnlineManager.GetHotelConnectionStringById(hotelId);

			var tHotel = new YummyOnlineManager().GetHotelForPrintingById(hotelId);
			var tDine = new HotelManager(connStr).GetDineForPrintingById(dineId, dineMenuIds);
			var tPrinterFormat = new HotelManager(connStr).GetPrinterFormatForPrinting();
			var tUser = new YummyOnlineManager().GetUserForPrintingById((await tDine).UserId);

			return Json(new DineForPrinting {
				Hotel = await tHotel,
				Dine = await tDine,
				User = await tUser,
				PrinterFormat = await tPrinterFormat
			});
		}

		private DineForPrinting generateTestProtocal() {
			DineForPrinting p = new DineForPrinting {
				Hotel = new Hotel {
					Id = 1,
					Name = "测试饭店名",
					Address = "测试饭店地址",
					OpenTime = new TimeSpan(8, 0, 0),
					CloseTime = new TimeSpan(22, 0, 0),
					Tel = "021-00000000",
					Usable = true
				},
				Dine = new Dine {
					Id = "00000000000000",
					Status = HotelDAO.Models.DineStatus.Untreated,
					Type = HotelDAO.Models.DineType.ToStay,
					HeadCount = 10,
					Price = 123.56m,
					OriPrice = 987.65m,
					Change = 123.456m,
					Discount = 0.5,
					DiscountName = "测试折扣名",
					DiscountType = HotelDAO.Models.DiscountType.PayKind,
					BeginTime = DateTime.Now,
					IsPaid = true,
					IsOnline = true,
					UserId = "0000000000",
					Waiter = new Staff {
						Id = "00000000",
						Name = "测试服务员名"
					},
					Clerk = new Staff {
						Id = "00000000",
						Name = "测试收银员名"
					},
					Remarks = new List<Remark> {
							new Remark {Id = 1,Name = "测试备注1",Price = 12.34m },
							new Remark {Id = 2,Name = "测试备注2",Price = 56.78m },
							new Remark {Id = 3,Name = "测试备注3",Price = 90.12m },
							new Remark {Id = 4,Name = "测试备注4",Price = 34.56m }
						},
					Desk = new Desk {
						Id = "000",
						QrCode = "111",
						Name = "测试桌名",
						Description = "测试桌子备注信息",
						ReciptPrinter = new Printer {
							Id = 0,
							Name = "Microsoft XPS Document Writer",
							IpAddress = "127.0.0.1",
							Usable = true
						},
						ServePrinter = new Printer {
							Id = 1,
							Name = "Microsoft XPS Document Writer",
							IpAddress = "127.0.0.2",
							Usable = true
						}
					},
					DineMenus = new List<DineMenu> {
							new DineMenu {
								Status = HotelDAO.Models.DineMenuStatus.Normal,
								Count = 10,
								OriPrice = 56.78m,
								Price = 12.34m,
								RemarkPrice = 12.34m,
								Remarks = new List<Remark> {
											new Remark {Id = 0,Name = "测试备注1",Price = 12.34m },
											new Remark {Id = 1,Name = "测试备注2",Price = 56.78m },
											new Remark {Id = 2,Name = "测试备注3",Price = 90.12m },
											new Remark {Id = 3,Name = "测试备注4",Price = 34.56m }
										},
								Menu = new Menu {
									Id = $"00000",
									Code = "test",
									Name = $"测试套餐",
									NameAbbr = $"测试0",
									Unit = "份",
									IsSetMeal = true,
									SetMealMenus = new List<SetMealMenu> {
										new SetMealMenu {
											Id = "10000",
											Name = "测试套餐菜品1",
											Count = 10
										},
										new SetMealMenu {
											Id = "10001",
											Name = "测试套餐菜品2",
											Count = 10
										},
										new SetMealMenu {
											Id = "10002",
											Name = "测试套餐菜品3",
											Count = 10
										}
									},
									Printer = new Printer {
										Id = 2,
										Name = "Microsoft XPS Document Writer",
										IpAddress = "127.0.0.1",
										Usable = true
									},
								}
							}
						},
					DinePaidDetails = new List<DinePaidDetail> {
							new DinePaidDetail {
								Price = 12.34m,
								RecordId = "1234567890abcdeABCDE",
								PayKind = new PayKind {
									Id = 0,
									Name = "测试支付1",
									Type = HotelDAO.Models.PayKindType.Online
								}
							},
							new DinePaidDetail {
								Price = 12.34m,
								RecordId = "1234567890abcdeABCDE",
								PayKind = new PayKind {
									Id = 0,
									Name = "测试支付2",
									Type = HotelDAO.Models.PayKindType.Offline
								}
							},
							new DinePaidDetail {
								Price = 12.34m,
								RecordId = "1234567890abcdeABCDE",
								PayKind = new PayKind {
									Id = 0,
									Name = "测试支付3",
									Type = HotelDAO.Models.PayKindType.Points
								}
							}
						}
				},
				User = new User {
					Id = "00000000",
					Email = "test@test.test",
					UserName = "测试用户名",
					PhoneNumber = "12345678900"
				},
				PrinterFormat = new PrinterFormat {
					PaperSize = 278,
					Font = "宋体",
					ReciptBigFontSize = 12,
					ReciptFontSize = 8,
					ReciptSmallFontSize = 7,
					ServeOrderFontSize = 9,
					ServeOrderSmallFontSize = 9,
					KitchenOrderFontSize = 9,
					KitchenOrderSmallFontSize = 9
				}
			};

			for(int i = 1; i <= 5; i++) {
				p.Dine.DineMenus.Add(new DineMenu {
					Status = HotelDAO.Models.DineMenuStatus.Normal,
					Count = 10,
					OriPrice = 56.78m,
					Price = 12.34m,
					RemarkPrice = 12.34m,
					Remarks = new List<Remark> {
									new Remark {Id = 0,Name = "测试备注1",Price = 12.34m },
									new Remark {Id = 1,Name = "测试备注2",Price = 56.78m },
									new Remark {Id = 2,Name = "测试备注3",Price = 90.12m },
									new Remark {Id = 3,Name = "测试备注4",Price = 34.56m }
								},
					Menu = new Menu {
						Id = $"0000{i}",
						Code = "test",
						Name = $"测试菜品名{i}",
						NameAbbr = $"测试{i}",
						Unit = "份",
						IsSetMeal = false,
						Printer = new Printer {
							Id = 2,
							Name = "Microsoft XPS Document Writer",
							IpAddress = "127.0.0.1",
							Usable = true
						},
					}
				});
			}

			return p;
		}
	}
}