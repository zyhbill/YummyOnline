using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Senparc.Weixin.MP.Entities.Request;
using WeiPay;
using Senparc.Weixin.MessageHandlers;

namespace WeChat.Controllers
{
    using Senparc.Weixin.MP;
    using Senparc.Weixin.MP.MvcExtension;

    public class WeixinController : Controller
    {
        public static readonly string Token = "wechatdianxiaoer";
        public static readonly string EncodingAESKey = "fABUsgNVABIcGe7ceZCIjuLFToRYZeN3nPAldcf0JHe";
        public static readonly string AppId = "wx51c299b84496948d";


        [HttpGet]
        [ActionName("Index")]
        public ActionResult Get(PostModel postModel, string echostr)
        {
            if (CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, Token))
            {
                return Content(echostr);
            }
            else
            {
                return Content(postModel.Signature + CheckSignature.GetSignature(postModel.Timestamp, postModel.Nonce, Token));
            }
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult Post(PostModel postModel)
        {
            if (!CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, Token))
            {
                return Content("参数错误！");
            }
            postModel.Token = Token;
            postModel.EncodingAESKey = EncodingAESKey;
            postModel.AppId = AppId;

            var messageHandler = new CustomMessageHandler(Request.InputStream, postModel);
            messageHandler.Execute();
            return new FixWeixinBugWeixinResult(messageHandler);
        }
    }
}