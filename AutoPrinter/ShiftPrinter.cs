using Protocol.PrintingProtocol;
using System.Drawing;
using System.Drawing.Printing;

namespace AutoPrinter {
	public class ShiftPrinter : BasePrinter {
		private ShiftForPrinting protocol;

		public void Print(ShiftForPrinting protocol) {
			this.protocol = protocol;

			PrinterGraphics.FontName = "宋体";
			PrinterGraphics.PaperWidth = 278;

			PrintDocument printer = new PrintDocument();
			printer.DefaultPageSettings.PaperSize = new PaperSize("Custom", PrinterGraphics.PaperWidth, 1000);
			PrintPageEventHandler printHandler = (sender, e) => {
				printShifts(e.Graphics);
			};

			printer.PrinterSettings.PrinterName = protocol.PrinterName;
			printer.PrintPage += printHandler;
			printer.Print();
			printer.PrintPage -= printHandler;
		}

		private void printShifts(Graphics g) {
			PrinterGraphics printer = new PrinterGraphics(g);
			decimal receivablePriceAll = 0, realPriceAll = 0;

			printer.DrawStringLine($"交接班", protocol.PrinterFormat.ShiftBigFontSize, align: StringAlignment.Center);
			foreach(Shift shift in protocol.Shifts) {
				printer.DrawStringLine($"班次: {shift.Id}", protocol.PrinterFormat.ShiftFontSize);
				printer.DrawStringLine($"时间: {shift.DateTime.ToString("yyyy-MM-dd HH:mm:ss")}", protocol.PrinterFormat.ShiftFontSize);

				printGrid433(printer, new string[] { "支付名称", "应收", "实收" }, protocol.PrinterFormat.ShiftSmallFontSize);
				foreach(ShiftDetail detail in shift.ShiftDetails) {
					receivablePriceAll += detail.ReceivablePrice;
					realPriceAll += detail.RealPrice;
					printGrid433(printer, new string[] { detail.PayKind, $"￥{detail.ReceivablePrice}", $"￥{detail.RealPrice}" }, protocol.PrinterFormat.ShiftFontSize);
				}
				printHr(printer);
			}

			printer.DrawStringLine("总计:", protocol.PrinterFormat.ShiftSmallFontSize);
			printGrid55f(printer, new string[] { "应收", $"￥{receivablePriceAll}" }, protocol.PrinterFormat.ShiftFontSize);
			printGrid55f(printer, new string[] { "实收", $"￥{realPriceAll}" }, protocol.PrinterFormat.ShiftFontSize);
			printGrid55f(printer, new string[] { "盈亏", $"￥{(realPriceAll - receivablePriceAll)}" }, protocol.PrinterFormat.ShiftFontSize);

			printEnd(printer);
		}
	}
}
