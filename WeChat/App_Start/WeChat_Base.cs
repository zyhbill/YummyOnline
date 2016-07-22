using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using System.Threading;
namespace WeiPay
{
    class WeChat_Base
    {
        /// <summary>
        /// 用于获取基本信息的类
        /// <para>
        /// appid, appsecret请不要更改
        /// sleep_time设置为7000秒
        /// 每7000秒线程更新一次access_token
        /// </para>
        /// </summary>

        private static string appid = PayConfig.AppId;
        private static string appsecret = PayConfig.AppSecret;

        public static string access_token = null;
        public static string jsapi_ticket = null;
        public static int sleep_time = 7000 * 1000;

        public static string get_url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential";
        public static string jsapi_ur = "https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi";
        public static void update_access_token()
        {

            string url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=wxa6f547fa6bc50c20&secret=891aebd4347061f9fdcce8b5b76a247b";
            //string.Format(get_url + "&appid={0}&appsecret={1}", appid, appsecret);
            while(true)
            {
                string content = HttpHelper.sendMsgByGet(url);
                JObject json = JObject.Parse(content);
               
                if (json["access_token"] != null)
                {
                    access_token = json["access_token"].ToString();
                    string jsapi_content = HttpHelper.sendMsgByGet(string.Format(jsapi_ur, access_token));
  
                    JObject json1 = JObject.Parse(jsapi_content);
                    if (json1["ticket"] != null)
                    {
                        jsapi_ticket = json1["ticket"].ToString();
                    }
                    

                }
                Thread.Sleep(sleep_time);
            }
        }


    }
}
