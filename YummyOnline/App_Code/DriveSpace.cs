using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace YummyOnline.Utility {
	public class DriveSpace {
		/// <summary>
		/// 获取驱动器空间大小
		/// </summary>
		/// <returns>大小(GB)</returns>
		public static int GetHardDiskSpace() {
			int totalSize = 0;
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach(DriveInfo drive in drives) {
				totalSize += Convert.ToInt32(drive.TotalSize / (1024 * 1024 * 1024));
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
				freeSpace = Convert.ToInt32(drive.TotalFreeSpace / (1024 * 1024 * 1024));
			}
			return freeSpace;
		}
	}
}