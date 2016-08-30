using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeChat
{
    public class AppConfig
    {
        public static readonly string Token = "wechatdianxiaoer";
        public static readonly string EncodingAESKey = "fABUsgNVABIcGe7ceZCIjuLFToRYZeN3nPAldcf0JHe";
        public static readonly string AppId = "wx51c299b84496948d";
        public static readonly string AppSecret = "0ccbaa0c5332170b53d91bdd4be26003";
        //通知支付成功的模板消息
        public static readonly string TemplateIdForResponse = "ZdG4C7FzuHV_NiZ57al_XweuFHAvfMWhpqVmDhqA5wI";
        public static readonly string TemplateResponseUrl = "http://ordersystem.yummyonline.net";
    }
}