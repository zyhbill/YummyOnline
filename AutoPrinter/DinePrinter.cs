using HotelDAO.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using Protocal;
using System;

namespace AutoPrinter {
	class DinePrinter {
		private DineForPrintingProtocal protocal;
		private List<PrintType> printTypes;

		private float fontSizeReciptHeader = 12,
			fontSizeRecipt = 8,
			fontSizeReciptSmall = 7,

			fontSizeKitchenOrder = 9,

			fontSizeServeOrder = 9;

		public DinePrinter(DineForPrintingProtocal protocal, List<PrintType> printTypes) {
			this.protocal = protocal;
			this.printTypes = printTypes;
		}

		public void Print() {
			PrintDocument printer = new PrintDocument();

			printer.DefaultPageSettings.PaperSize = new PaperSize("Custom", PrinterGraphics.PaperWidth, 1000);
			PrintPageEventHandler printHandler = null;

			foreach(PrintType type in printTypes) {
				switch(type) {
					case PrintType.Recipt:
						if(protocal.Dine.Desk.ReciptPrinter == null) {
							break;
						}
						printer.PrinterSettings.PrinterName = protocal.Dine.Desk.ReciptPrinter.Name;
						printHandler = (sender, e) => {
							printRecipt(e.Graphics);
						};
						printer.PrintPage += printHandler;
						printer.Print();
						printer.PrintPage -= printHandler;
						break;
					case PrintType.ServeOrder:
						if(protocal.Dine.Desk.ServePrinter == null) {
							break;
						}
						printer.PrinterSettings.PrinterName = protocal.Dine.Desk.ServePrinter.Name;
						printHandler = (sender, e) => {
							printServeOrder(e.Graphics);
						};
						printer.PrintPage += printHandler;
						printer.Print();
						printer.PrintPage -= printHandler;
						break;
					case PrintType.KitchenOrder:
						DineForPrintingProtocal.DineMenu currDineMenu = null;
						DineForPrintingProtocal.SetMealMenu currSetMealMenu = null;

						printHandler = (sender, e) => {
							printKitchenOrder(e.Graphics, currDineMenu, currSetMealMenu);
						};
						printer.PrintPage += printHandler;
						foreach(DineForPrintingProtocal.DineMenu dineMenu in protocal.Dine.DineMenus) {
							if(dineMenu.Menu.Printer == null) {
								continue;
							}
							printer.PrinterSettings.PrinterName = dineMenu.Menu.Printer.Name;
							currDineMenu = dineMenu;
							if(!dineMenu.Menu.IsSetMeal) {
								printer.Print();
							}
							else {
								foreach(DineForPrintingProtocal.SetMealMenu setMealMenu in protocal.SetMeals.First(p => p.MenuSetId == dineMenu.Menu.Id).Menus) {
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

			printer.DrawStringLine($"欢迎光临{protocal.Hotel.Name}", fontSizeReciptHeader, align: StringAlignment.Center);
			printer.DrawStringLine($"TEL: {protocal.Hotel.Tel}", fontSizeReciptSmall, align: StringAlignment.Center);
			printer.DrawStringLine($"{protocal.Hotel.Address}", fontSizeReciptSmall, align: StringAlignment.Center);
			printer.DrawStringLine($"收据", fontSizeReciptSmall, align: StringAlignment.Center);
			printer.DrawSpacing(5);

			printGrid55(printer, new string[] { $"单号: {protocal.Dine.Id}", $"时间: {protocal.Dine.BeginTime.ToString("M-d HH:mm")}" }, fontSizeRecipt);
			printGrid55(printer, new string[] { $"顾客: {protocal.User.Id}", $"服务员: {protocal.Dine.Waiter.Name}" }, fontSizeRecipt);
			printer.DrawStringLine($"餐桌: {protocal.Dine.Desk.Name}", fontSizeRecipt);

			printHr(printer);

			printGrid5122(printer, new string[] { "名称", "数量", "原价", "折后价格" }, fontSizeRecipt);

			printHr(printer);

			foreach(DineForPrintingProtocal.DineMenu dineMenu in protocal.Dine.DineMenus) {
				// 打印具体菜品信息
				printGrid5122(printer, new string[] { dineMenu.Menu.Name, dineMenu.Count.ToString(), dineMenu.OriPrice.ToString(), dineMenu.Price.ToString() }, fontSizeRecipt);

				// 如果菜品为套餐，则打印套餐包含的具体菜品信息
				if(dineMenu.Menu.IsSetMeal) {
					List<DineForPrintingProtocal.SetMealMenu> setMealMenus = protocal.SetMeals.First(p => p.MenuSetId == dineMenu.Menu.Id).Menus;
					for(int i = 0; i < setMealMenus.Count; i++) {
						char tab = '├';
						if(i == setMealMenus.Count - 1) {
							tab = '└';
						}
						printGrid5122(printer, new string[] {
							$"{tab} {setMealMenus[i].Name}",
							setMealMenus[i].Count.ToString(),
							null, null
						}, fontSizeRecipt);
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
					}, fontSizeRecipt);
				}
			};

			printHr(printer);

			printGrid5122(printer, new string[] { "总计", null, null, protocal.Dine.Price.ToString() }, fontSizeReciptHeader);

			string paidWay = protocal.Dine.IsOnline ? "线上支付" : "线下支付";
			printer.DrawStringLine($"支付方式: {paidWay}", fontSizeRecipt);
			foreach(DineForPrintingProtocal.DinePaidDetail dinePaidDetail in protocal.Dine.DinePaidDetails) {
				printer.DrawStringLine($"{dinePaidDetail.PayKind.Name}: ￥{dinePaidDetail.Price}", fontSizeRecipt);
			}
		}
		/// <summary>
		/// 打印传菜单
		/// </summary>
		private void printServeOrder(Graphics g) {
			PrinterGraphics printer = new PrinterGraphics(g);

			printGrid55(printer, new string[] { $"单号: {protocal.Dine.Id}", $"时间: {protocal.Dine.BeginTime.ToString("M-d HH:mm")}" }, fontSizeServeOrder);
			printGrid55(printer, new string[] { $"顾客: {protocal.User.Id}", $"服务员: {protocal.Dine.Waiter.Name}" }, fontSizeServeOrder);
			printer.DrawStringLine($"餐桌: {protocal.Dine.Desk.Name}", fontSizeServeOrder);

			printHr(printer);

			printGrid82(printer, new string[] { "名称", "数量" }, fontSizeServeOrder);

			printHr(printer);

			foreach(DineForPrintingProtocal.DineMenu dineMenu in protocal.Dine.DineMenus) {
				// 打印具体菜品信息
				printGrid82(printer, new string[] { dineMenu.Menu.Name, dineMenu.Count.ToString() }, fontSizeServeOrder);

				// 如果菜品为套餐，则打印套餐包含的具体菜品信息
				if(dineMenu.Menu.IsSetMeal) {
					List<DineForPrintingProtocal.SetMealMenu> setMealMenus = protocal.SetMeals.First(p => p.MenuSetId == dineMenu.Menu.Id).Menus;
					for(int i = 0; i < setMealMenus.Count; i++) {
						char tab = '├';
						if(i == setMealMenus.Count - 1) {
							tab = '└';
						}
						printGrid82(printer, new string[] { $"{tab} {setMealMenus[i].Name}", setMealMenus[i].Count.ToString() }, fontSizeServeOrder);
					}
				}

				// 打印菜品的备注信息
				var remarks = dineMenu.Remarks.ToList();
				for(int i = 0; i < dineMenu.Remarks.Count; i++) {
					char tab = '├';
					if(i == dineMenu.Remarks.Count - 1) {
						tab = '└';
					}
					printGrid82(printer, new string[] { $"{tab} {remarks[i].Name}", null, }, fontSizeServeOrder);
				}
			};
		}

		/// <summary>
		/// 打印厨房单
		/// </summary>
		private void printKitchenOrder(Graphics g, DineForPrintingProtocal.DineMenu dineMenu, DineForPrintingProtocal.SetMealMenu setMealMenu) {
			PrinterGraphics printer = new PrinterGraphics(g);
			printGrid55(printer, new string[] { $"单号: {protocal.Dine.Id}", $"时间: {((DateTime)protocal.Dine.BeginTime).ToString("M-d HH:mm")}" }, fontSizeKitchenOrder);
			printer.DrawStringLine($"餐桌: {protocal.Dine.Desk.Name}", fontSizeKitchenOrder);

			printGrid82(printer, new string[] { dineMenu.Menu.Name, dineMenu.Count.ToString() }, fontSizeKitchenOrder);

			if(setMealMenu != null) {
				printGrid82(printer, new string[] { $"└ {setMealMenu.Name}", setMealMenu.Count.ToString() }, fontSizeServeOrder);
			}

			// 打印菜品的备注信息
			var remarks = dineMenu.Remarks.ToList();
			for(int i = 0; i < dineMenu.Remarks.Count; i++) {
				char tab = '├';
				if(i == dineMenu.Remarks.Count - 1) {
					tab = '└';
				}
				printGrid82(printer, new string[] { $"  {tab} {remarks[i].Name}", null, }, fontSizeServeOrder);
			}
		}


		private void printGrid55(PrinterGraphics printer, string[] texts, float fontSize) {
			printer.DrawGrid(new float[] { 0.5f, 0.5f, },
				texts,
				fontSize,
				new StringAlignment[] { StringAlignment.Near, StringAlignment.Near });
		}
		private void printGrid82(PrinterGraphics printer, string[] texts, float fontSize) {
			printer.DrawGrid(new float[] { 0.8f, 0.2f, },
				texts,
				fontSize,
				new StringAlignment[] { StringAlignment.Near, StringAlignment.Center });
		}
		private void printGrid5122(PrinterGraphics printer, string[] texts, float fontSize) {
			printer.DrawGrid(new float[] { 0.5f, 0.1f, 0.2f, 0.2f },
				texts,
				fontSize,
				new StringAlignment[] { StringAlignment.Near, StringAlignment.Near, StringAlignment.Far, StringAlignment.Far });
		}
		private void printHr(PrinterGraphics printer) {
			printer.TrimY(-5);
			printer.DrawStringLineLoop("-", fontSizeRecipt);
			printer.TrimY(-4);
		}
	}
}
