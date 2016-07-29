using System;
using System.IO;
using System.Linq;
using System.Web;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.Context;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.Sample.CommonService.CustomMessageHandler;
using WeChat;
using WeiPay;
using System.Text;

namespace WeChat.Controllers
{
    public partial class CustomMessageHandler : MessageHandler<MessageContext<IRequestMessageBase, IResponseMessageBase>>
    {
        private Stream inputStream;
        private PostModel postModel;

        public CustomMessageHandler(Stream inputStream, PostModel postModel) : base(inputStream, postModel)
        {
            this.inputStream = inputStream;
            this.postModel = postModel;
        }

        internal new void Execute()
        {
            string postXML = string.Empty;
            using (inputStream)
            {
                Byte[] postByte = new Byte[inputStream.Length];
                inputStream.Read(postByte, 0, (Int32)inputStream.Length);
                postXML = Encoding.UTF8.GetString(postByte);
                HandleMsg(postXML);
            }
            var hm = new YummyOnlineManager();
            hm.Log("Execute",postXML);
        }

        private void HandleMsg(string postXML)
        {
            throw new NotImplementedException();
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

        public override void OnExecuting()
        {
            if (CurrentMessageContext.StorageData == null)
            {
                CurrentMessageContext.StorageData = 0;
            }
            base.OnExecuting();
        }

        public override void OnExecuted()
        {
            base.OnExecuted();
            CurrentMessageContext.StorageData = ((int)CurrentMessageContext.StorageData + 1);
        }
    }
}