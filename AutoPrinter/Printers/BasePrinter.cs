using System;
using System.Net;
using System.Drawing;

namespace AutoPrinter {
	public abstract class BasePrinter {
		public BasePrinter(Action<IPEndPoint, Guid, Exception> errorDelegate) {
			this.errorDelegate = errorDelegate;
		}
		protected Action<IPEndPoint, Guid, Exception> errorDelegate;

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
}
