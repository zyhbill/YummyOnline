using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GetPassword {
	class Program {
		public static string GetMd5(string str) {
			string pwd = "";

			MD5 md5 = MD5.Create();
			byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
			for(int i = 0; i < s.Length; i++) {
				pwd = pwd + s[i].ToString("X");
			}

			return pwd;
		}
		static void Main(string[] args) {
			Console.WriteLine("1: Generate Password");
			Console.WriteLine("2: Generate OnlineNotify");
			while(true) {
				Console.Write("Input: ");
				string i = Console.ReadLine();
				if(i == "1") {
					string pass = Console.ReadLine();
					Console.WriteLine(GetMd5(pass));
				}
				else if(i == "2") {
					string info = Console.ReadLine();
					Console.WriteLine(Cryptography.DesCryptography.DesEncrypt(info));
				}
			}
		}
	}
}
