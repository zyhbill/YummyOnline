using Protocol;
using Protocol.PrintingProtocol;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;

namespace AutoPrinter {
	public class DinePrinter : BasePrinter {
		private const int maxHeight = 2000;

		public DinePrinter(Action<IPEndPoint, Exception> errorDelegate) : base(errorDelegate) { }

		public void Print(DineForPrinting protocol, List<PrintType> printTypes, bool isFullDineMenus) {
			foreach(PrintType type in printTypes) {
				if(type == PrintType.Recipt) {
					if(protocol.Dine.Desk.ReciptPrinter == null) {
						continue;
					}
					IPAddress ip = IPAddress.Parse(protocol.Dine.Desk.ReciptPrinter.IpAddress);
					IPPrinter printer = new IPPrinter(new IPEndPoint(ip, 9100), errorDelegate);

					Bitmap bmp = generateReciptBmp(protocol, isFullDineMenus);
					printer.Print(bmp, protocol.PrinterFormat.ColorDepth);
				}
				else if(type == PrintType.ServeOrder) {
					if(protocol.Dine.Desk.ServePrinter == null) {
						continue;
					}
					IPAddress ip = IPAddress.Parse(protocol.Dine.Desk.ServePrinter.IpAddress);
					IPPrinter printer = new IPPrinter(new IPEndPoint(ip, 9100), errorDelegate);

					Bitmap bmp = generateServeOrderBmp(protocol);
					printer.Print(bmp, protocol.PrinterFormat.ColorDepth);
				}
				else if(type == PrintType.KitchenOrder) {
					foreach(DineMenu dineMenu in protocol.Dine.DineMenus.Where(p => p.Status != HotelDAO.Models.DineMenuStatus.Returned)) {
						if(dineMenu.Menu.Printer == null) {
							continue;
						}

						if(!dineMenu.Menu.IsSetMeal) {
							Bitmap bmp = generateKitchenOrderBmp(protocol, dineMenu, null);
							IPAddress ip = IPAddress.Parse(dineMenu.Menu.Printer.IpAddress);
							IPPrinter printer = new IPPrinter(new IPEndPoint(ip, 9100), errorDelegate);
							printer.Print(bmp, protocol.PrinterFormat.ColorDepth);
						}
						else {
							foreach(SetMealMenu setMealMenu in dineMenu.Menu.SetMealMenus) {
								Bitmap bmp = generateKitchenOrderBmp(protocol, dineMenu, setMealMenu);
								IPAddress ip = IPAddress.Parse(dineMenu.Menu.Printer.IpAddress);
								IPPrinter printer = new IPPrinter(new IPEndPoint(ip, 9100), errorDelegate);
								printer.Print(bmp, protocol.PrinterFormat.ColorDepth);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// 打印收银条
		/// </summary>
		private Bitmap generateReciptBmp(DineForPrinting protocol, bool isFullDineMenus) {
			Bitmap bmp = new Bitmap(protocol.PrinterFormat.PaperSize, maxHeight);
			Graphics g = Graphics.FromImage(bmp);

			PrinterGraphics printerG = new PrinterGraphics(g, protocol.PrinterFormat.PaperSize, maxHeight, protocol.PrinterFormat.Font);

			printerG.DrawStringLine($"欢迎光临{protocol.Hotel.Name}", protocol.PrinterFormat.ReciptBigFontSize, align: StringAlignment.Center);
			printerG.DrawStringLine($"TEL: {protocol.Hotel.Tel}", protocol.PrinterFormat.ReciptSmallFontSize, align: StringAlignment.Center);
			printerG.DrawStringLine($"{protocol.Hotel.Address}", protocol.PrinterFormat.ReciptSmallFontSize, align: StringAlignment.Center);
			printerG.DrawStringLine($"收据", protocol.PrinterFormat.ReciptSmallFontSize, align: StringAlignment.Center);
			printerG.TrimY(5);

			printGrid55(printerG, new string[] { $"单号: {protocol.Dine.Id}", $"时间: {protocol.Dine.BeginTime.ToString("M-d HH:mm")}" }, protocol.PrinterFormat.ReciptFontSize);
			printGrid55(printerG, new string[] { $"顾客: {protocol.User?.Id}", $"服务员: {protocol.Dine.Waiter.Name}" }, protocol.PrinterFormat.ReciptFontSize);

			if(protocol.Dine.Type == HotelDAO.Models.DineType.ToStay) {
				printerG.DrawStringLine($"餐桌: {protocol.Dine.Desk.Name}", protocol.PrinterFormat.ReciptFontSize);
			}
			else if(protocol.Dine.Type == HotelDAO.Models.DineType.ToGo) {
				printerG.DrawStringLine($"外卖: {protocol.Dine.Desk.Name}", protocol.PrinterFormat.ReciptBigFontSize);
				if(protocol.Dine.TakeOut.RecordId != null) {
					printerG.DrawStringLine($"外卖平台编号: {protocol.Dine.TakeOut.RecordId}", protocol.PrinterFormat.ReciptBigFontSize);
				}
				printerG.DrawStringLine($"手机: {protocol.User.PhoneNumber}", protocol.PrinterFormat.ReciptBigFontSize);
				printerG.DrawStringLine($"地址: {protocol.Dine.TakeOut.Address}", protocol.PrinterFormat.ReciptBigFontSize);
			}

			printHr(printerG);

			printGrid5122(printerG, new string[] { "名称", "数量", "单价", "折后小计" }, protocol.PrinterFormat.ReciptFontSize);

			printHr(printerG);

			decimal priceAll = 0m;
			foreach(DineMenu dineMenu in protocol.Dine.DineMenus.Where(p => p.Status != HotelDAO.Models.DineMenuStatus.Returned)) {
				// 打印具体菜品信息
				printGrid5122(printerG, new string[] {
					dineMenu.Menu.Name,
					dineMenu.Count.ToString(),
					dineMenu.OriPrice.ToString(),
					(dineMenu.Price * dineMenu.Count).ToString()
				}, protocol.PrinterFormat.ReciptFontSize);

				priceAll += dineMenu.Price * dineMenu.Count;

				// 如果菜品为套餐，则打印套餐包含的具体菜品信息
				if(dineMenu.Menu.IsSetMeal) {
					List<SetMealMenu> setMealMenus = dineMenu.Menu.SetMealMenus;
					for(int i = 0; i < setMealMenus.Count; i++) {
						char tab = '├';
						if(i == setMealMenus.Count - 1) {
							tab = '└';
						}
						printGrid5122(printerG, new string[] {
							$"{tab} {setMealMenus[i].Name}",
							setMealMenus[i].Count.ToString(),
							null, null
						}, protocol.PrinterFormat.ReciptFontSize);
					}
				}

				// 打印菜品的备注信息
				var remarks = dineMenu.Remarks.ToList();
				for(int i = 0; i < dineMenu.Remarks.Count; i++) {
					char tab = '├';
					if(i == dineMenu.Remarks.Count - 1) {
						tab = '└';
					}
					printGrid5122(printerG, new string[] {
						$"{tab} {remarks[i].Name}",
						null,
						0 == remarks[i].Price ? null : remarks[i].Price.ToString(),
						0 == remarks[i].Price ? null : remarks[i].Price.ToString()
					}, protocol.PrinterFormat.ReciptFontSize);

					priceAll += remarks[i].Price;
				}
			};

			printHr(printerG);

			if(isFullDineMenus) {
				priceAll = protocol.Dine.Price;
			}
			printGrid55f(printerG, new string[] { "总计", priceAll.ToString() }, protocol.PrinterFormat.ReciptBigFontSize);

			string paidWay = protocol.Dine.IsOnline ? "线上支付" : "线下支付";
			printerG.DrawStringLine($"支付方式: {paidWay}", protocol.PrinterFormat.ReciptFontSize);
			foreach(DinePaidDetail dinePaidDetail in protocol.Dine.DinePaidDetails) {
				decimal dinePaidDetailPrice = dinePaidDetail.Price;
				if(dinePaidDetail.PayKind.Type == HotelDAO.Models.PayKindType.Cash) {
					dinePaidDetailPrice += protocol.Dine.Change;
				}
				printerG.DrawStringLine($"{dinePaidDetail.PayKind.Name}: ￥{dinePaidDetailPrice}", protocol.PrinterFormat.ReciptFontSize);
			}
			if(protocol.Dine.IsPaid) {
				printerG.DrawStringLine($"找零: ￥{protocol.Dine.Change}", protocol.PrinterFormat.ReciptFontSize);
			}
			else {
				printerG.DrawStringLine("未支付", protocol.PrinterFormat.ReciptFontSize);
			}

			printEnd(printerG);

			g.Dispose();

			return cutBmp(bmp, printerG.GetHeight());
		}
		/// <summary>
		/// 打印传菜单
		/// </summary>
		private Bitmap generateServeOrderBmp(DineForPrinting protocol) {
			Bitmap bmp = new Bitmap(protocol.PrinterFormat.PaperSize, maxHeight);
			Graphics g = Graphics.FromImage(bmp);

			PrinterGraphics printerG = new PrinterGraphics(g, protocol.PrinterFormat.PaperSize, maxHeight, protocol.PrinterFormat.Font);

			printGrid55(printerG, new string[] { $"单号: {protocol.Dine.Id}", $"时间: {protocol.Dine.BeginTime.ToString("M-d HH:mm")}" }, protocol.PrinterFormat.ServeOrderSmallFontSize);
			printGrid55(printerG, new string[] { $"顾客: {protocol.User?.Id}", $"服务员: {protocol.Dine.Waiter.Name}" }, protocol.PrinterFormat.ServeOrderSmallFontSize);
			printerG.DrawStringLine($"餐桌: {protocol.Dine.Desk.Name}", protocol.PrinterFormat.ServeOrderFontSize);

			printHr(printerG);

			printGrid82(printerG, new string[] { "名称", "数量" }, protocol.PrinterFormat.ServeOrderSmallFontSize);

			printHr(printerG);

			foreach(DineMenu dineMenu in protocol.Dine.DineMenus.Where(p => p.Status != HotelDAO.Models.DineMenuStatus.Returned)) {
				// 打印具体菜品信息
				printGrid82(printerG, new string[] { dineMenu.Menu.Name, dineMenu.Count.ToString() }, protocol.PrinterFormat.ServeOrderFontSize);

				// 如果菜品为套餐，则打印套餐包含的具体菜品信息
				if(dineMenu.Menu.IsSetMeal) {
					List<SetMealMenu> setMealMenus = dineMenu.Menu.SetMealMenus;
					for(int i = 0; i < setMealMenus.Count; i++) {
						char tab = '├';
						if(i == setMealMenus.Count - 1) {
							tab = '└';
						}
						printGrid82(printerG, new string[] { $"{tab} {setMealMenus[i].Name}", setMealMenus[i].Count.ToString() }, protocol.PrinterFormat.ServeOrderFontSize);
					}
				}

				// 打印菜品的备注信息
				var remarks = dineMenu.Remarks.ToList();
				for(int i = 0; i < dineMenu.Remarks.Count; i++) {
					char tab = '├';
					if(i == dineMenu.Remarks.Count - 1) {
						tab = '└';
					}
					printGrid82(printerG, new string[] { $"{tab} {remarks[i].Name}", null, }, protocol.PrinterFormat.ServeOrderFontSize);
				}
			};

			printEnd(printerG);

			return cutBmp(bmp, printerG.GetHeight());
		}
		/// <summary>
		/// 打印厨房单
		/// </summary>
		private Bitmap generateKitchenOrderBmp(DineForPrinting protocol, DineMenu dineMenu, SetMealMenu setMealMenu) {
			Bitmap bmp = new Bitmap(protocol.PrinterFormat.PaperSize, maxHeight);
			Graphics g = Graphics.FromImage(bmp);

			PrinterGraphics printerG = new PrinterGraphics(g, protocol.PrinterFormat.PaperSize, maxHeight, protocol.PrinterFormat.Font);

			printGrid55(printerG, new string[] { $"单号: {protocol.Dine.Id}", $"时间: {((DateTime)protocol.Dine.BeginTime).ToString("M-d HH:mm")}" }, protocol.PrinterFormat.KitchenOrderSmallFontSize);

			if(dineMenu.Status == HotelDAO.Models.DineMenuStatus.Returned) {
				string returnStr = "退菜";
				if(dineMenu.ReturnedReason != null)
					returnStr += $", 理由: {dineMenu.ReturnedReason}";
				printerG.DrawStringLine(returnStr, protocol.PrinterFormat.KitchenOrderFontSize);
			}

			printerG.DrawStringLine($"餐桌: {protocol.Dine.Desk.Name}", protocol.PrinterFormat.KitchenOrderFontSize);

			printGrid82(printerG, new string[] { dineMenu.Menu.Name, dineMenu.Count.ToString() }, protocol.PrinterFormat.KitchenOrderFontSize);

			if(setMealMenu != null) {
				printGrid82(printerG, new string[] { $"└ {setMealMenu.Name}", setMealMenu.Count.ToString() }, protocol.PrinterFormat.KitchenOrderFontSize);
			}

			// 打印菜品的备注信息
			var remarks = dineMenu.Remarks.ToList();
			for(int i = 0; i < dineMenu.Remarks.Count; i++) {
				char tab = '├';
				if(i == dineMenu.Remarks.Count - 1) {
					tab = '└';
				}
				printGrid82(printerG, new string[] { $"{tab} {remarks[i].Name}", null, }, protocol.PrinterFormat.KitchenOrderFontSize);
			}

			printEnd(printerG);

			return cutBmp(bmp, printerG.GetHeight());
		}
	}
}
