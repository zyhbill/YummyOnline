using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace WeiPay
{
    public class HttpHelper
    {

        public static string sendMsgByGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            //request.UserAgent = DefaultUserAgent;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string content = reader.ReadToEnd();
            reader.Close();
            return content;                                    
        }

        public static ArrayList sendMsgByPostAndGetCookie(string url, string param)
        {
            ArrayList arrList = new ArrayList();
            string strURL = url;
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";

            string paraUrlCoded = param;
            byte[] payload;
            payload = Encoding.UTF8.GetBytes(paraUrlCoded);
            request.ContentLength = payload.Length;

            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            String tempCookie = response.GetResponseHeader("Set-Cookie");
        
            //todo: 
            arrList.Add(tempCookie);
            //=========
            Stream s;
            s = response.GetResponseStream();

            string StrDate = "";
            string strValue = "";

            StreamReader Reader = new StreamReader(s, Encoding.UTF8);
            while ((StrDate = Reader.ReadLine()) != null)
            {
                strValue += StrDate;// + "\r\n";
            }
            arrList.Add(strValue);
            return arrList;
        }
        public static string sendMsgByPost(string url, string param)
        {
            //ArrayList arrList = new ArrayList();
            string strURL = url;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";

            string paraUrlCoded = param;
            byte[] payload;
            payload = Encoding.UTF8.GetBytes(paraUrlCoded);
            request.ContentLength = payload.Length;

            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            
            Stream s;
            s = response.GetResponseStream();

            string StrDate = "";
            string strValue = "";

            StreamReader Reader = new StreamReader(s, Encoding.UTF8);
            while ((StrDate = Reader.ReadLine()) != null)
            {
                strValue += StrDate;// + "\r\n";
            }

            return strValue;
        }
        public static string sendMsgByPost(string url, string param, string cookies)
        {
            //ArrayList arrList = new ArrayList();
            string strURL = url;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";
            request.Headers.Set("Cookie", cookies);
            string paraUrlCoded = param;
            byte[] payload;
            payload = Encoding.UTF8.GetBytes(paraUrlCoded);
            request.ContentLength = payload.Length;

            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream s;
            s = response.GetResponseStream();

            string StrDate = "";
            string strValue = "";

            StreamReader Reader = new StreamReader(s, Encoding.UTF8);
            while ((StrDate = Reader.ReadLine()) != null)
            {
                strValue += StrDate;// + "\r\n";
            }

            return strValue;
        }

    }
}
