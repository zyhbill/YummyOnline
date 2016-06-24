using Protocal.PrintingProtocal;
using System.Drawing;
using System.Drawing.Printing;

namespace AutoPrinter {
	public class ShiftPrinter : BasePrinter {
		private ShiftForPrinting protocal;

		public void Print(ShiftForPrinting protocal) {
			this.protocal = protocal;

			PrinterGraphics.FontName = "宋体";
			PrinterGraphics.PaperWidth = 278;

			PrintDocument printer = new PrintDocument();
			printer.DefaultPageSettings.PaperSize = new PaperSize("Custom", PrinterGraphics.PaperWidth, 1000);
			PrintPageEventHandler printHandler = (sender, e) => {
				printShifts(e.Graphics);
			};

			printer.PrinterSettings.PrinterName = protocal.PrinterName;
			printer.PrintPage += printHandler;
			printer.Print();
			printer.PrintPage -= printHandler;
		}

		private void printShifts(Graphics g) {
			PrinterGraphics printer = new PrinterGraphics(g);
			decimal receivablePriceAll = 0, realPriceAll = 0;

			printer.DrawStringLine($"交接班", protocal.PrinterFormat.ShiftBigFontSize, align: StringAlignment.Center);
			foreach(Shift shift in protocal.Shifts) {
				printer.DrawStringLine($"班次: {shift.Id}", protocal.PrinterFormat.ShiftFontSize);
				printer.DrawStringLine($"时间: {shift.DateTime.ToString("yyyy-MM-dd HH:mm:ss")}", protocal.PrinterFormat.ShiftFontSize);

				printGrid433(printer, new string[] { "支付名称", "应收", "实收" }, protocal.PrinterFormat.ShiftSmallFontSize);
				foreach(ShiftDetail detail in shift.ShiftDetails) {
					receivablePriceAll += detail.ReceivablePrice;
					realPriceAll += detail.RealPrice;
					printGrid433(printer, new string[] { detail.PayKind, $"￥{detail.ReceivablePrice}", $"￥{detail.RealPrice}" }, protocal.PrinterFormat.ShiftFontSize);
				}
				printHr(printer);
			}

			printer.DrawStringLine("总计:", protocal.PrinterFormat.ShiftSmallFontSize);
			printGrid55f(printer, new string[] { "应收", $"￥{receivablePriceAll}" }, protocal.PrinterFormat.ShiftFontSize);
			printGrid55f(printer, new string[] { "实收", $"￥{realPriceAll}" }, protocal.PrinterFormat.ShiftFontSize);
			printGrid55f(printer, new string[] { "盈亏", $"￥{(realPriceAll - receivablePriceAll)}" }, protocal.PrinterFormat.ShiftFontSize);

			printEnd(printer);
		}
	}
}
