using System;
using System.Drawing;
using System.Text;

namespace AutoPrinter {
	public class PrinterGraphics {
		private float currX = 0;
		private float currY = 0;
		private Graphics g;

		private int paperWidth { get; set; }
		private string fontName { get; set; }
		private int paddingRight { get; set; }
		private float spacing { get; set; } = 0;

		public PrinterGraphics(Graphics g, int paperWidth, string fontName, int paddingRight) {
			this.g = g;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			this.paperWidth = paperWidth;
			this.fontName = fontName;
			this.paddingRight = paddingRight;
		}
		public int GetHeight() {
			return Convert.ToInt32(currY);
		}
		public void TrimX(float x) {
			currX += x;
		}
		public void TrimY(float y) {
			currY += y;
		}

		public void DrawStringLine(string text, float fontSize, Brush brush, bool wrapper = true, StringAlignment align = StringAlignment.Near, FontStyle style = FontStyle.Regular) {
			RectangleF areaRec = new RectangleF(currX, currY, paperWidth - paddingRight, 100);
			Font font = new Font(fontName, fontSize, style);
			StringFormat format = new StringFormat {
				Alignment = align,
				FormatFlags = wrapper ? 0 : StringFormatFlags.NoWrap
			};

			g.DrawString(text, font, brush, areaRec, format);

			currX = 0;
			// 如果换行就统计一行的font高度
			if(wrapper) {
				currY += spacing + g.MeasureString(text, font, areaRec.Size).Height;
			}
			else {
				currY += spacing + g.MeasureString(text, font).Height;
			}
		}
		public void DrawStringLine(string text, float fontSize, bool wrapper = true, StringAlignment align = StringAlignment.Near, FontStyle style = FontStyle.Regular) {
			DrawStringLine(text, fontSize, Brushes.Black, wrapper, align, style);
		}

		public void DrawStringLineLoop(string text, float fontSize, int loop) {
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < loop; i++) {
				sb.Append(text);
			}
			DrawStringLine(sb.ToString(), fontSize, false);
		}
		public void DrawStringLineLoop(string text, float fontSize) {
			Font font = new Font(fontName, fontSize);
			DrawStringLineLoop(text, fontSize, paperWidth);
		}

		public void DrawGrid(float[] ratios, string[] texts, float fontSize, Brush brush, StringAlignment[] aligns) {
			Font font = new Font(fontName, fontSize);
			float maxHeight = 0;
			for(int i = 0; i < ratios.Length; i++) {
				float width = (paperWidth - paddingRight) * ratios[i];
				RectangleF areaRec = new RectangleF(currX, currY, width, 100);
				StringFormat format = new StringFormat { Alignment = aligns[i] };
				g.DrawString(texts[i], font, brush, areaRec, format);
				currX += width;
				float height = g.MeasureString(texts[i], font, areaRec.Size).Height;
				if(height > maxHeight) {
					maxHeight = height;
				}
			}
			currX = 0;
			currY += spacing + maxHeight;
		}
		public void DrawGrid(float[] ratios, string[] texts, float fontSize, StringAlignment[] aligns) {
			DrawGrid(ratios, texts, fontSize, Brushes.Black, aligns);
		}
	}
}
