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
		/// <summary>
		/// 需要传输的最终字节流
		/// </summary>
		public byte[] printingBytes { get; set; }
	}

	public class TcpClientInfo {
		public TcpClientInfo(TcpClient client) {
			Client = client;
			ConnectedTime = DateTime.Now;
		}

		public TcpClient Client { get; set; }
		public DateTime ConnectedTime { get; set; }
		/// <summary>
		/// 打印机空闲时间
		/// </summary>
		public int IdleTime { get; set; }

		public void Close() {
			Client.Close();
		}
	}

	public class ClientBmpInfo {
		public Queue<BmpInfo> Queue { get; set; }
		/// <summary>
		/// 多个打印命令互斥信号量
		/// </summary>
		public AutoResetEvent PrintingMutex { get; set; }
		public AutoResetEvent ConnectionMutex { get; set; }
		public TcpClientInfo TcpClientInfo { get; set; }
		/// <summary>
		/// 尝试重新连接次数
		/// </summary>
		public int TryTime { get; set; }
		public bool IsConnecting { get; set; }
	}

	public class IPPrinter {
		public Dictionary<IPAddress, ClientBmpInfo> IPClientBmpMap { get; set; } = new Dictionary<IPAddress, ClientBmpInfo>();

		// 初始化
		private readonly byte[] init = new byte[] { 0x1B, 0x40 };
		// 切纸
		private readonly byte[] cut = new byte[] { 0x1D, 0x56, 1, 49 };
		// 换行
		private readonly byte[] lf = new byte[] { 0x0A };

		private Action<IPAddress, BmpInfo, string, bool> callBack;

		private IPPrinter(Action<IPAddress, BmpInfo, string, bool> callBack) {
			this.callBack = callBack;

			System.Timers.Timer t = new System.Timers.Timer(10000);
			t.Elapsed += T_Elapsed;
			t.Start();
		}

		private static IPPrinter instance;
		private static readonly object locker = new object();
		/// <summary>
		/// 获得单例对象
		/// </summary>
		public static IPPrinter GetInstance(Action<IPAddress, BmpInfo, string, bool> callBack = null) {
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
			lock(IPClientBmpMap) {
				foreach(IPAddress ip in IPClientBmpMap.Keys) {
					if(IPClientBmpMap[ip].TcpClientInfo == null) {
						continue;
					}

					if(++IPClientBmpMap[ip].TcpClientInfo.IdleTime == 6) {
						IPClientBmpMap[ip].TcpClientInfo.Close();
						IPClientBmpMap[ip].TcpClientInfo = null;
						callBack(ip, null, "超时断开连接", true);
					}
					else {
						callBack(ip, null, null, true);
					}
				}
			}
		}

		public async Task Print(IPAddress ip, Bitmap bmp, int colorDepth) {
			BmpInfo bmpInfo = new BmpInfo {
				bmp = bmp,
				printingBytes = getPrintingBytes(bmp, colorDepth)
			};

			lock(IPClientBmpMap) {
				if(!IPClientBmpMap.ContainsKey(ip)) {
					IPClientBmpMap.Add(ip, new ClientBmpInfo {
						PrintingMutex = new AutoResetEvent(true),
						ConnectionMutex = new AutoResetEvent(true),
						Queue = new Queue<BmpInfo>()
					});
				}
				IPClientBmpMap[ip].Queue.Enqueue(bmpInfo);
			}
			callBack?.Invoke(ip, bmpInfo, "已进入打印队列", true);

			if(IPClientBmpMap[ip].IsConnecting) {
				return;
			}
			await Task.Run(() => {
				IPClientBmpMap[ip].PrintingMutex.WaitOne();
				IPClientBmpMap[ip].PrintingMutex.Reset();
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
			TcpClientInfo client;

			lock(IPClientBmpMap) {
				if(!IPClientBmpMap.ContainsKey(ip) || IPClientBmpMap[ip].Queue.Count == 0) {
					IPClientBmpMap[ip].PrintingMutex.Set();
					return;
				}

				client = IPClientBmpMap[ip].TcpClientInfo;
			}

			if(client == null) {
				await Connect(ip);
			}
			else {
				await print(ip, IPClientBmpMap[ip].Queue.Peek());
			}
		}
		private async Task print(IPAddress ip, BmpInfo bmpInfo) {
			TcpClientInfo client;

			client = IPClientBmpMap[ip].TcpClientInfo;

			try {
				NetworkStream stream = client.Client.GetStream();
				if(!stream.CanWrite) {
					throw new Exception("不支持写入");
				}

				await stream.WriteAsync(bmpInfo.printingBytes, 0, bmpInfo.printingBytes.Length);

				client.IdleTime = 0;
				lock(IPClientBmpMap) {
					IPClientBmpMap[ip].Queue.Dequeue();
				}
				callBack?.Invoke(ip, bmpInfo, "打印成功", true);

				await prePrint(ip);
			}
			catch(Exception e) {
				callBack?.Invoke(ip, null, $"写入数据发生错误 {e.Message}", false);

				lock(IPClientBmpMap) {
					IPClientBmpMap[ip].TcpClientInfo?.Close();
					IPClientBmpMap[ip].TcpClientInfo = null;
				}

				callBack?.Invoke(ip, null, "重新尝试连接", true);
				await Connect(ip);
			}
		}

		public async Task Connect(IPAddress ip) {
			ClientBmpInfo clientBmpInfo;
			lock(IPClientBmpMap) {
				if(!IPClientBmpMap.ContainsKey(ip)) {
					IPClientBmpMap.Add(ip, new ClientBmpInfo {
						PrintingMutex = new AutoResetEvent(true),
						ConnectionMutex = new AutoResetEvent(true),
						Queue = new Queue<BmpInfo>()
					});
				}
				clientBmpInfo = IPClientBmpMap[ip];

				callBack?.Invoke(ip, null, null, true);

				if(clientBmpInfo.TcpClientInfo != null) {
					callBack?.Invoke(ip, null, "已连接", false);
					return;
				}
				if(clientBmpInfo.IsConnecting) {
					callBack?.Invoke(ip, null, "正在连接中", false);
					return;
				}
			}

			//await Task.Run(() => {
			//	clientBmpInfo.ConnectionMutex.WaitOne();
			//	clientBmpInfo.ConnectionMutex.Reset();
			//});

			await connect(ip, clientBmpInfo);
		}
		private async Task connect(IPAddress ip, ClientBmpInfo clientBmpInfo) {
			TcpClient client = new TcpClient();

			try {
				clientBmpInfo.IsConnecting = true;
				callBack?.Invoke(ip, null, "正在连接", false);
				await client.ConnectAsync(ip, 9100);

				clientBmpInfo.TcpClientInfo = new TcpClientInfo(client);
				clientBmpInfo.TryTime = 0;

				callBack?.Invoke(ip, null, "连接成功", true);
				clientBmpInfo.IsConnecting = false;

				await prePrint(ip);
			}
			catch(Exception e) {
				callBack?.Invoke(ip, null, $"连接发生错误, {e.Message}", false);
				callBack?.Invoke(ip, null, $"第{++clientBmpInfo.TryTime}次重新尝试连接", false);

				if(clientBmpInfo.TryTime >= 2) {
					callBack?.Invoke(ip, null, $"连接次数已达上限, 连接失败", false);
					clientBmpInfo.TryTime = 0;
					clientBmpInfo.IsConnecting = false;
					clientBmpInfo.PrintingMutex.Set();

					return;
				}

				await Task.Delay(2000);
				await connect(ip, clientBmpInfo);
			}
		}
	}
}
