using Protocol.PrintingProtocol;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPrinter {
	public class BaseShiftPrinter : BasePrinter {
		protected Bitmap generateShiftBmp(ShiftForPrinting protocol) {
			Bitmap bmp = new Bitmap(protocol.PrinterFormat.PaperSize, maxHeight);
			Graphics g = Graphics.FromImage(bmp);
			int realHeight = drawShift(g, protocol);
			return cutBmp(bmp, realHeight);
		}

		protected int drawShift(Graphics g, ShiftForPrinting protocol) {
			PrinterGraphics printerG = new PrinterGraphics(g, protocol.PrinterFormat.PaperSize, protocol.PrinterFormat.Font, protocol.PrinterFormat.PaddingRight);

			printerG.DrawStringLine($"交接班", protocol.PrinterFormat.ShiftBigFontSize, align: StringAlignment.Center);
			
			decimal receivablePriceAll = 0, realPriceAll = 0;

			foreach(Shift shift in protocol.Shifts) {
				printerG.DrawStringLine($"班次: {shift.Id}", protocol.PrinterFormat.ShiftFontSize);
				printerG.DrawStringLine($"时间: {shift.DateTime.ToString("yyyy-MM-dd HH:mm:ss")}", protocol.PrinterFormat.ShiftFontSize);

				printGrid55f(printerG, new string[] { "累计消费", $"￥{shift.OriPrice}" }, protocol.PrinterFormat.ShiftFontSize);
				printGrid55f(printerG, new string[] { "累计堂吃", $"￥{shift.ToStayPrice}" }, protocol.PrinterFormat.ShiftFontSize);
				printGrid55f(printerG, new string[] { "累计外卖", $"￥{shift.ToGoPrice}" }, protocol.PrinterFormat.ShiftFontSize);
				printGrid55f(printerG, new string[] { "累计应付", $"￥{shift.Price}" }, protocol.PrinterFormat.ShiftFontSize);
				printGrid55f(printerG, new string[] { "累计优惠", $"￥{shift.PreferencePrice}" }, protocol.PrinterFormat.ShiftFontSize);
				printGrid55f(printerG, new string[] { "累计赠菜", $"￥{shift.GiftPrice}" }, protocol.PrinterFormat.ShiftFontSize);
				printGrid55f(printerG, new string[] { "累计退菜", $"￥{shift.ReturnedPrice}" }, protocol.PrinterFormat.ShiftFontSize);
				printGrid55f(printerG, new string[] { "累计桌数", shift.DeskCount.ToString() }, protocol.PrinterFormat.ShiftFontSize);
				printGrid55f(printerG, new string[] { "累计人数", shift.CustomerCount.ToString() }, protocol.PrinterFormat.ShiftFontSize);
				printGrid55f(printerG, new string[] { "人均消费", $"￥{shift.AveragePrice}" }, protocol.PrinterFormat.ShiftFontSize);

				printHr(printerG);

				printerG.DrawStringLine("付款明细:", protocol.PrinterFormat.ShiftFontSize);
				printGrid433(printerG, new string[] { "支付名称", "应收", "实收" }, protocol.PrinterFormat.ShiftSmallFontSize);
				foreach(PayKindShiftDetail detail in protocol.PayKindShifts.FirstOrDefault(p => p.Id == shift.Id).PayKindShiftDetails) {
					receivablePriceAll += detail.ReceivablePrice;
					realPriceAll += detail.RealPrice;
					printGrid433(printerG, new string[] { detail.PayKind, $"￥{detail.ReceivablePrice}", $"￥{detail.RealPrice}" }, protocol.PrinterFormat.ShiftFontSize);
				}

				printHr(printerG);

				printerG.DrawStringLine("菜品类统计:", protocol.PrinterFormat.ShiftFontSize);
				foreach(MenuClassShiftDetail detail in protocol.MenuClassShifts.FirstOrDefault(p => p.Id == shift.Id).MenuClassShiftDetails) {
					printGrid55f(printerG, new string[] { detail.MenuClass, $"￥{detail.Price}" }, protocol.PrinterFormat.ShiftFontSize);
				}

				printerG.TrimY(10);
				printHr(printerG);
				printHr(printerG);
				printerG.TrimY(10);
			}

			printerG.DrawStringLine("总计:", protocol.PrinterFormat.ShiftSmallFontSize);
			printGrid55f(printerG, new string[] { "应收", $"￥{receivablePriceAll}" }, protocol.PrinterFormat.ShiftFontSize);
			printGrid55f(printerG, new string[] { "实收", $"￥{realPriceAll}" }, protocol.PrinterFormat.ShiftFontSize);
			printGrid55f(printerG, new string[] { "盈亏", $"￥{(realPriceAll - receivablePriceAll)}" }, protocol.PrinterFormat.ShiftFontSize);

			printEnd(printerG);

			g.Dispose();

			return printerG.GetHeight();
		}
	}
}
