using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication {
	class Program {
		static void Main(string[] args) {
			Console.WriteLine(DriveSpace.GetHardDiskFreeSpace());
			Console.WriteLine(DriveSpace.GetHardDiskSpace());
			Console.Read();
		}
	}

	public class DriveSpace {
		/// <summary>
		/// 获取驱动器空间大小
		/// </summary>
		/// <returns>大小(GB)</returns>
		public static int GetHardDiskSpace() {
			int totalSize = 0;
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach(DriveInfo drive in drives) {
				if(drive.DriveType == DriveType.Fixed) {
					totalSize += Convert.ToInt32(drive.TotalSize / (1024 * 1024 * 1024));
				}
			}
			return totalSize;
		}
		/// <summary>
		/// 获取驱动器剩余空间大小
		/// </summary>
		/// <returns>大小(GB)</returns>
		public static int GetHardDiskFreeSpace() {
			int freeSpace = 0;
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach(DriveInfo drive in drives) {
				if(drive.DriveType == DriveType.Fixed) {
					freeSpace = Convert.ToInt32(drive.TotalFreeSpace / (1024 * 1024 * 1024));
				}
			}
			return freeSpace;
		}
	}
}
