using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Helpers;
using Senparc.Weixin.Context;
using System.IO;
using Senparc.Weixin.MP;

namespace MessageHandle
{
    public partial class CustomMessageHandler : MessageHandler<MessageContext<IRequestMessageBase, IResponseMessageBase>>
    {
        public CustomMessageHandler(Stream inputStream, int maxRecordCount = 0)
            : base(inputStream, null, maxRecordCount)
        {
            WeixinContext.ExpireMinutes = 3;
        }
        public override void OnExecuting()
        {
            //测试MessageContext.StorageData
            if (CurrentMessageContext.StorageData == null)
            {
                CurrentMessageContext.StorageData = 0;
            }
            base.OnExecuting();
        }
        public override void OnExecuted()
        {
            base.OnExecuted();
            CurrentMessageContext.StorageData = ((int)CurrentMessageContext.StorageData) + 1;
        }

        //处理文字请求
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            var strongResposeMessage = CreateResponseMessage<ResponseMessageNews>();
            var result = new StringBuilder();
            if (requestMessage.Content == "1")
            {
                strongResposeMessage.Articles.Add(new Article
                {
                    Title = "店小二系统总图",
                    //Description = "店小二系统总图",
                    PicUrl = "http://static.yummyonline.net/dianxiaoer/yun.jpg",
                    Url = "http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000081&idx=1&sn=2505ac9e23995714b49cca1122ece2bd#rd"
                });
                //result.AppendLine("http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000081&idx=1&sn=2505ac9e23995714b49cca1122ece2bd#rd");
                return strongResposeMessage;
            }
            else if (requestMessage.Content == "2")
            {
                strongResposeMessage.Articles.Add(new Article
                {
                    Title = "店小二版本种类",
                    //Description = "店小二版本种类",
                    PicUrl = "https://mp.weixin.qq.com/cgi-bin/filepage?type=2&begin=0&count=12&t=media/img_list&token=81448731&lang=zh_CN",
                    Url = "http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000083&idx=1&sn=bc0bbf22bba637121f7bedc919003147#rd"
                });
                //result.AppendLine("http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000083&idx=1&sn=bc0bbf22bba637121f7bedc919003147#rd");
                return strongResposeMessage;
            }
            else if (requestMessage.Content == "3")
            {
                strongResposeMessage.Articles.Add(new Article
                {
                    Title = "店小二介绍",
                    //Description="店小二介绍",
                    PicUrl = "http://static.yummyonline.net/dianxiaoer/yun.jpg",
                    Url = "http://www.rabbitpre.com/m/fERuy7QYX"
                });
                //result.AppendLine("http://www.rabbitpre.com/m/fERuy7QYX");
                return strongResposeMessage;
            }
            else if (requestMessage.Content == "4")
            {
                strongResposeMessage.Articles.Add(new Article
                {
                    Title = "店掌柜",
                    //Description="店掌柜",
                    PicUrl = "http://static.yummyonline.net/dianxiaoer/dian.jpg",
                    Url = "http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000086&idx=1&sn=e032e82cff9d642dfcbe1788f9065f80#rd"
                });
                //result.AppendLine("http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000086&idx=1&sn=e032e82cff9d642dfcbe1788f9065f80#rd");
                return strongResposeMessage;
            }
            else if (requestMessage.Content == "5")
            {
                strongResposeMessage.Articles.Add(new Article
                {
                    Title = "云掌柜",
                    //Description="云掌柜",
                    PicUrl = "http://static.yummyonline.net/dianxiaoer/yun.jpg",
                    Url = "http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000088&idx=1&sn=15e9880802e286445eca480ab329d362#rd"
                });
                //result.AppendLine("http://mp.weixin.qq.com/s?__biz=MzIyNzQxOTg1MQ==&mid=100000088&idx=1&sn=15e9880802e286445eca480ab329d362#rd");
                return strongResposeMessage;
            }
            else
            {
                result.AppendLine("请输入数字1-5");
                responseMessage.Content = result.ToString();
                return responseMessage;
            }
        }


        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您好,欢迎关注店小二";
            return responseMessage;
        }
    }
}
