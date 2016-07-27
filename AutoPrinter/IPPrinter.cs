using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Timers;
using System.Diagnostics;

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
		/// 打印机客户端
		/// </summary>
		public TcpClientInfo TcpClientInfo { get; set; }
		/// <summary>
		/// 打印机状态客户端
		/// </summary>
		public TcpClientInfo StatusTcpClientInfo { get; set; }
		/// <summary>
		/// 尝试重新连接次数
		/// </summary>
		public int TryTime { get; set; }
		public bool IsConnecting { get; set; }
		public bool IsPrinting { get; set; }
		/// <summary>
		/// 打印互斥
		/// </summary>
		public AutoResetEvent Mutex { get; set; }
	}

	public class IPPrinter {
		public Dictionary<IPAddress, ClientBmpInfo> IPClientBmpMap { get; set; } = new Dictionary<IPAddress, ClientBmpInfo>();

		// 初始化
		private readonly byte[] init = new byte[] { 0x1B, 0x40 };
		// 切纸
		private readonly byte[] cut = new byte[] { 0x1D, 0x56, 1, 49 };
		// 换行
		private readonly byte[] lf = new byte[] { 0x0A };
		// 查询打印机状态
		private readonly byte[] status = new byte[] { 27, 118 };
		// 打印机正常状态
		private readonly byte[] statusOKBuffer = { 20, 0, 0, 0 };

		/// <summary>
		/// 最大重新尝试连接次数
		/// </summary>
		private int maxTryTime = 5;
		/// <summary>
		/// 尝试间隔
		/// </summary>
		private int tryInterval = 2;
		/// <summary>
		/// 最大空闲占用时间
		/// </summary>
		private int maxIdleTime = 60;
		/// <summary>
		/// 日志回调函数
		/// </summary>
		public event Action<IPAddress, Bitmap, string, LogLevel> OnLog;

		private IPPrinter() {
			System.Timers.Timer t = new System.Timers.Timer(1000);
			t.Elapsed += T_Elapsed;
			t.Start();
		}

		private static IPPrinter instance;
		private static readonly object locker = new object();
		/// <summary>
		/// 获得单例对象
		/// </summary>
		public static IPPrinter GetInstance() {
			if(instance == null) {
				lock(locker) {
					if(instance == null) {
						instance = new IPPrinter();
					}
				}
			}
			return instance;
		}

		/// <summary>
		/// 每隔一段时间检测打印机是否连接空闲超时
		/// </summary>
		private void T_Elapsed(object sender, ElapsedEventArgs e) {
			lock(IPClientBmpMap) {
				foreach(IPAddress ip in IPClientBmpMap.Keys) {
					if(IPClientBmpMap[ip].TcpClientInfo == null) {
						continue;
					}

					if(++IPClientBmpMap[ip].TcpClientInfo.IdleTime >= maxIdleTime) {
						closeIP(ip);
						OnLog?.Invoke(ip, null, "超时断开连接", LogLevel.Error);
					}
					else {
						OnLog?.Invoke(ip, null, null, LogLevel.Info);
					}
				}
			}
		}

		public async Task Print(IPAddress ip, Bitmap bmp, int colorDepth) {
			await Task.Run(async () => {
				BmpInfo bmpInfo = null;
				// 获得需要打印的图片数据
				bmpInfo = new BmpInfo {
					bmp = bmp,
					printingBytes = await getPrintingBytes(bmp, colorDepth)
				};

				lock(IPClientBmpMap) {
					addIP(ip);

					IPClientBmpMap[ip].Queue.Enqueue(bmpInfo);

					OnLog?.Invoke(ip, bmpInfo?.bmp, "已进入打印队列", LogLevel.Success);

					// 如果正在打印则不做任何操作, 队列中的数据会自动打印
					if(IPClientBmpMap[ip].IsPrinting) {
						return;
					}
					IPClientBmpMap[ip].IsPrinting = true;
				}
				await prePrint(ip);
			});
		}

		private async Task<byte[]> getPrintingBytes(Bitmap bmp, int colorDepth) {
			List<byte> printingBytes = new List<byte>();

			await Task.Run(() => {
				printingBytes.AddRange(init);
				printingBytes.AddRange(getBmpBytes(bmp, colorDepth));
				printingBytes.AddRange(cut);
			});

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
			lock(IPClientBmpMap) {
				if(!IPClientBmpMap.ContainsKey(ip) || IPClientBmpMap[ip].Queue.Count == 0) {
					IPClientBmpMap[ip].IsPrinting = false;
					return;
				}
			}

			// 如果打印机没有连接则连接否则打印
			if(IPClientBmpMap[ip].TcpClientInfo == null || IPClientBmpMap[ip].StatusTcpClientInfo == null) {
				await Connect(ip);
			}
			else {
				await print(ip, IPClientBmpMap[ip].Queue.Peek());
			}
		}
		private async Task print(IPAddress ip, BmpInfo bmpInfo) {
			TcpClientInfo client = IPClientBmpMap[ip].TcpClientInfo;
			TcpClientInfo statusClient = IPClientBmpMap[ip].StatusTcpClientInfo;

			try {
				OnLog?.Invoke(ip, bmpInfo.bmp, $"正在尝试打印", LogLevel.Info);

				NetworkStream stream = client.Client.GetStream();
				NetworkStream statusStream = statusClient.Client.GetStream();

				// 先获取打印机状态服务器的状态, 并阻塞线程
				await statusStream.WriteAsync(status, 0, status.Length);

				IPClientBmpMap[ip].Mutex.Reset();
				IPClientBmpMap[ip].Mutex.WaitOne();

				// 写入打印数据流
				await stream.WriteAsync(bmpInfo.printingBytes, 0, bmpInfo.printingBytes.Length);

				
				lock(IPClientBmpMap) {
					client.IdleTime = 0;
					IPClientBmpMap[ip].Queue.Dequeue();
				}

				OnLog?.Invoke(ip, bmpInfo?.bmp, "打印成功", LogLevel.Success);

				await prePrint(ip);
			}
			catch(Exception e) {
				OnLog?.Invoke(ip, null, $"写入数据发生错误 {e.Message}", LogLevel.Error);

				lock(IPClientBmpMap) {
					closeIP(ip);
				}

				OnLog?.Invoke(ip, null, "重新尝试连接", LogLevel.Info);
				await Connect(ip);
			}
		}

		public async Task Connect(IPAddress ip) {
			ClientBmpInfo clientBmpInfo;
			lock(IPClientBmpMap) {
				addIP(ip);

				clientBmpInfo = IPClientBmpMap[ip];

				OnLog?.Invoke(ip, null, null, LogLevel.Info);

				if(clientBmpInfo.TcpClientInfo != null) {
					OnLog?.Invoke(ip, null, "已连接", LogLevel.Success);
					return;
				}
				if(clientBmpInfo.IsConnecting) {
					OnLog?.Invoke(ip, null, "正在连接中", LogLevel.Info);
					return;
				}
				clientBmpInfo.IsConnecting = true;
			}

			await connect(ip, clientBmpInfo);
		}
		private async Task connect(IPAddress ip, ClientBmpInfo clientBmpInfo) {
			TcpClient client = new TcpClient();
			TcpClient statusClient = new TcpClient();

			try {
				OnLog?.Invoke(ip, null, "正在连接", LogLevel.Info);
				await client.ConnectAsync(ip, 9100);
				await statusClient.ConnectAsync(ip, 4000);

				clientBmpInfo.TcpClientInfo = new TcpClientInfo(client);
				clientBmpInfo.StatusTcpClientInfo = new TcpClientInfo(statusClient);
				startReceiving(ip, statusClient);

				clientBmpInfo.TryTime = 0;
				clientBmpInfo.IsConnecting = false;
				OnLog?.Invoke(ip, null, "连接成功", LogLevel.Success);

				await prePrint(ip);
			}
			catch(Exception e) {
				OnLog?.Invoke(ip, null, $"连接发生错误, {e.Message}", LogLevel.Error);

				if(++clientBmpInfo.TryTime >= maxTryTime) {
					clientBmpInfo.IsConnecting = false;

					OnLog?.Invoke(ip, null, $"连接次数已达上限{clientBmpInfo.TryTime}次, 连接失败", LogLevel.Error);
					clientBmpInfo.IsPrinting = false;
					clientBmpInfo.TryTime = 0;

					return;
				}

				OnLog?.Invoke(ip, null, $"{tryInterval}秒后重新尝试第{clientBmpInfo.TryTime}次连接", LogLevel.Info);

				await Task.Delay(tryInterval * 1000);
				await connect(ip, clientBmpInfo);
			}
		}

		private void startReceiving(IPAddress ip, TcpClient client) {
			Task.Run(async () => {
				if(client == null)
					return;

				try {
					NetworkStream networkStream = client.GetStream();
					int receiveZeroCount = 0;

					while(true) {
						byte[] buffer = new byte[4];

						int lengthRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);

						// 如果持续受到0十次以上，则远程socket连接关闭
						if(lengthRead == 0) {
							receiveZeroCount++;
							if(receiveZeroCount >= 10) {
								throw new Exception("远程连接被关闭");
							}
							continue;
						}
						receiveZeroCount = 0;

						if(!isArryEqual(buffer, statusOKBuffer)) {
							OnLog(ip, null, "打印机状态错误", LogLevel.Error);
							await Task.Delay(tryInterval * 1000);
							NetworkStream stream = client.GetStream();
							await stream.WriteAsync(status, 0, status.Length);
							continue;
						}

						lock(IPClientBmpMap) {
							IPClientBmpMap[ip].Mutex.Set();
						}
					}
				}
				catch(Exception e) {
					lock(IPClientBmpMap) {
						closeIP(ip);
						IPClientBmpMap[ip].Mutex.Set();
					}

					OnLog(ip, null, $"打印机失去连接 {e.Message}", LogLevel.Error);
				}
			});
		}

		private void addIP(IPAddress ip) {
			if(!IPClientBmpMap.ContainsKey(ip)) {
				IPClientBmpMap.Add(ip, new ClientBmpInfo {
					Queue = new Queue<BmpInfo>(),
					Mutex = new AutoResetEvent(true)
				});
			}
		}
		private void closeIP(IPAddress ip) {
			IPClientBmpMap[ip].TcpClientInfo.Close();
			IPClientBmpMap[ip].StatusTcpClientInfo.Close();
			IPClientBmpMap[ip].TcpClientInfo = null;
			IPClientBmpMap[ip].StatusTcpClientInfo = null;

			IPClientBmpMap[ip].IsPrinting = false;
		}

		private bool isArryEqual(byte[] b1, byte[] b2) {
			if(b1 == null || b2 == null || b1.Length != b2.Length) {
				return false;
			}

			for(int i = 0; i < b1.Length; i++) {
				if(b1[i] != b2[i]) {
					return false;
				}
			}

			return true;
		}
	}
}
