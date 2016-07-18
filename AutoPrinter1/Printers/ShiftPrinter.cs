using Protocol.PrintingProtocol;
using System;
using System.Drawing;
using System.Net;

namespace AutoPrinter {
	public class ShiftPrinter : BasePrinter {
		private int maxHeight = 2000;

		public ShiftPrinter(Action<IPEndPoint, Exception> errorDelegate) : base(errorDelegate) { }

		public void Print(ShiftForPrinting protocol) {
			IPAddress ip = IPAddress.Parse(protocol.PrinterIpAddress);
			IPPrinter printer = new IPPrinter(new IPEndPoint(ip, 9100), errorDelegate);

			Bitmap bmp = generateShiftsBmp(protocol);
			printer.Print(bmp);
		}

		private Bitmap generateShiftsBmp(ShiftForPrinting protocol) {
			Bitmap bmp = new Bitmap(protocol.PrinterFormat.PaperSize, maxHeight);
			Graphics g = Graphics.FromImage(bmp);

			PrinterGraphics printerG = new PrinterGraphics(g, protocol.PrinterFormat.PaperSize, maxHeight, protocol.PrinterFormat.Font);

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

			return cutBmp(bmp, printerG.GetHeight());
		}
	}
}
