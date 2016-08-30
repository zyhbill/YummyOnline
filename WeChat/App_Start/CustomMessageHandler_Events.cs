using System.Linq;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.Context;
using System.IO;
using System.Web;
using System.Web.Configuration;
using WeChat;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft;
using WeiPay;
using System;

namespace MessageHandle
{
    public partial class CustomMessageHandler : MessageHandler<MessageContext<IRequestMessageBase, IResponseMessageBase>>
    {
        private string appId = WebConfigurationManager.AppSettings["appId"];
        private string appSecret = WebConfigurationManager.AppSettings["appSecret"];

        public override IResponseMessageBase OnEvent_ClickRequest(RequestMessageEvent_Click requestMessage)
        {
            IResponseMessageBase reponseMessage = null;
            switch (requestMessage.EventKey)
            {
                case "Account":
                    {
                        var strongResposeMessageA = CreateResponseMessage<ResponseMessageNews>();
                        var strongResposeMessageT = CreateResponseMessage<ResponseMessageText>();
                        if (isUser() == true)
                        {
                            strongResposeMessageT.Content = "您已注册绑定~";
                            reponseMessage = strongResposeMessageT;
                        }
                        else
                        {
                            strongResposeMessageA.Articles.Add(new Article()
                            {
                                Title="会员注册",
                                Description= "店小二温馨提示：客官请先注册会员才能查看哦～",
                                PicUrl= "http://static.yummyonline.net/dianxiaoer/bigbang.jpg",
                                Url= "http://wechatplatform.yummyonline.net/account/?openid="+WeixinOpenId
                            });
                            reponseMessage = strongResposeMessageA;
                        }

                    }
                    break;
                case "Points":
                    {
                        var strongResposeMessageF = CreateResponseMessage<ResponseMessageNews>();
                        var strongResposeMessageT = CreateResponseMessage<ResponseMessageText>();
                        if (isUser() == false)
                        {
                            strongResposeMessageF.Articles.Add(new Article()
                            {
                                Title = "会员绑定",
                                Description = "店小二温馨提示：客官请先注册会员再绑定哦～",
                                PicUrl = "http://static.yummyonline.net/dianxiaoer/bigbang.jpg",
                                Url = "http://wechatplatform.yummyonline.net/login/login/?openid=" + WeixinOpenId
                            });
                            reponseMessage = strongResposeMessageF;
                        }
                        else
                        {
                            strongResposeMessageT.Content = Points();
                            reponseMessage = strongResposeMessageT;
                        }
                    }
                    break;
                case "History":
                    {
                        var strongResposeMessage = CreateResponseMessage<ResponseMessageNews>();
                        if (isUser() == false)
                        {
                            strongResposeMessage.Articles.Add(new Article()
                            {
                                Title = "会员绑定",
                                Description = "店小二温馨提示：客官请先注册会员再绑定哦～",
                                PicUrl = "http://static.yummyonline.net/dianxiaoer/bigbang.jpg",
                                Url = "http://wechatplatform.yummyonline.net/login/login/?openid=" + WeixinOpenId
                            });
                            reponseMessage = strongResposeMessage;
                        }
                        else
                        {
                            strongResposeMessage.Articles.Add(new Article()
                            {
                                Title = "查看历史订单",
                                Description = "历史订单",
                                PicUrl = "http://img.ivsky.com/img/tupian/pre/201012/04/katong_meishi-010.jpg",
                                Url = "http://wechatplatform.yummyonline.net/history/history/?openid=" + WeixinOpenId
                            });
                            reponseMessage = strongResposeMessage;
                        }

                    }
                    break;

                case "isProduct":
                    {
                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                        strongResponseMessage.Content =product();
                        reponseMessage = strongResponseMessage;
                    }
                    break;

                case "Product":
                    {
                        var strongResponseMessage = CreateResponseMessage<ResponseMessageNews>();
                        strongResponseMessage.Articles.Add(new Article()
                        {
                            Title= "店小二系统总图",
                            PicUrl= "http://static.yummyonline.net/dianxiaoer/zong.jpg",
                            Url= "http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000081&idx=1&sn=2505ac9e23995714b49cca1122ece2bd#rd"
                        });
                        strongResponseMessage.Articles.Add(new Article()
                        {
                            Title = "店小二版本",
                            PicUrl = "http://static.yummyonline.net/dianxiaoer/all.jpg",
                            Url = "http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000083&idx=1&sn=bc0bbf22bba637121f7bedc919003147#rd"
                        });
                        strongResponseMessage.Articles.Add(new Article()
                        {
                            Title = "店小二介绍",
                            PicUrl = "http://static.yummyonline.net/dianxiaoer/dianxiaoer.jpg",
                            Url = "http://www.rabbitpre.com/m/fERuy7QYX"
                        });
                        strongResponseMessage.Articles.Add(new Article()
                        {
                            Title = "店掌柜介绍",
                            PicUrl = "http://static.yummyonline.net/dianxiaoer/dian.jpg",
                            Url = "http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000086&idx=1&sn=e032e82cff9d642dfcbe1788f9065f80#rd"
                        });
                        strongResponseMessage.Articles.Add(new Article()
                        {
                            Title = "云掌柜介绍",
                            PicUrl = "http://static.yummyonline.net/dianxiaoer/yun.jpg",
                            Url = "http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000088&idx=1&sn=15e9880802e286445eca480ab329d362#rd"
                        });
                        reponseMessage = strongResponseMessage;
                    }
                    break;
                case "Contact":
                    {
                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                        strongResponseMessage.Content =contact();
                        reponseMessage = strongResponseMessage;
                    }
                    break;

                default:
                    {
                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                        strongResponseMessage.Content = "您好";
                        reponseMessage = strongResponseMessage;
                    }
                    break;
            }

            return reponseMessage;
        }

        //积分查询
        public string Points()
        {
            var yummonlineManager = new YummyOnlineManager();
            var wechatid = WeixinOpenId;
            var ctx = new YummyOnlineDAO.Models.YummyOnlineContext();
            var result = ctx.Users.Where(p => p.WeChatOpenId == wechatid).Select(d => new { d.Id }).FirstOrDefault();
            if (result == null)
            {
                return string.Format(@"请绑定会员:{0}{1}", "http://wechatplatform.yummyonline.net/login/login/?openid=", WeixinOpenId);
            }
            int points = 0;
            var hotels = ctx.Hotels.Where(d => d.Usable == true).ToList();
            foreach (var i in hotels)
            {
                var ConnectStr = i.ConnectionString;
                var HotelManager = new HotelManager(ConnectStr);
                points += HotelManager.GetUserPointById(result.Id);
            }
            var end = new StringBuilder();
            end.AppendFormat("您的总积分： {0}", points);
            //end.AppendLine("\r\n");
            //end.AppendFormat("");
            return end.ToString();//string.Format("您的积分：{0}", points);

        }
        //产品介绍
        public string product()
        {
            var end = new StringBuilder();
            end.AppendLine("请输入下列数字");
            end.AppendLine("1.店小二系统总图");
            end.AppendLine("2.店小二版本种类");
            end.AppendLine("3.店小二介绍");
            end.AppendLine("4.店掌柜介绍");
            end.AppendLine("5.云掌柜介绍");
            return end.ToString();
        }

        public string testProduct()
        {
            var result = new StringBuilder();
            result.AppendFormat("<a href={0}>1.店小二系统总图</a>", "http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000081&idx=1&sn=2505ac9e23995714b49cca1122ece2bd#rd");
            result.AppendLine("\r\n");
            result.AppendFormat("<a href={1}>2.店小二版本种类</a>", "http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000083&idx=1&sn=bc0bbf22bba637121f7bedc919003147#rd");
            result.AppendLine("\r\n");
            result.AppendFormat("<a href={2}>3.店小二介绍</a>", "http://www.rabbitpre.com/m/fERuy7QYX");
            result.AppendLine("\r\n");
            result.AppendFormat("<a href={3}>4.店掌柜介绍</a>", "http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000086&idx=1&sn=e032e82cff9d642dfcbe1788f9065f80#rd");
            result.AppendLine("\r\n");
            result.AppendFormat("<a href={4}>5.云掌柜介绍</a>", "http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000088&idx=1&sn=15e9880802e286445eca480ab329d362#rd");
            result.AppendLine("输入数字查看");
            return result.ToString();
        }



        //联系方式
        public string contact()
        {
            var end = new StringBuilder();
            end.AppendLine("电话：021-6660 1020");
            end.AppendLine("传真：021-6660 1020");
            end.AppendLine("邮箱：choice_dxe@163.com");
            end.AppendLine("地址：上海市虹口区花园路66弄1号602室");
            return end.ToString();
        }

        //判断用户openid是否存在
        public bool isUser()
        {
            var yummonlineManager = new YummyOnlineManager();
            var wechatid = WeixinOpenId;
            var ctx = new YummyOnlineDAO.Models.YummyOnlineContext();
            var result = ctx.Users.Where(p => p.WeChatOpenId == wechatid).Select(d => new { d.Id }).FirstOrDefault();
            if (result == null)
                return false;
            else
                return true;
        }

        //判断用户userID是否存在
        public bool isUserID(string userid)
        {
            var yummonlineManager = new YummyOnlineManager();
            var ctx = new YummyOnlineDAO.Models.YummyOnlineContext();
            var result = ctx.Users.Where(p => p.Id == userid).Select(d => new { d.WeChatOpenId }).FirstOrDefault();
            if (result == null)
                return false;
            else
                return true;
        }

        //关注事件
        public override IResponseMessageBase OnEvent_SubscribeRequest(RequestMessageEvent_Subscribe requestMessage)
        {
            var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageNews>(requestMessage);
            responseMessage.Articles.Add(new Article()
            {
                Title = "欢迎关注店小二",
                Description = "店小二温馨提示：客官请先注册会员再绑定哦～",
                PicUrl = "http://static.yummyonline.net/dianxiaoer/bigbang.jpg",
                Url = "http://wechatplatform.yummyonline.net/login/login/?openid=" + WeixinOpenId
            });
            return responseMessage;
        }

        //发送模板消息
        //POST
        public JObject Notice()
        {
            if (isUser() == false)
            {
                return null;
            }
            else
            {
                JObject relJson = DineID();
                string pay = relJson["Price"].ToString();
                string time = relJson["BeginTime"].ToString();
                string userid = relJson["UserId"].ToString();
                if (isUserID(userid) == false)
                    return null;
                else
                {
                    string url = "https://api.weixin.qq.com/cgi-bin/message/template/send?access_token=ACCESS_TOKEN";
                    var templatdID = "ZdG4C7FzuHV_NiZ57al_XweuFHAvfMWhpqVmDhqA5wI";
                    JObject json = new JObject();
                    json.Add(new JProperty("touser", WeixinOpenId));
                    json.Add(new JProperty("template_id", templatdID));
                    json.Add(new JProperty("url", url));
                    JObject payData = new JObject();
                    payData.Add(new JProperty("value", pay));//消费金额
                    payData.Add(new JProperty("color", "#173177"));
                    JObject addressData = new JObject();
                    addressData.Add(new JProperty("value", "店小二"));//消费地址
                    addressData.Add(new JProperty("color", "#173177"));
                    JObject timeData = new JObject();
                    timeData.Add(new JProperty("value", time));//消费时间
                    timeData.Add(new JProperty("color", "#173177"));
                    JObject remarkData = new JObject();
                    remarkData.Add(new JProperty("value", "欢迎再次光临！"));
                    remarkData.Add(new JProperty("color", "#173177"));
                    JObject data = new JObject();
                    data.Add(new JProperty("keynote1", payData));
                    data.Add(new JProperty("keynote2", addressData));
                    data.Add(new JProperty("keynote3", timeData));
                    data.Add(new JProperty("keynote4", remarkData));
                    json.Add(new JProperty("data", data));
                    var js = HttpHelper.sendMsgByPost(url, json.ToString());
                    JObject jn = new JObject(js);
                    return jn;
                }
            }
        }

        //根据订单号获取点单
        public JObject DineID()
        {
            string URL = "waiter.yummyonline.net";
            string url_ = URL + "/Order/GetDineById";
            JObject json = new JObject();
            //json.Add(new JProperty("DineID", DineID));//获取订单号DineID
            var result = HttpHelper.sendMsgByPost(url_, json.ToString());
            JObject resJson = new JObject(result);
            return resJson;
        }
    }
}
