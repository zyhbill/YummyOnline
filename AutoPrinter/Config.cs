using Newtonsoft.Json;
using Protocol.PrintingProtocol;
using System;
using System.Collections.Generic;
using System.IO;

namespace AutoPrinter {
	public class HistoryConfig {
		public bool IsIPPrinter { get; set; }
		public string HistorySigninName { get; set; }
		public string HistoryPassword { get; set; }
		public string HistoryIPAddress { get; set; }
	}

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

		public static bool IsIPPrinter { get; set; }
		public static string HistorySigninName { get; set; }
		public static string HistoryPassword { get; set; }
		public static string HistoryIPAddress { get; set; }

		public static void LoadConfigs() {
			if(!File.Exists(configFilePath)) {
				return;
			}

			string configStr = File.ReadAllText(configFilePath);
			try {
				HistoryConfig config = JsonConvert.DeserializeObject<HistoryConfig>(configStr);
				IsIPPrinter = config.IsIPPrinter;
				HistorySigninName = config.HistorySigninName;
				HistoryPassword = config.HistoryPassword;
				HistoryIPAddress = config.HistoryIPAddress;
			}
			catch { }
		}

		public static void SaveConfigs() {
			HistoryConfig config = new HistoryConfig {
				IsIPPrinter = IsIPPrinter,
				HistorySigninName = HistorySigninName,
				HistoryPassword = HistoryPassword,
				HistoryIPAddress = HistoryIPAddress,
			};
			string configStr = JsonConvert.SerializeObject(config);
			File.WriteAllText(configFilePath, configStr);
		}

		public static DineForPrinting GetTestProtocol(string ipOrName) {
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
							Name = ipOrName,
							IpAddress = ipOrName,
							Usable = true
						},
						ServePrinter = new Printer {
							Id = 1,
							Name = ipOrName,
							IpAddress = ipOrName,
							Usable = true
						},
						ReciptDepartmentName = "测试收银部门名",
						ServeDepartmentName = "测试传菜部门名"
					},
					DineMenus = new List<DineMenu> {
						new DineMenu {
							Status = HotelDAO.Models.DineMenuStatus.Normal,
							Count = 1,
							OriPrice = 56.78m,
							Price = 12.34m,
							RemarkPrice = 12.34m,
							Remarks = new List<Remark>(),
							Menu = new Menu {
								Id = $"00000",
								Code = "test",
								Name = $"测试套餐",
								NameAbbr = $"测试0",
								Unit = "份",
								IsSetMeal = true,
								Printer = new Printer {
									Id = 2,
									Name = ipOrName,
									IpAddress = ipOrName,
									Usable = true
								},
							},
							SetMealClasses = new List<DineMenuSetMealClass> {
								new DineMenuSetMealClass {
									ClassName = "测试套餐分类1",
									SetMealMenus = new List<DineMenuSetMealMenu> {
										new DineMenuSetMealMenu {
											Count = 2,
											Menu = new Menu {
												Id = $"00000",
												Code = "test",
												Name = $"测试套餐菜品1",
												NameAbbr = $"测试1",
												Unit = "份",
												IsSetMeal = true,
												Printer = new Printer {
													Id = 2,
													Name =ipOrName,
													IpAddress =ipOrName,
													Usable = true
												},
											}
										},
										new DineMenuSetMealMenu {
											Count = 5,
											Menu = new Menu {
												Id = $"00000",
												Code = "test",
												Name = $"测试套餐菜品2",
												NameAbbr = $"测试2",
												Unit = "份",
												IsSetMeal = true,
												Printer = new Printer {
													Id = 2,
													Name = ipOrName,
													IpAddress =ipOrName,
													Usable = true
												},
											}
										}
									}
								},
								new DineMenuSetMealClass {
									ClassName = "测试套餐分类2",
									SetMealMenus = new List<DineMenuSetMealMenu> {
										new DineMenuSetMealMenu {
											Count = 2,
											Menu = new Menu {
												Id = $"00000",
												Code = "test",
												Name = $"测试套餐菜品1",
												NameAbbr = $"测试1",
												Unit = "份",
												IsSetMeal = true,
												Printer = new Printer {
													Id = 2,
													Name = ipOrName,
													IpAddress = ipOrName,
													Usable = true
												},
											}
										},
										new DineMenuSetMealMenu {
											Count = 5,
											Menu = new Menu {
												Id = $"00000",
												Code = "test",
												Name = $"测试套餐菜品2",
												NameAbbr = $"测试2",
												Unit = "份",
												IsSetMeal = true,
												Printer = new Printer {
													Id = 2,
													Name = ipOrName,
													IpAddress = ipOrName,
													Usable = true
												},
											}
										}
									}
								}
							}
						}
					},
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
					PaperSize = 278,
					Font = "宋体",
					ColorDepth = 55,
					ReciptBigFontSize = 10,
					ReciptFontSize = 8,
					ReciptSmallFontSize = 7,
					KitchenOrderFontSize = 10,
					KitchenOrderSmallFontSize = 8,
					ServeOrderFontSize = 10,
					ServeOrderSmallFontSize = 8,
					ShiftBigFontSize = 12,
					ShiftFontSize = 8,
					ShiftSmallFontSize = 7
				}
			};

			for(int i = 1; i <= 2; i++) {
				p.Dine.DineMenus.Add(new DineMenu {
					Status = HotelDAO.Models.DineMenuStatus.Normal,
					Count = 10,
					OriPrice = 156.78m,
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
							Name = ipOrName,
							IpAddress = ipOrName,
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
