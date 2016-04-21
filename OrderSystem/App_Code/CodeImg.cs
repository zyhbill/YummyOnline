using System;
using System.Drawing;

namespace OrderSystem.Utility {
	public static class CodeImg {
		public static bool IsEmail(string signInString) {
			return signInString.Contains("@");
		}

		public static string CreateRandomCode() {
			string allChar = "0,1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,W,X,Y,Z,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z";
			string[] allCharArray = allChar.Split(',');
			string randomCode = "";
			Random rand = new Random();
			for(int i = 0; i < 4; i++) {
				int t = rand.Next(61);
				randomCode += allCharArray[t];
			}
			return randomCode;
		}

		public static byte[] CreateCheckCodeImage(string checkCode) {
			Bitmap image = new Bitmap((int)Math.Ceiling((checkCode.Length * 32.5)), 30);
			Graphics g = Graphics.FromImage(image);

			try {
				Random random = new Random();
				//清空图片背景色 
				g.Clear(Color.FromArgb(250, 250, 250));

				//画图片的背景噪音线 
				for(int i = 0; i < 20; i++) {
					int x1 = random.Next(image.Width);
					int x2 = random.Next(image.Width);
					int y1 = random.Next(image.Height);
					int y2 = random.Next(image.Height);

					g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
				}

				for(int k = 0; k <= checkCode.Length - 1; k++) {
					int cindex = random.Next(7);
					int findex = random.Next(5);

					Font drawFont = new Font("Arial", 13, (System.Drawing.FontStyle.Bold));

					SolidBrush drawBrush = new SolidBrush(Color.Black);

					float x = 5.0F;
					float y = 0.0F;
					float width = 20.0F;
					float height = 25.0F;
					int sjx = random.Next(10);
					int sjy = random.Next(image.Height - (int)height);

					RectangleF drawRect = new RectangleF(x + sjx + (k * 25), y + sjy, width, height);

					StringFormat drawFormat = new StringFormat();
					drawFormat.Alignment = StringAlignment.Center;

					g.DrawString(checkCode[k].ToString(), drawFont, drawBrush, drawRect, drawFormat);
				}

				//画图片的前景噪音点 
				for(int i = 0; i < 100; i++) {
					int x = random.Next(image.Width);
					int y = random.Next(image.Height);

					image.SetPixel(x, y, Color.FromArgb(random.Next()));
				}

				System.IO.MemoryStream ms = new System.IO.MemoryStream();
				image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
				return ms.ToArray();
			}
			finally {
				g.Dispose();
				image.Dispose();
			}
		}
	}
}
