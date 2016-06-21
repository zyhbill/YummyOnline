using System.Drawing;
using System.Text;

namespace AutoPrinter {
	public class PrinterGraphics {
		private float currX = 0;
		private float currY = 0;
		private Graphics g;

		public static int PaperWidth { get; set; }
		public static string FontName { get; set; }
		public static float Spacing { get; set; } = 0;

		public PrinterGraphics(Graphics graphics) {
			g = graphics;
		}
		public void TrimX(float x) {
			currX += x;
		}
		public void TrimY(float y) {
			currY += y;
		}

		public void DrawStringLine(string text, float fontSize, Brush brush, bool wrapper = true, StringAlignment align = StringAlignment.Near) {
			RectangleF areaRec = new RectangleF(currX, currY, PaperWidth + fontSize, 100);
			Font font = new Font(FontName, fontSize);
			StringFormat format = new StringFormat {
				Alignment = align,
				FormatFlags = wrapper ? 0 : StringFormatFlags.NoWrap
			};

			g.DrawString(text, font, brush, areaRec, format);

			currX = 0;
			// 如果换行就统计一行的font高度
			if(wrapper) {
				currY += Spacing + g.MeasureString(text, font, areaRec.Size).Height;
			}
			else {
				currY += Spacing + g.MeasureString(text, font).Height;
			}
		}
		public void DrawStringLine(string text, float fontSize, bool wrapper = true, StringAlignment align = StringAlignment.Near) {
			DrawStringLine(text, fontSize, Brushes.Black, wrapper, align);
		}

		public void DrawStringLineLoop(string text, float fontSize, int loop) {
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < loop; i++) {
				sb.Append(text);
			}
			DrawStringLine(sb.ToString(), fontSize, false);
		}
		public void DrawStringLineLoop(string text, float fontSize) {
			Font font = new Font(FontName, fontSize);
			DrawStringLineLoop(text, fontSize, 100);
		}

		public void DrawSpacing(float height) {
			currY += height;
		}

		public void DrawGrid(float[] ratios, string[] texts, float fontSize, Brush brush, StringAlignment[] aligns) {
			Font font = new Font(FontName, fontSize);
			float maxHeight = 0;
			for(int i = 0; i < ratios.Length; i++) {
				float width = PaperWidth * ratios[i];
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
			currY += Spacing + maxHeight;
		}
		public void DrawGrid(float[] ratios, string[] texts, float fontSize, StringAlignment[] aligns) {
			DrawGrid(ratios, texts, fontSize, Brushes.Black, aligns);
		}
	}
}
