using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        public static bool Send(string mobile, string code)
        {
            string data = "apikey=2763feb79430838ffecf355f9454e624&";
            data += "mobile=" + mobile + "&";
            data += "text=【上海乔曦信息技术有限公司】您的验证码是" + code;
            string result = PostWebRequest("http://yunpian.com/v1/sms/send.json", data);
            if (result.Contains("OK"))
            {
                return true;
            }
            return false;
        }

        private static string PostWebRequest(string postUrl, string paramData)
        {
            string ret = string.Empty;
            byte[] byteArray = Encoding.UTF8.GetBytes(paramData); //转化
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
            webReq.Method = "POST";
            webReq.ContentType = "application/x-www-form-urlencoded";

            webReq.ContentLength = byteArray.Length;
            Stream newStream = webReq.GetRequestStream();
            newStream.Write(byteArray, 0, byteArray.Length);//写入参数
            newStream.Close();
            HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default);
            ret = sr.ReadToEnd();
            sr.Close();
            response.Close();
            return ret;
        }

    }
}