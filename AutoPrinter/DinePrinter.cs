using Protocol;
using Protocol.PrintingProtocol;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;

namespace AutoPrinter {
	public abstract class BasePrinter {
		protected void printGrid55(PrinterGraphics printer, string[] texts, float fontSize) {
			printer.DrawGrid(new float[] { 0.5f, 0.5f, },
				texts,
				fontSize,
				new StringAlignment[] { StringAlignment.Near, StringAlignment.Near });
		}
		protected void printGrid55f(PrinterGraphics printer, string[] texts, float fontSize) {
			printer.DrawGrid(new float[] { 0.5f, 0.5f, },
				texts,
				fontSize,
				new StringAlignment[] { StringAlignment.Near, StringAlignment.Far });
		}
		protected void printGrid82(PrinterGraphics printer, string[] texts, float fontSize) {
			printer.DrawGrid(new float[] { 0.8f, 0.2f, },
				texts,
				fontSize,
				new StringAlignment[] { StringAlignment.Near, StringAlignment.Center });
		}
		protected void printGrid5122(PrinterGraphics printer, string[] texts, float fontSize) {
			printer.DrawGrid(new float[] { 0.5f, 0.1f, 0.2f, 0.2f },
				texts,
				fontSize,
				new StringAlignment[] { StringAlignment.Near, StringAlignment.Near, StringAlignment.Far, StringAlignment.Far });
		}
		protected void printGrid433(PrinterGraphics printer, string[] texts, float fontSize) {
			printer.DrawGrid(new float[] { 0.4f, 0.3f, 0.3f },
				texts,
				fontSize,
				new StringAlignment[] { StringAlignment.Near, StringAlignment.Far, StringAlignment.Far });
		}
		protected void printHr(PrinterGraphics printer) {
			printer.TrimY(-5);
			printer.DrawStringLineLoop("-", 8);
			printer.TrimY(-4);
		}
		protected void printEnd(PrinterGraphics printer) {
			printer.TrimY(10);
			printer.DrawStringLineLoop("*", 8);
			printer.TrimY(10);
		}
	}



	public class DinePrinter : BasePrinter {
		private DineForPrinting protocol;

		public static List<string> ListPrinters() {
			List<string> printers = new List<string>();
			foreach(string printer in PrinterSettings.InstalledPrinters) {
				printers.Add(printer);
			}
			return printers;
		}

		public void Print(DineForPrinting protocol, List<PrintType> printTypes) {
			this.protocol = protocol;
			PrinterGraphics.FontName = protocol.PrinterFormat.Font;
			PrinterGraphics.PaperWidth = protocol.PrinterFormat.PaperSize;

			PrintDocument printer = new PrintDocument();

			printer.DefaultPageSettings.PaperSize = new PaperSize("Custom", PrinterGraphics.PaperWidth, 1000);
			PrintPageEventHandler printHandler = null;

			foreach(PrintType type in printTypes) {
				switch(type) {
					case PrintType.Recipt:
						if(protocol.Dine.Desk.ReciptPrinter == null) {
							break;
						}
						printer.PrinterSettings.PrinterName = protocol.Dine.Desk.ReciptPrinter.Name;
						printHandler = (sender, e) => {
							printRecipt(e.Graphics);
						};
						printer.PrintPage += printHandler;
						printer.Print();
						printer.PrintPage -= printHandler;
						break;
					case PrintType.ServeOrder:
						if(protocol.Dine.Desk.ServePrinter == null) {
							break;
						}
						printer.PrinterSettings.PrinterName = protocol.Dine.Desk.ServePrinter.Name;
						printHandler = (sender, e) => {
							printServeOrder(e.Graphics);
						};
						printer.PrintPage += printHandler;
						printer.Print();
						printer.PrintPage -= printHandler;
						break;
					case PrintType.KitchenOrder:
						DineMenu currDineMenu = null;
						SetMealMenu currSetMealMenu = null;

						printHandler = (sender, e) => {
							printKitchenOrder(e.Graphics, currDineMenu, currSetMealMenu);
						};
						printer.PrintPage += printHandler;
						foreach(DineMenu dineMenu in protocol.Dine.DineMenus.Where(p => p.Status != HotelDAO.Models.DineMenuStatus.Returned)) {
							if(dineMenu.Menu.Printer == null) {
								continue;
							}
							printer.PrinterSettings.PrinterName = dineMenu.Menu.Printer.Name;
							currDineMenu = dineMenu;
							if(!dineMenu.Menu.IsSetMeal) {
								printer.Print();
							}
							else {
								foreach(SetMealMenu setMealMenu in dineMenu.Menu.SetMealMenus) {
									currSetMealMenu = setMealMenu;
									printer.Print();
									currSetMealMenu = null;
								}
							}
						}
						printer.PrintPage -= printHandler;
						break;
				}
			}
		}

		/// <summary>
		/// 打印收银条
		/// </summary>
		private void printRecipt(Graphics g) {
			PrinterGraphics printer = new PrinterGraphics(g);

			printer.DrawStringLine($"欢迎光临{protocol.Hotel.Name}", protocol.PrinterFormat.ReciptBigFontSize, align: StringAlignment.Center);
			printer.DrawStringLine($"TEL: {protocol.Hotel.Tel}", protocol.PrinterFormat.ReciptSmallFontSize, align: StringAlignment.Center);
			printer.DrawStringLine($"{protocol.Hotel.Address}", protocol.PrinterFormat.ReciptSmallFontSize, align: StringAlignment.Center);
			printer.DrawStringLine($"收据", protocol.PrinterFormat.ReciptSmallFontSize, align: StringAlignment.Center);
			printer.DrawSpacing(5);

			printGrid55(printer, new string[] { $"单号: {protocol.Dine.Id}", $"时间: {protocol.Dine.BeginTime.ToString("M-d HH:mm")}" }, protocol.PrinterFormat.ReciptFontSize);
			printGrid55(printer, new string[] { $"顾客: {protocol.User?.Id}", $"服务员: {protocol.Dine.Waiter.Name}" }, protocol.PrinterFormat.ReciptFontSize);
			printer.DrawStringLine($"餐桌: {protocol.Dine.Desk.Name}", protocol.PrinterFormat.ReciptFontSize);

			printHr(printer);

			printGrid5122(printer, new string[] { "名称", "数量", "单价", "折后小计" }, protocol.PrinterFormat.ReciptFontSize);

			printHr(printer);

			foreach(DineMenu dineMenu in protocol.Dine.DineMenus.Where(p => p.Status != HotelDAO.Models.DineMenuStatus.Returned)) {
				// 打印具体菜品信息
				printGrid5122(printer, new string[] {
					dineMenu.Menu.Name,
					dineMenu.Count.ToString(),
					dineMenu.OriPrice.ToString(),
					(dineMenu.Price * dineMenu.Count).ToString()
				}, protocol.PrinterFormat.ReciptFontSize);

				// 如果菜品为套餐，则打印套餐包含的具体菜品信息
				if(dineMenu.Menu.IsSetMeal) {
					List<SetMealMenu> setMealMenus = dineMenu.Menu.SetMealMenus;
					for(int i = 0; i < setMealMenus.Count; i++) {
						char tab = '├';
						if(i == setMealMenus.Count - 1) {
							tab = '└';
						}
						printGrid5122(printer, new string[] {
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
					printGrid5122(printer, new string[] {
						$"{tab} {remarks[i].Name}",
						null,
						0 == remarks[i].Price ? null : remarks[i].Price.ToString(),
						0 == remarks[i].Price ? null : remarks[i].Price.ToString()
					}, protocol.PrinterFormat.ReciptFontSize);
				}
			};

			printHr(printer);

			printGrid55f(printer, new string[] { "总计", protocol.Dine.Price.ToString() }, protocol.PrinterFormat.ReciptBigFontSize);

			string paidWay = protocol.Dine.IsOnline ? "线上支付" : "线下支付";
			printer.DrawStringLine($"支付方式: {paidWay}", protocol.PrinterFormat.ReciptFontSize);
			foreach(DinePaidDetail dinePaidDetail in protocol.Dine.DinePaidDetails) {
				printer.DrawStringLine($"{dinePaidDetail.PayKind.Name}: ￥{dinePaidDetail.Price}", protocol.PrinterFormat.ReciptFontSize);
			}
			if(protocol.Dine.IsPaid) {
				printer.DrawStringLine($"找零: ￥{protocol.Dine.Change}", protocol.PrinterFormat.ReciptFontSize);
			}
			else {
				printer.DrawStringLine("未支付", protocol.PrinterFormat.ReciptFontSize);
			}

			printEnd(printer);
		}
		/// <summary>
		/// 打印传菜单
		/// </summary>
		private void printServeOrder(Graphics g) {
			PrinterGraphics printer = new PrinterGraphics(g);

			printGrid55(printer, new string[] { $"单号: {protocol.Dine.Id}", $"时间: {protocol.Dine.BeginTime.ToString("M-d HH:mm")}" }, protocol.PrinterFormat.ServeOrderSmallFontSize);
			printGrid55(printer, new string[] { $"顾客: {protocol.User?.Id}", $"服务员: {protocol.Dine.Waiter.Name}" }, protocol.PrinterFormat.ServeOrderSmallFontSize);
			printer.DrawStringLine($"餐桌: {protocol.Dine.Desk.Name}", protocol.PrinterFormat.ServeOrderFontSize);

			printHr(printer);

			printGrid82(printer, new string[] { "名称", "数量" }, protocol.PrinterFormat.ServeOrderSmallFontSize);

			printHr(printer);

			foreach(DineMenu dineMenu in protocol.Dine.DineMenus.Where(p => p.Status != HotelDAO.Models.DineMenuStatus.Returned)) {
				// 打印具体菜品信息
				printGrid82(printer, new string[] { dineMenu.Menu.Name, dineMenu.Count.ToString() }, protocol.PrinterFormat.ServeOrderFontSize);

				// 如果菜品为套餐，则打印套餐包含的具体菜品信息
				if(dineMenu.Menu.IsSetMeal) {
					List<SetMealMenu> setMealMenus = dineMenu.Menu.SetMealMenus;
					for(int i = 0; i < setMealMenus.Count; i++) {
						char tab = '├';
						if(i == setMealMenus.Count - 1) {
							tab = '└';
						}
						printGrid82(printer, new string[] { $"{tab} {setMealMenus[i].Name}", setMealMenus[i].Count.ToString() }, protocol.PrinterFormat.ServeOrderFontSize);
					}
				}

				// 打印菜品的备注信息
				var remarks = dineMenu.Remarks.ToList();
				for(int i = 0; i < dineMenu.Remarks.Count; i++) {
					char tab = '├';
					if(i == dineMenu.Remarks.Count - 1) {
						tab = '└';
					}
					printGrid82(printer, new string[] { $"{tab} {remarks[i].Name}", null, }, protocol.PrinterFormat.ServeOrderFontSize);
				}
			};

			printEnd(printer);
		}

		/// <summary>
		/// 打印厨房单
		/// </summary>
		private void printKitchenOrder(Graphics g, DineMenu dineMenu, SetMealMenu setMealMenu) {
			PrinterGraphics printer = new PrinterGraphics(g);

			printGrid55(printer, new string[] { $"单号: {protocol.Dine.Id}", $"时间: {((DateTime)protocol.Dine.BeginTime).ToString("M-d HH:mm")}" }, protocol.PrinterFormat.KitchenOrderSmallFontSize);

			if(dineMenu.Status == HotelDAO.Models.DineMenuStatus.Returned) {
				string returnStr = "退菜";
				if(dineMenu.ReturnedReason != null)
					returnStr += $", 理由: {dineMenu.ReturnedReason}";
				printer.DrawStringLine(returnStr, protocol.PrinterFormat.KitchenOrderFontSize);
			}

			printer.DrawStringLine($"餐桌: {protocol.Dine.Desk.Name}", protocol.PrinterFormat.KitchenOrderFontSize);

			printGrid82(printer, new string[] { dineMenu.Menu.Name, dineMenu.Count.ToString() }, protocol.PrinterFormat.KitchenOrderFontSize);

			if(setMealMenu != null) {
				printGrid82(printer, new string[] { $"└ {setMealMenu.Name}", setMealMenu.Count.ToString() }, protocol.PrinterFormat.KitchenOrderFontSize);
			}

			// 打印菜品的备注信息
			var remarks = dineMenu.Remarks.ToList();
			for(int i = 0; i < dineMenu.Remarks.Count; i++) {
				char tab = '├';
				if(i == dineMenu.Remarks.Count - 1) {
					tab = '└';
				}
				printGrid82(printer, new string[] { $"{tab} {remarks[i].Name}", null, }, protocol.PrinterFormat.KitchenOrderFontSize);
			}

			printEnd(printer);
		}
	}
}
