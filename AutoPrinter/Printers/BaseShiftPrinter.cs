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

			decimal receivablePriceAll = 0, realPriceAll = 0;

			printerG.DrawStringLine($"交接班", protocol.PrinterFormat.ShiftBigFontSize, align: StringAlignment.Center);
			foreach(Shift shift in protocol.Shifts) {
				printerG.DrawStringLine($"班次: {shift.Id}", protocol.PrinterFormat.ShiftFontSize);
				printerG.DrawStringLine($"时间: {shift.DateTime.ToString("yyyy-MM-dd HH:mm:ss")}", protocol.PrinterFormat.ShiftFontSize);

				printGrid433(printerG, new string[] { "支付名称", "应收", "实收" }, protocol.PrinterFormat.ShiftSmallFontSize);
				foreach(ShiftDetail detail in shift.ShiftDetails) {
					receivablePriceAll += detail.ReceivablePrice;
					realPriceAll += detail.RealPrice;
					printGrid433(printerG, new string[] { detail.PayKind, $"￥{detail.ReceivablePrice}", $"￥{detail.RealPrice}" }, protocol.PrinterFormat.ShiftFontSize);
				}
				printHr(printerG);
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
