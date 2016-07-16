using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AutoPrinter {
	public class IPPrinter {
		// 初始化
		private readonly byte[] init = new byte[] { 0x1B, 0x40 };
		// 切纸
		private readonly byte[] cut = new byte[] { 0x1D, 0x56, 1, 49 };
		// 换行
		private readonly byte[] lf = new byte[] { 0x0A };

		private Action<IPEndPoint, Exception> errorDelegate;
		private byte colorDeep;

		private IPEndPoint ipEndPoint;

		public IPPrinter(IPEndPoint ipEndPoint, Action<IPEndPoint, Exception> errorDelegate, byte colorDeep = 125) {
			this.ipEndPoint = ipEndPoint;
			this.errorDelegate = errorDelegate;
			this.colorDeep = colorDeep;
		}

		public void Print(Bitmap bmp) {
			Task.Run(async () => {
				TcpClient client = new TcpClient();
				NetworkStream stream = null;

				try {
					client.Connect(ipEndPoint);
					stream = client.GetStream();
					if(!stream.CanWrite) {
						throw new Exception("不支持写入");
					}

					List<byte> data = new List<byte>();

					data.AddRange(init);
					printImg(data, bmp);
					data.AddRange(cut);

					stream.Write(data.ToArray(), 0, data.Count);
				}
				catch(Exception e) {
					errorDelegate(ipEndPoint, e);
					await Task.Delay(1000);
					Print(bmp);
				}
				finally {
					stream?.Close();
					stream?.Dispose();
					client.Close();
				}
			});
		}

		private void printImg(List<byte> sendData, Bitmap bmp) {
			byte[] escBmp = new byte[] { 0x1B, 0x2A, 33, 44, 2 };

			//循环图片像素打印图片  
			//循环高  
			for(int i = 0; i < bmp.Height / 24 + 1; i++) {
				//设置模式为位图模式  
				sendData.AddRange(escBmp);
				//循环宽  
				for(int j = 0; j < bmp.Width; j++) {

					byte[] imgData = new byte[3];

					for(int k = 0; k < 24; k++) {
						int yPos = i * 24 + k;
						if(yPos < bmp.Height) {
							Color pixelColor = bmp.GetPixel(j, yPos);
							if(pixelColor.A > 125) {
								imgData[k / 8] += (byte)(128 >> (k % 8));
							}
						}
					}
					//一次写入一个data，24个像素
					sendData.AddRange(imgData);
				}
			}
		}
	}
}
