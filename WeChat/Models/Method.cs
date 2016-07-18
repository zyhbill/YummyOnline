using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WeChat.Models
{
    public class Method
    {
        public static string GetMd5(string str)
        {
            string pwd = "";
            //#if DEBUG
            //            pwd = str;
            //#else
            System.Security.Cryptography.MD5 md5 = MD5.Create();
			byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
			for(int i = 0; i < s.Length; i++) {
				pwd = pwd + s[i].ToString("X");
			}
//#endif
            return pwd;
        }
    }
}