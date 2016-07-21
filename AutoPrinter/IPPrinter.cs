using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Timers;

namespace AutoPrinter {
	public class BmpInfo {
		public Bitmap bmp { get; set; }
		public byte[] printingBytes { get; set; }
	}
	public class IPInfo {
		public Queue<BmpInfo> Queue { get; set; }
		public ManualResetEvent Mutex { get; set; }
	}

	public class TcpClientInfo {
		public TcpClientInfo(TcpClient client) {
			Client = client;
			OriginalRemotePoint = (IPEndPoint)client.Client.RemoteEndPoint;
			ConnectedTime = DateTime.Now;
			Connected = true;
		}
		public TcpClientInfo(TcpClient client, IPEndPoint originalRemotePoint) {
			Client = client;
			OriginalRemotePoint = originalRemotePoint;
		}

		public TcpClient Client { get; set; }
		public IPEndPoint OriginalRemotePoint { get; set; }
		public DateTime ConnectedTime { get; set; }
		public int TimeOut { get; set; }
		public bool Connected { get; set; }

		public void Close() {
			Client.Close();
			Connected = false;
		}
	}

	public class IPPrinter {

		public List<TcpClientInfo> PrinterClients { get; set; } = new List<TcpClientInfo>();
		public Dictionary<IPAddress, IPInfo> IpBmpDataMap { get; set; } = new Dictionary<IPAddress, IPInfo>();

		// 初始化
		private readonly byte[] init = new byte[] { 0x1B, 0x40 };
		// 切纸
		private readonly byte[] cut = new byte[] { 0x1D, 0x56, 1, 49 };
		// 换行
		private readonly byte[] lf = new byte[] { 0x0A };

		private Action<IPAddress, BmpInfo, string> callBack;

		private IPPrinter(Action<IPAddress, BmpInfo, string> callBack) {
			this.callBack = callBack;

			System.Timers.Timer t = new System.Timers.Timer(10000);
			t.Elapsed += T_Elapsed;
			t.Start();
		}

		private static IPPrinter instance;
		private static readonly object locker = new object();
		public static IPPrinter GetInstance(Action<IPAddress, BmpInfo, string> callBack = null) {
			if(instance == null) {
				lock(locker) {
					if(instance == null) {
						instance = new IPPrinter(callBack);
					}
				}
			}
			return instance;
		}

		private void T_Elapsed(object sender, ElapsedEventArgs e) {
			lock(PrinterClients) {
				PrinterClients.Where(c => c.Connected).ToList().ForEach(c => {
					if(++c.TimeOut == 6) {
						callBack(c.OriginalRemotePoint.Address, null, "超时断开连接");
						c.Close();
					}
					else {
						callBack(c.OriginalRemotePoint.Address, null, $"无打印, 等待超时{c.TimeOut}");
					}
				});
			}
		}

		public async Task Print(IPAddress ip, Bitmap bmp, int colorDepth) {
			BmpInfo bmpInfo = new BmpInfo {
				bmp = bmp,
				printingBytes = getPrintingBytes(bmp, colorDepth)
			};

			lock(IpBmpDataMap) {
				if(!IpBmpDataMap.ContainsKey(ip)) {
					IpBmpDataMap.Add(ip, new IPInfo {
						Mutex = new ManualResetEvent(true),
						Queue = new Queue<BmpInfo>()
					});
				}
				IpBmpDataMap[ip].Queue.Enqueue(bmpInfo);
			}

			await Task.Run(() => {
				IpBmpDataMap[ip].Mutex.WaitOne();
				IpBmpDataMap[ip].Mutex.Reset();
			});

			await prePrint(ip);
		}

		private byte[] getPrintingBytes(Bitmap bmp, int colorDepth) {
			List<byte> printingBytes = new List<byte>();
			printingBytes.AddRange(init);
			printingBytes.AddRange(getBmpBytes(bmp, colorDepth));
			printingBytes.AddRange(cut);

			return printingBytes.ToArray();
		}

		private List<byte> getBmpBytes(Bitmap bmp, int colorDepth) {
			List<byte> bmpBytes = new List<byte>();

			bmpBytes.AddRange(new byte[] { 0x1B, 0x33, 0x00 });

			byte[] escBmp = new byte[] { 0x1B, 0x2A, 33, 0, 0 };
			escBmp[3] = (byte)(bmp.Width % 256);
			escBmp[4] = (byte)(bmp.Width / 256);

			//循环图片像素打印图片  
			//循环高  
			for(int i = 0; i < bmp.Height / 24 + 1; i++) {
				//设置模式为位图模式  
				bmpBytes.AddRange(escBmp);
				//循环宽  
				for(int j = 0; j < bmp.Width; j++) {

					byte[] imgData = new byte[3];

					for(int k = 0; k < 24; k++) {
						int yPos = i * 24 + k;
						if(yPos < bmp.Height) {
							Color pixelColor = bmp.GetPixel(j, yPos);
							if(pixelColor.R <= colorDepth) {
								imgData[k / 8] += (byte)(128 >> (k % 8));
							}
						}
					}
					//一次写入一个data，24个像素
					bmpBytes.AddRange(imgData);

				}
				bmpBytes.AddRange(lf);
			}

			return bmpBytes;
		}


		private async Task prePrint(IPAddress ip) {
			lock(IpBmpDataMap) {
				if(!IpBmpDataMap.ContainsKey(ip) || IpBmpDataMap[ip].Queue.Count == 0) {
					IpBmpDataMap[ip].Mutex.Set();
					return;
				}
			}
			TcpClientInfo client;
			lock(PrinterClients) {
				client = PrinterClients.FirstOrDefault(p => p.OriginalRemotePoint.Address.Equals(ip) && p.Connected);
			}
			if(client == null) {
				await connect(ip);
			}
			else {
				await print(client, IpBmpDataMap[ip].Queue.Peek());
			}
		}
		private async Task print(TcpClientInfo client, BmpInfo bmpInfo) {
			try {
				NetworkStream stream = client.Client.GetStream();
				if(!stream.CanWrite) {
					throw new Exception("不支持写入");
				}

				await stream.WriteAsync(bmpInfo.printingBytes, 0, bmpInfo.printingBytes.Length);
				callBack?.Invoke(client.OriginalRemotePoint.Address, bmpInfo, "打印成功");
				client.TimeOut = 0;
				lock(IpBmpDataMap) {
					IpBmpDataMap[client.OriginalRemotePoint.Address].Queue.Dequeue();
				}
				await prePrint(client.OriginalRemotePoint.Address);
			}
			catch(Exception e) {
				callBack?.Invoke(client.OriginalRemotePoint.Address, null, "写入数据发生错误");
				callBack?.Invoke(client.OriginalRemotePoint.Address, null, e.Message);
				lock(PrinterClients) {
					for(int i = PrinterClients.Count - 1; i >= 0; i--) {
						if(PrinterClients[i] == client) {
							PrinterClients[i].Close();
						}
					}
				}
				callBack?.Invoke(client.OriginalRemotePoint.Address, null, "重新连接");
				await Task.Delay(1000);
				await connect(client.OriginalRemotePoint.Address);
			}
		}

		private async Task connect(IPAddress ip) {
			TcpClient client = new TcpClient();


			TcpClientInfo info;
			lock(PrinterClients) {
				info = PrinterClients.FirstOrDefault(p => p.OriginalRemotePoint?.Address == ip);

				if(info == null) {
					info = new TcpClientInfo(client, new IPEndPoint(ip, 9100));
					PrinterClients.Add(info);
				}
				else {
					info.Client = client;
				}
			}

			try {
				callBack?.Invoke(ip, null, "正在连接");
				await client.ConnectAsync(ip, 9100);
				callBack?.Invoke(ip, null, "连接成功");

				info.Connected = true;
				info.TimeOut = 0;

				await prePrint(ip);
			}
			catch(Exception e) {
				info.Connected = false;
				info.TimeOut--;

				callBack?.Invoke(ip, null, $"连接发生错误, {e.Message}");
				callBack?.Invoke(ip, null, $"第{-info.TimeOut}次重新尝试连接");

				if(-info.TimeOut >= 10) {
					callBack?.Invoke(ip, null, $"连接次数已达上限, 连接失败");
					return;
				}

				await Task.Delay(1000);
				await connect(ip);
			}
		}
	}
}
