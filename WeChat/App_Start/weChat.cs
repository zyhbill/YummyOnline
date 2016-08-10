using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Xml;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace WeiPay
{
    public class weChat
    {
        private string token = "wechatdianxiaoer";
        private static string ticket = string.Empty;
        //此模板字符串用于processnews
        public string templateString = @"""title"":""{0}"",
                            ""description"":""{1}"",
	                        ""picurl"":""{2}"",
	                        ""url"":""{3}""
                                ";
        public string serverUrl = "http://wechatplatform.yummyonline.net";
        /// <summary>
        /// 接受消息所用参数
        /// </summary>
        #region 参数
        public string ToUserName = string.Empty;
        //开发者微信号
        public string FromUserName = string.Empty;
        //发送方帐号 openID
        public Int64 MsgId;
        //消息id， 64位整数
        public string Content = string.Empty;
        //消息的内容
        public string MsgType = string.Empty;
        //消息类型
        public string Event = string.Empty;
        //事件类型
        public string EventKey = string.Empty;
        //点击事件的key
        /// <summary>
        /// 自定义菜单接口
        /// </summary>
        public string Latitude = string.Empty;
        //地理位置纬度
        public string Longitude = string.Empty;
        //地理位置经度
        public string Precision = string.Empty;
        //地理位置精度

        public string menu_url = " https://api.weixin.qq.com/cgi-bin/menu/create?access_token=";
        //创建按钮接口请求URL

        public Dictionary<string, int> MSGTYPE;
        #endregion
        public weChat()
        {
            Thread get_access_token = new Thread(new ThreadStart(WeChat_Base.update_access_token));
            get_access_token.IsBackground = true;
            get_access_token.Start();

        }

        public void Auth()
        {
            string echoStr = HttpContext.Current.Request.QueryString["echoStr"];
            if (CheckSignature())
            {
                if (!String.IsNullOrEmpty(echoStr))
                {
                    HttpContext.Current.Response.Write(echoStr);
                    HttpContext.Current.Response.End();
                }
            }
        }
        public bool CheckSignature()
        {
            string signature = HttpContext.Current.Request.QueryString["signature"];
            string timestamp = HttpContext.Current.Request.QueryString["timestamp"];
            string nonce = HttpContext.Current.Request.QueryString["nonce"];
            string[] arraytmp = { token, timestamp, nonce };
            Array.Sort(arraytmp);
            string tmpstr = String.Join("", arraytmp);
            tmpstr = FormsAuthentication.HashPasswordForStoringInConfigFile(tmpstr, "SHA1");
            tmpstr = tmpstr.ToLower();

            if (tmpstr == signature)
                return true;
            else
                return false;
        }
        public static string getTicket()
        {
            return WeChat_Base.jsapi_ticket;
        }
        /// <summary>
        /// 此函数用于处理/接收文本类消息
        /// </summary>
        public void ProcessMsg()
        {
            string postXML = string.Empty;
            if (HttpContext.Current.Request.HttpMethod == "POST")
            {
                using (Stream stream = HttpContext.Current.Request.InputStream)
                {
                    Byte[] postByte = new Byte[stream.Length];
                    stream.Read(postByte, 0, (Int32)stream.Length);
                    postXML = Encoding.UTF8.GetString(postByte);
                    //postXML存储了接收到的消息
                    HandleMsg(postXML);
                }
            }
        }
        /// <summary>
        /// 处理xml消息格式
        /// </summary>
        /// <param name="xml"></param>
        /// xml样式为
        /* <xml>
             <ToUserName><![CDATA[toUser]]></ToUserName>
             <FromUserName><![CDATA[fromUser]]></FromUserName> 
            <CreateTime>1348831860</CreateTime>
             <MsgType><![CDATA[text]]></MsgType>
             <Content><![CDATA[this is a test]]></Content>
             <MsgId>1234567890123456</MsgId>
             </xml>
         */
        public void HandleMsg(String xml)
        {
            //类型初始化
            MSGTYPE = new Dictionary<string, int>();
            MSGTYPE.Add("text", 1);//文本消息
            MSGTYPE.Add("image", 2);//图文消息
            MSGTYPE.Add("voice", 3);//语音消息
            MSGTYPE.Add("video", 4);//视频消息

            string content = string.Empty;
            XmlDocument xd = new XmlDocument();
            xd.Load(xml);
            XmlNode xmlnode = xd.SelectSingleNode("xml/ToUserName");
            if (xmlnode != null)
                this.ToUserName = xmlnode.InnerText;

            xmlnode = xd.SelectSingleNode("xml/FromUserName");
            if (xmlnode != null)
                this.FromUserName = xmlnode.InnerText;

            xmlnode = xd.SelectSingleNode("xml/MsgId");
            if (xmlnode != null)
                this.MsgId = Int64.Parse(xmlnode.InnerText);

            xmlnode = xd.SelectSingleNode("xml/Content");
            if (xmlnode != null)
                content = xmlnode.InnerText;
            Content = content;

            xmlnode = xd.SelectSingleNode("xml/MsgType");
            if (xmlnode != null)
                MsgType = xmlnode.InnerText;

            xmlnode = xd.SelectSingleNode("xml/Event");
            if (xmlnode != null)
                Event = xmlnode.InnerText;

            #region 地理位置
            xmlnode = xd.SelectSingleNode("xml/Latitude");
            if (xmlnode != null)
                Latitude = xmlnode.InnerText;
            xmlnode = xd.SelectSingleNode("xml/Longitude");
            if (xmlnode != null)
                Longitude = xmlnode.InnerText;
            xmlnode = xd.SelectSingleNode("xml/Precision");
            if (xmlnode != null)
                Precision = xmlnode.InnerText;

            #endregion
            #region 信息类型
            switch (MsgType)
            {
                #region 文本信息
                case "text":
                    {
                        if (Content != "1"
                        && Content != "2")
                        {
                            //ProcessMsgText("回复以下代号:\n1.所有的餐厅\n2.近期火热的菜品TOP10");
                            ProcessMsgText("欢迎关注在线美味!\n回复以下代号:\n1.所有的餐厅\n2.近期火热的菜品TOP10\n或者输入想吃什么");
                            //ProcessMsgText(@"");
                        }
                        else if (Content == "1")
                        {
                            //回复news
                            //对应所有的餐厅
                            string tmp = @"{
	                            ""title"":""支持在线美味所有的餐厅"",
                                ""description"":""所有餐厅"",
	                            ""picurl"":""http://114.215.107.208/weixin/wechat/res/img/A2003.jpg"",
	                            ""url"":""http://114.215.107.208/weixin/wechat/wechat.aspx""
                                    }";
                            ProcessNews(tmp);
                        }
                        else
                        {
                            //回复news
                            //查找相应的菜品TOP10
                            string tmp = @"{
	                            ""title"":""TOP10美味"",
                                ""description"":""在线美味TOP10"",
	                            ""picurl"":""http://114.215.107.208/weixin/res/img/A1003.jpg"",
	                            ""url"":""http://114.215.107.208/weixin/wechat/wechat.aspx""
                                    }";
                            ProcessNews(tmp);
                        }
                        break;
                    }

                #endregion
                #region 事件信息
                case "event":
                    {
                        switch (Event)
                        {
                            //关注事件
                            case "subscribe":
                                subscribe();
                                break;
                            case "click":
                                //点击事件
                                {
                                    xmlnode = xd.SelectSingleNode("xml/EventKey");
                                    if (xmlnode != null)
                                        EventKey = xmlnode.InnerText;
                                    switch (EventKey)
                                    {
                                        case "cli_openid":
                                            {
                                                //获取openid

                                                ProcessMsgText("openid:\n" + FromUserName);
                                                break;
                                            }
                                        case "cli_myinfo":
                                            {
                                                //会员注册
                                                string tmp1 = string.Format(templateString, "店小二平台会员注册", "欢迎您，在线美味尊贵会员"
                                                                                                    , serverUrl + "res/img/A1005.jpg"
                                                                                                    , "http://114.215.107.208/weixin/wechat/wechat.aspx");
                                                ProcessNews("{" + tmp1 + "}");
                                                break;
                                            }
                                        case "cli_updateinfo":
                                            {
                                                //信息查询
                                                string tmp1 = string.Format(templateString, "会员信息及积分查询", "欢迎您，在线美味尊贵会员"
                                                    , serverUrl + "res/img/A1006.jpg"
                                                    , "http://114.215.107.208/weixin/wechat/wechat.aspx");
                                                ProcessNews("{" + tmp1 + "}");
                                                break;
                                            }
                                        case "cli_historyinfo":
                                            {
                                                //历史订单
                                                string tmp1 = string.Format(templateString, "历史订单查询", "欢迎您，在线美味尊贵会员"
                                                , serverUrl + "res/img/A1007.jpg"
                                                , "http://114.215.107.208/weixin/wechat/wechat.aspx");
                                                ProcessNews("{" + tmp1 + "}");
                                                break;
                                            }
                                        case "cli_all":
                                            {
                                                //会员优惠活动
                                                string tmp1 = string.Format(templateString, "会员优惠活动", "在线美味欢迎您"
                                                , serverUrl + "res/img/A2001.jpg"
                                                , "http://114.215.107.208/weixin/wechat/wechat.aspx");
                                                ProcessNews("{" + tmp1 + "}");
                                                break;
                                            }
                                        case "cli_allres":
                                            {
                                                string tmp1 = string.Format(templateString, "所有餐厅", "在线美味欢迎您"
                                                , serverUrl + "res/img/A1002.jpg"
                                                , "http://114.215.107.208/weixin/wechat/wechat.aspx");
                                                ProcessNews("{" + tmp1 + "}");
                                                break;
                                            }
                                        //case "cli_board":
                                        //{
                                        //        string tmp1 = string.Format(templateString, "餐厅排行", "在线美味欢迎您"
                                        //        , serverUrl + "res/img/A2002.jpg"
                                        //        , "http://114.215.107.208/weixin/wechat/wechat.aspx");
                                        //        ProcessNews("{" + tmp1 + "}");
                                        //       break;
                                        //}
                                        case "cli_near":
                                            {

                                                string tmp1 = string.Format(templateString, "附近的店", "在线美味欢迎您"
                                                , serverUrl + "res/img/A2003.jpg"
                                                , "http://m.amap.com/navi/?dest=121.392385,31.315824&destName=上海大学&hideRouteIcon=1&key=838344be6c55fda45c1f1c6d77736626");
                                                ProcessNews("{" + tmp1 + "}");
                                                break;
                                            }
                                        case "cli_com":
                                            {
                                                string tmp1 = string.Format(templateString, "公司介绍", "在线美味欢迎您"
                                                , serverUrl + "res/img/main.png"
                                                , "http://mp.weixin.qq.com/s?__biz=MzA3MTg2OTg3NA==&mid=208993449&idx=1&sn=8022e7b419b489f37111284d4f0dc096&scene=18#wechat_redirect");
                                                ProcessNews("{" + tmp1 + "}");
                                                break;
                                            }
                                        case "cli_comAct":
                                            {
                                                string tmp1 = string.Format(templateString, "联系我们", "在线美味欢迎您"
                                                , serverUrl + "res/img/main.png"
                                                , "http://mp.weixin.qq.com/s?__biz=MzA3MTg2OTg3NA==&mid=208993669&idx=1&sn=5bc2bbbab2c462dab97fd3423f4a6706&scene=18#rd");
                                                ProcessNews("{" + tmp1 + "}", 2);
                                                break;
                                            }
                                        case "cli_comCon":
                                            {
                                                string tmp1 = string.Format(templateString, "加盟我们", "在线美味欢迎您"
                                                , serverUrl + "res/img/main.png"
                                                , "http://mp.weixin.qq.com/s?__biz=MzA3MTg2OTg3NA==&mid=208993669&idx=1&sn=5bc2bbbab2c462dab97fd3423f4a6706&scene=18#rd");
                                                ProcessNews("{" + tmp1 + "}", 2);
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case "LOCATION":
                                //   ProcessMsgText(string.Format("地理位置详情:{0}\n{1}\n{2}\n",
                                //                                                   Latitude, Longitude, Precision));
                                break;
                            default:
                                break;
                        }
                        break;
                    }
                default:
                    break;
                    #endregion
            }

            #endregion
        }
        /// <summary>
        /// 回复文本消息
        /// </summary>
        /// <param name="content"></param>
        /// <example>
        /* 
            <xml>
            <ToUserName><![CDATA[toUser]]></ToUserName>
            <FromUserName><![CDATA[fromUser]]></FromUserName>
            <CreateTime>12345678</CreateTime>
            <MsgType><![CDATA[text]]></MsgType>
            <Content><![CDATA[你好]]></Content>
            </xml>
        */
        /// </example>
        public void ProcessMsgText(string content)
        {

            StringBuilder bxml = new StringBuilder();
            bxml.AppendFormat(@"<xml>
                             <ToUserName><![CDATA[{0}]]></ToUserName>
                             <FromUserName><![CDATA[{1}]]></FromUserName>
                             <CreateTime>{2}</CreateTime>
                             <MsgType><![CDATA[{3}]]></MsgType>
                             <Content><![CDATA[{4}]]></Content>
                             </xml>"
            , FromUserName, ToUserName, Helper.convertTime2String(DateTime.Now)
            , "text", content);

            HttpContext.Current.Response.Write(bxml.ToString());
        }
        /// <summary>
        /// 用户关注后事件接口函数
        /// </summary>
        public void subscribe()
        {
            //ProcessMsgText("欢迎关注在线美味!\n回复以下代号:\n1.所有的餐厅\n2.近期火热的菜品TOP10\n或者输入想吃什么");
            //Thread.Sleep(500);
            string tmp = @"{
	                        ""title"":""在线美味--轻松点餐"",
                            ""description"":""会员活动，所有餐厅"",
	                        ""picurl"":""http://wechatold.yummyonline.net/weixin/res/img/A2002.jpg"",
	                        ""url"":""http://mp.weixin.qq.com/s?__biz=MzA3MTg2OTg3NA==&mid=208993449&idx=1&sn=8022e7b419b489f37111284d4f0dc096#rd""
                                }";
            ProcessNews(tmp);
        }
        /// <summary>
        /// 处理图文消息函数
        /// </summary>
        /// <param name="content"></param>
        public void ProcessNews(string content, int count = 1)
        {
            if (count == 1)
            {
                //content中保存json数据包
                JObject json = JObject.Parse(content);
                string title = json["title"].ToString();
                string description = json["description"].ToString();
                string picurl = json["picurl"].ToString();
                string url = json["url"].ToString();
                //string url = "http://114.215.107.208/weixin/wechat/wechat.aspx";
                //string picurl = "http://114.215.107.208/weixin/res/img/A1006.jpg";
                StringBuilder bxml = new StringBuilder();
                bxml.AppendFormat(@"<xml>
                        <ToUserName><![CDATA[{0}]]></ToUserName>
                        <FromUserName><![CDATA[{1}]]></FromUserName>
                        <CreateTime>{2}</CreateTime>

                        <MsgType><![CDATA[news]]></MsgType>
                        <ArticleCount>1</ArticleCount>
                        <Articles>
                        <item>
                        <Title><![CDATA[{3}]]></Title> 
                        <Description><![CDATA[{4}]]></Description>
                        <PicUrl><![CDATA[{5}]]></PicUrl>
                        <Url><![CDATA[{6}]]></Url>
                        </item>
                        </Articles>
                        </xml> ", FromUserName, ToUserName, Helper.convertTime2String(DateTime.Now),
                           title, description, picurl, url);// <PicUrl><![CDATA[{5}]]></PicUrl> );
                HttpContext.Current.Response.Write(bxml.ToString());
            }
            else if (count == 2)
            {
                //content中保存json数据包
                JObject json = JObject.Parse(content);
                string title = json["title"].ToString();
                string description = json["description"].ToString();
                string picurl = json["picurl"].ToString();
                string url = json["url"].ToString();
                //string url = "http://114.215.107.208/weixin/wechat/wechat.aspx";
                //string picurl = "http://114.215.107.208/weixin/res/img/A1007.jpg";
                StringBuilder bxml = new StringBuilder();
                bxml.AppendFormat(@"<xml>
                        <ToUserName><![CDATA[{0}]]></ToUserName>
                        <FromUserName><![CDATA[{1}]]></FromUserName>
                        <CreateTime>{2}</CreateTime>
                        <MsgType><![CDATA[news]]></MsgType>
                        <ArticleCount>2</ArticleCount>
                        <Articles>
                        <item>
                        <Title><![CDATA[{3}]]></Title> 
                        <Description><![CDATA[{4}]]></Description>
                        <PicUrl><![CDATA[{5}]]></PicUrl>
                        <Url><![CDATA[{6}]]></Url>
                        </item>
                        <item>
                        <Title><![CDATA[{3}]]></Title> 
                        <Description><![CDATA[{4}]]></Description>
                        <PicUrl><![CDATA[{5}]]></PicUrl>
                        <Url><![CDATA[{6}]]></Url>
                        </item>
                        </Articles>
                        </xml> ", FromUserName, ToUserName, Helper.convertTime2String(DateTime.Now),
                           title, description, picurl, url);// <PicUrl><![CDATA[{5}]]></PicUrl> );
                HttpContext.Current.Response.Write(bxml.ToString());
            }

        }

        /// <summary>
        /// 调用setButton函数请注意
        /// 1.修改按钮格式
        /// 2.只有需要的时候在PageLoad中调用
        /// 3.如果不再需要注意注释掉！
        /// </summary>
        public void SetButton()
        {
            string json = @"{""button"":
                            [{
	                            ""name"":""会员中心"",
	                            ""sub_button"":[
	                            {
		                            ""type"":""view"",
		                            ""name"":""积分查询"",
		                            ""url"":""http://wechatplatform.yummyonline.net""
	                            },
	                            {
		                            ""type"":""click"",
		                            ""name"":""test"",
		                            ""key"":""cli_openid""
	                            }]
	                            }]
                            }";

            string URL = string.Empty;
            while (WeChat_Base.access_token == string.Empty
                 || URL == string.Empty)
                URL = menu_url + WeChat_Base.access_token;
            HttpHelper.sendMsgByPost(URL, json);
        }
    }

}
