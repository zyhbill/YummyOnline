using System.Linq;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.Context;
using System.IO;
using System.Web;
using System.Web.Configuration;
using WeChat;
using System.Text;

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
                case "Openid":
                    {
                        var strongResposeMessage = CreateResponseMessage<ResponseMessageText>();
                        strongResposeMessage.Content = "尊敬的用户，您好";
                        reponseMessage = strongResposeMessage;
                    }
                    break;

                case "Points":
                    {
                        var strongResposeMessage = CreateResponseMessage<ResponseMessageText>();
                        strongResposeMessage.Content = Points();
                        reponseMessage = strongResposeMessage;

                    }
                    break;
                case "History":
                    {
                        var strongResposeMessage = CreateResponseMessage<ResponseMessageNews>();
                        strongResposeMessage.Articles.Add(new Article()
                        {
                            Title = "查看历史订单",
                            Description = "历史订单",
                            PicUrl = "http://img.ivsky.com/img/tupian/pre/201012/04/katong_meishi-010.jpg",
                            Url = "http://wechatplatform.yummyonline.net/history/history/?openid=" + WeixinOpenId
                        });
                        reponseMessage = strongResposeMessage;
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
            end.AppendFormat("您的总积分： {0}",points);
            //end.AppendLine("\r\n");
            //end.AppendFormat("");
            return end.ToString();//string.Format("您的积分：{0}", points);

        }


        //关注事件
        public override IResponseMessageBase OnEvent_SubscribeRequest(RequestMessageEvent_Subscribe requestMessage)
        {
            var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageNews>(requestMessage);
            responseMessage.Articles.Add(new Article()
            {
                Title = "欢迎关注店小二",
                Description = "会员绑定",
                PicUrl = "http://m2.biz.itc.cn/pic/new/f/57/23/Img3832357_f.jpg",
                Url = "http://wechatplatform.yummyonline.net/login/login/?openid=" + WeixinOpenId
            });
            return responseMessage;
        }
    }
}
