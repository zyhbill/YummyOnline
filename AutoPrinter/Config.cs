using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Protocol.PrintingProtocol;

namespace AutoPrinter {
	public static class Config {
#if DEBUG
		public const string TcpServerIp = "127.0.0.1";
		public const int TcpServerPort = 18000;

		public const string RemoteSigninUrl = "http://localhost:54860/Account/Signin";
		public const string RemoteGetHotelConfigUrl = "http://localhost:54860/Order/GetHotelConfig";
		public const string RemoteLogUrl = "http://localhost:55615/Log/RemoteRecord";
		public const string RemotePrintCompletedUrl = "http://localhost:57504/Payment/PrintCompleted";
		public const string RemoteGetDineForPrintingUrl = "http://localhost:57504/OrderForPrinting/GetDineForPrinting";
		public const string RemoteGetShiftsForPrintingUrl = "http://localhost:57504/OrderForPrinting/GetShiftsForPrinting";
		public const string RemoteGetPrintersForPrintingUrl = "http://localhost:57504/OrderForPrinting/GetPrintersForPrinting";
#elif COMPANYSERVER
		public const string TcpServerIp = "192.168.0.200";
		public const int TcpServerPort = 18000;

		public const string RemoteSigninUrl = "http://192.168.0.200:8888/Account/Signin";
		public const string RemoteGetHotelConfigUrl = "http://192.168.0.200:8888/Order/GetHotelConfig";
		public const string RemoteLogUrl = "http://192.168.0.200:8889/Log/RemoteRecord";
		public const string RemotePrintCompletedUrl = "http://192.168.0.200:8080/Payment/PrintCompleted";
		public const string RemoteGetDineForPrintingUrl = "http://192.168.0.200:8080/OrderForPrinting/GetDineForPrinting";
		public const string RemoteGetShiftsForPrintingUrl = "http://192.168.0.200:8080/OrderForPrinting/GetShiftsForPrinting";
		public const string RemoteGetPrintersForPrintingUrl = "http://192.168.0.200:8080/OrderForPrinting/GetPrintersForPrinting";
#else
		public const string TcpServerIp = "122.114.96.157";
		public const int TcpServerPort = 18000;

		public const string RemoteSigninUrl = "http://waiter.yummyonline.net/Account/Signin";
		public const string RemoteGetHotelConfigUrl = "http://waiter.yummyonline.net/Order/GetHotelConfig";
		public const string RemoteLogUrl = "http://system.yummyonline.net/Log/RemoteRecord";
		public const string RemotePrintCompletedUrl = "http://ordersystem.yummyonline.net/Payment/PrintCompleted";
		public const string RemoteGetDineForPrintingUrl = "http://ordersystem.yummyonline.net/OrderForPrinting/GetDineForPrinting";
		public const string RemoteGetShiftsForPrintingUrl = "http://ordersystem.yummyonline.net/OrderForPrinting/GetShiftsForPrinting";
		public const string RemoteGetPrintersForPrintingUrl = "http://ordersystem.yummyonline.net/OrderForPrinting/GetPrintersForPrinting";
#endif

		public static string BaseDir {
			get {
				return AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
			}
		}
		private static string configFilePath = $@"{BaseDir}\config.json";

		public static int HotelId { get; set; }

		public static DineForPrinting GetTestProtocol(string ipAddress) {
			DineForPrinting p = new DineForPrinting {
				Hotel = new Hotel {
					Id = 1,
					Name = "本地测试饭店名",
					Address = "本地测试饭店地址",
					OpenTime = new TimeSpan(8, 0, 0),
					CloseTime = new TimeSpan(22, 0, 0),
					Tel = "000-00000000",
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
					DiscountName = "本地测试折扣名",
					DiscountType = HotelDAO.Models.DiscountType.PayKind,
					BeginTime = DateTime.Now,
					IsPaid = true,
					IsOnline = true,
					UserId = "0000000000",
					Waiter = new Staff {
						Id = "00000000",
						Name = "本地测试服务员名"
					},
					Clerk = new Staff {
						Id = "00000000",
						Name = "本地测试收银员名"
					},
					Remarks = new List<Remark> {
							new Remark {Id = 1,Name = "本地测试备注1",Price = 12.34m },
							new Remark {Id = 2,Name = "本地测试备注2",Price = 56.78m },
							new Remark {Id = 3,Name = "本地测试备注3",Price = 90.12m },
							new Remark {Id = 4,Name = "本地测试备注4",Price = 34.56m }
						},
					Desk = new Desk {
						Id = "000",
						QrCode = "111",
						Name = "本地测试桌名",
						Description = "本地测试桌子备注信息",
						AreaType = HotelDAO.Models.AreaType.Normal,
						ReciptPrinter = new Printer {
							Id = 0,
							Name = "本地测试打印机",
							IpAddress = ipAddress,
							Usable = true
						},
						ServePrinter = new Printer {
							Id = 1,
							Name = "本地测试打印机",
							IpAddress = ipAddress,
							Usable = true
						}
					},
					DineMenus = new List<DineMenu> (),
					DinePaidDetails = new List<DinePaidDetail> {
							new DinePaidDetail {
								Price = 12.34m,
								RecordId = "1234567890abcdeABCDE",
								PayKind = new PayKind {
									Id = 0,
									Name = "本地测试支付1",
									Type = HotelDAO.Models.PayKindType.Online
								}
							},
							new DinePaidDetail {
								Price = 12.34m,
								RecordId = "1234567890abcdeABCDE",
								PayKind = new PayKind {
									Id = 0,
									Name = "本地测试支付2",
									Type = HotelDAO.Models.PayKindType.Offline
								}
							},
							new DinePaidDetail {
								Price = 12.34m,
								RecordId = "1234567890abcdeABCDE",
								PayKind = new PayKind {
									Id = 0,
									Name = "本地测试支付3",
									Type = HotelDAO.Models.PayKindType.Points
								}
							}
						}
				},
				User = new User {
					Id = "00000000",
					Email = "test@test.test",
					UserName = "本地测试用户名",
					PhoneNumber = "12345678900"
				},
				PrinterFormat = new PrinterFormat {
					PaperSize = 556,
					Font = "宋体",
					ColorDepth = 200,
					ReciptBigFontSize = 25,
					ReciptFontSize = 17,
					ReciptSmallFontSize = 15,
					ServeOrderFontSize = 19,
					ServeOrderSmallFontSize = 19,
					KitchenOrderFontSize = 19,
					KitchenOrderSmallFontSize = 19
				}
			};

			for(int i = 1; i <= 2; i++) {
				p.Dine.DineMenus.Add(new DineMenu {
					Status = HotelDAO.Models.DineMenuStatus.Normal,
					Count = 10,
					OriPrice = 56.78m,
					Price = 12.34m,
					RemarkPrice = 12.34m,
					Remarks = new List<Remark> {
									new Remark {Id = 0,Name = "本地测试备注1",Price = 12.34m },
									new Remark {Id = 1,Name = "本地测试备注2",Price = 56.78m },
									new Remark {Id = 2,Name = "本地测试备注3",Price = 90.12m },
									new Remark {Id = 3,Name = "本地测试备注4",Price = 34.56m }
								},
					Menu = new Menu {
						Id = $"0000{i}",
						Code = "test",
						Name = $"本地测试菜品名{i}",
						NameAbbr = $"本地测试{i}",
						Unit = "份",
						IsSetMeal = false,
						Printer = new Printer {
							Id = 2,
							Name = $"本地测试打印机{i}",
							IpAddress = ipAddress,
							Usable = true
						},
						DepartmentName = $"测试厨房名{i}"
					}
				});
			}

			return p;
		}
	}
}
