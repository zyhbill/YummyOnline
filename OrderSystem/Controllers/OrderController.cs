using HotelDAO;
using HotelDAO.Models;
using Protocal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using YummyOnlineDAO;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;

namespace OrderSystem.Controllers {
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
			var t1 = new HotelManager(CurrHotel.ConnectionString).GetMenuClasses();
			var t2 = new HotelManager(CurrHotel.ConnectionString).GetMenus();
			var t3 = new HotelManager(CurrHotel.ConnectionString).GetMenuOnSales();
			var t4 = new HotelManager(CurrHotel.ConnectionString).GetMenuSetMeals();
			var t5 = new HotelManager(CurrHotel.ConnectionString).GetPayKinds(new List<PayKindType> { PayKindType.Online, PayKindType.Other });
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
			return Json(await HotelManager.GetHistoryDines(User.Identity.GetUserId()));
		}


	}

	public class OrderForPrintingController : BaseOrderSystemController {
		public async Task<JsonResult> GetDineForPrinting(int hotelId, string dineId) {
			if(dineId == "00000000000000") {
				DineForPrintingProtocal testProtocal = generateTestProtocal();
				return Json(testProtocal);
			}

			Hotel hotel = await YummyOnlineManager.GetHotelById(hotelId);
			var hotelP = new {
				hotel.Id,
				hotel.Name,
				hotel.Address,
				hotel.OpenTime,
				hotel.CloseTime,
				hotel.Tel,
				hotel.Usable
			};
			HotelManager hotelManager = new HotelManager(hotel.ConnectionString);

			var dine = await hotelManager.GetDineForPrintingById(dineId);

			User user = await new UserManager().FindByIdAsync(dine.UserId);
			var userP = user == null ? null : new {
				user.Id,
				user.Email,
				user.UserName,
				user.PhoneNumber
			};

			List<dynamic> setMeals = new List<dynamic>();

			foreach(dynamic dineMenu in dine.DineMenus) {
				if(dineMenu.Menu.IsSetMeal) {
					setMeals.Add(new {
						MenuSetId = dineMenu.Menu.Id,
						Menus = await hotelManager.GetMenuSetMealByMenuSetId(dineMenu.Menu.Id)
					});
				}
			}
			return Json(new {
				Hotel = hotelP,
				Dine = dine,
				User = userP,
				SetMeals = setMeals
			});
		}

		private DineForPrintingProtocal generateTestProtocal() {
			DineForPrintingProtocal p = new DineForPrintingProtocal {
				Hotel = new DineForPrintingProtocal.HotelForPrinting {
					Id = 1,
					Name = "测试饭店名",
					Address = "测试饭店地址",
					OpenTime = new TimeSpan(8, 0, 0),
					CloseTime = new TimeSpan(22, 0, 0),
					Tel = "021-00000000",
					Usable = true
				},
				Dine = new DineForPrintingProtocal.DineForPrinting {
					Id = "00000000000000",
					Status = DineStatus.Untreated,
					Type = DineType.ToStay,
					HeadCount = 10,
					Price = 123.56m,
					OriPrice = 987.65m,
					Discount = 0.5,
					DiscountName = "测试折扣名",
					DiscountType = DiscountType.PayKind,
					BeginTime = DateTime.Now,
					IsPaid = true,
					IsOnline = true,
					UserId = "0000000000",
					Waiter = new DineForPrintingProtocal.Staff {
						Id = "00000000",
						Name = "测试服务员名"
					},
					Clerk = new DineForPrintingProtocal.Staff {
						Id = "00000000",
						Name = "测试收银员名"
					},
					Remarks = new List<DineForPrintingProtocal.Remark> {
							new DineForPrintingProtocal.Remark {Id = 1,Name = "测试备注1",Price = 12.34m },
							new DineForPrintingProtocal.Remark {Id = 2,Name = "测试备注2",Price = 56.78m },
							new DineForPrintingProtocal.Remark {Id = 3,Name = "测试备注3",Price = 90.12m },
							new DineForPrintingProtocal.Remark {Id = 4,Name = "测试备注4",Price = 34.56m }
						},
					Desk = new DineForPrintingProtocal.Desk {
						Id = "000",
						QrCode = "111",
						Name = "测试桌名",
						Description = "测试桌子备注信息",
						ReciptPrinter = new DineForPrintingProtocal.Printer {
							Id = 0,
							Name = "Microsoft XPS Document Writer",
							IpAddress = "127.0.0.1",
							Usable = true
						},
						ServePrinter = new DineForPrintingProtocal.Printer {
							Id = 1,
							Name = "Microsoft XPS Document Writer",
							IpAddress = "127.0.0.2",
							Usable = true
						}
					},
					DineMenus = new List<DineForPrintingProtocal.DineMenu> {
							new DineForPrintingProtocal.DineMenu {
								Status = DineMenuStatus.Normal,
								Count = 10,
								OriPrice = 56.78m,
								Price = 12.34m,
								RemarkPrice = 12.34m,
								Remarks = new List<DineForPrintingProtocal.Remark> {
											new DineForPrintingProtocal.Remark {Id = 0,Name = "测试备注1",Price = 12.34m },
											new DineForPrintingProtocal.Remark {Id = 1,Name = "测试备注2",Price = 56.78m },
											new DineForPrintingProtocal.Remark {Id = 2,Name = "测试备注3",Price = 90.12m },
											new DineForPrintingProtocal.Remark {Id = 3,Name = "测试备注4",Price = 34.56m }
										},
								Menu = new DineForPrintingProtocal.Menu {
									Id = $"00000",
									Code = "test",
									Name = $"测试套餐",
									NameAbbr = $"测试0",
									Unit = "份",
									IsSetMeal = true,
									Printer = new DineForPrintingProtocal.Printer {
										Id = 2,
										Name = "Microsoft XPS Document Writer",
										IpAddress = "127.0.0.1",
										Usable = true
									},
								}
							}
						},
					DinePaidDetails = new List<DineForPrintingProtocal.DinePaidDetail> {
							new DineForPrintingProtocal.DinePaidDetail {
								Price = 12.34m,
								RecordId = "1234567890abcdeABCDE",
								PayKind = new DineForPrintingProtocal.PayKind {
									Id = 0,
									Name = "测试支付1",
									Type = PayKindType.Online
								}
							},
							new DineForPrintingProtocal.DinePaidDetail {
								Price = 12.34m,
								RecordId = "1234567890abcdeABCDE",
								PayKind = new DineForPrintingProtocal.PayKind {
									Id = 0,
									Name = "测试支付2",
									Type = PayKindType.Offline
								}
							},
							new DineForPrintingProtocal.DinePaidDetail {
								Price = 12.34m,
								RecordId = "1234567890abcdeABCDE",
								PayKind = new DineForPrintingProtocal.PayKind {
									Id = 0,
									Name = "测试支付3",
									Type = PayKindType.Points
								}
							}
						}
				},
				User = new DineForPrintingProtocal.UserForPrinting {
					Id = "00000000",
					Email = "test@test.test",
					UserName = "测试用户名",
					PhoneNumber = "12345678900"
				},
				SetMeals = new List<DineForPrintingProtocal.SetMeal> {
						new DineForPrintingProtocal.SetMeal {
							MenuSetId = "00000",
							Menus = new List<DineForPrintingProtocal.SetMealMenu> {
								new DineForPrintingProtocal.SetMealMenu {
									Id = "10000",
									Name = "测试套餐菜品1",
									Count = 10
								},
								new DineForPrintingProtocal.SetMealMenu {
									Id = "10001",
									Name = "测试套餐菜品2",
									Count = 10
								},
								new DineForPrintingProtocal.SetMealMenu {
									Id = "10002",
									Name = "测试套餐菜品3",
									Count = 10
								}
							}
						}
					}
			};

			for(int i = 1; i <= 5; i++) {
				p.Dine.DineMenus.Add(new DineForPrintingProtocal.DineMenu {
					Status = DineMenuStatus.Normal,
					Count = 10,
					OriPrice = 56.78m,
					Price = 12.34m,
					RemarkPrice = 12.34m,
					Remarks = new List<DineForPrintingProtocal.Remark> {
									new DineForPrintingProtocal.Remark {Id = 0,Name = "测试备注1",Price = 12.34m },
									new DineForPrintingProtocal.Remark {Id = 1,Name = "测试备注2",Price = 56.78m },
									new DineForPrintingProtocal.Remark {Id = 2,Name = "测试备注3",Price = 90.12m },
									new DineForPrintingProtocal.Remark {Id = 3,Name = "测试备注4",Price = 34.56m }
								},
					Menu = new DineForPrintingProtocal.Menu {
						Id = $"0000{i}",
						Code = "test",
						Name = $"测试菜品名{i}",
						NameAbbr = $"测试{i}",
						Unit = "份",
						IsSetMeal = false,
						Printer = new DineForPrintingProtocal.Printer {
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