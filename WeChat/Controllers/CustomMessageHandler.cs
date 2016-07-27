using System;
using System.IO;
using System.Linq;
using System.Web;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.Context;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.Sample.CommonService.CustomMessageHandler;
using WeiPay;

namespace WeChat.Controllers
{
    public class CustomMessageHandler:MessageHandler<CustomMessageContext>
    {
        private Stream inputStream;
        private PostModel postModel;

        public CustomMessageHandler(Stream inputStream, PostModel postModel):base(inputStream,postModel)
        {
            this.inputStream = inputStream;
            this.postModel = postModel;
        }

        internal new void  Execute()
        {
            //var hm = new YummyOnlineManager();
            //hm.Log("Execute",inputStream.ToString());
        }

        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您的OpenID是：" + requestMessage.FromUserName      
                                    + "。\r\n您发送了文字信息：" + requestMessage.Content;  
            return responseMessage;
        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            throw new NotImplementedException();
        }
    }
}