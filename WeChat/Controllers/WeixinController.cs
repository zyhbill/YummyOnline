using System.Web.Mvc;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.MvcExtension;
using MessageHandle;

namespace WeChat.Controllers
{
    public class WeixinController : Controller
    {
    

        [HttpGet]
        [ActionName("Index")]
        public ActionResult Get(PostModel postModel, string echostr)
        {
            if (CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, AppConfig.Token))
            {
                return Content(echostr);
            }
            else
            {
                return Content(postModel.Signature + CheckSignature.GetSignature(postModel.Timestamp, postModel.Nonce, AppConfig.Token));
            }
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult Post(PostModel postModel)
        {
            if (!CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, AppConfig.Token))
            {
                return Content("参数错误！");
            }
            postModel.Token = AppConfig.Token;
            postModel.EncodingAESKey = AppConfig.EncodingAESKey;
            postModel.AppId = AppConfig.AppId;
            
            var messageHandler = new CustomMessageHandler(Request.InputStream, 1);
            messageHandler.Execute();
            return new FixWeixinBugWeixinResult(messageHandler);
        }
    }
}