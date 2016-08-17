using Protocol.PrintingProtocol;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;

namespace AutoPrinter {
	public abstract class BasePrinter {
		public static List<string> GetPritners() {
			List<string> printers = new List<string>();
			foreach(string printer in PrinterSettings.InstalledPrinters) {
				printers.Add(printer);
			}
			return printers;
		}

		/// <summary>
		/// 处理直接网络打印字体大小
		/// </summary>
		protected void handleIPPrinterFormat(PrinterFormat format) {
			format.PaperSize *= 2;

			format.ReciptBigFontSize = format.ReciptBigFontSize * 2 + 1;
			format.ReciptFontSize = format.ReciptFontSize * 2 + 1;
			format.ReciptSmallFontSize = format.ReciptSmallFontSize * 2 + 1;

			format.KitchenOrderFontSize = format.KitchenOrderFontSize * 2 + 1;
			format.KitchenOrderSmallFontSize = format.KitchenOrderSmallFontSize * 2 + 1;

			format.ServeOrderFontSize = format.ServeOrderFontSize * 2 + 1;
			format.ServeOrderSmallFontSize = format.ServeOrderSmallFontSize * 2 + 1;

			format.ShiftBigFontSize = format.ShiftBigFontSize * 2 + 1;
			format.ShiftFontSize = format.ShiftFontSize * 2 + 1;
			format.ShiftSmallFontSize = format.ShiftSmallFontSize * 2 + 1;
		}
		protected int maxHeight = 2000;
		/// <summary>
		/// 裁剪bmp至高度
		/// </summary>
		/// <param name="oriBmp">源BMP</param>
		/// <param name="height">需要裁剪的高度</param>
		protected Bitmap cutBmp(Bitmap oriBmp, int height) {
			Bitmap bmpOut = new Bitmap(oriBmp.Width, height);
			Graphics g = Graphics.FromImage(bmpOut);
			g.DrawImage(oriBmp, new Rectangle(0, 0, oriBmp.Width, height), new Rectangle(0, 0, oriBmp.Width, height), GraphicsUnit.Pixel);
			g.Dispose();
			return bmpOut;
		}

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
		protected void printGridRecipt(PrinterGraphics printer, string[] texts, float fontSize) {
			printer.DrawGrid(new float[] { 0.38f, 0.16f, 0.2f, 0.26f },
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
			printer.TrimY(100);
		}
	}
}
