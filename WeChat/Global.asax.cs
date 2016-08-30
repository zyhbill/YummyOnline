
using Protocol;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;
using HotelDAO;
using HotelDAO.Models;

namespace WeChat
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //增加接受订单消息tcpClient

            var _ = WeChatTcpInit.Initialize(async () =>
            {

                await new YummyOnlineManager().RecordLog(YummyOnlineDAO.Models.Log.LogProgram.WeChat, YummyOnlineDAO.Models.Log.LogLevel.Success, "WeChat TcpServer Connected");
            }, (t, p) =>
            {
                //此为收到的订单
                if (t != Protocol.TcpProtocolType.NewDineInform)
                    return;
                NewDineInformProtocol protocal = (NewDineInformProtocol)p;
                //1.查询订单
                //2.发送模板消息

            });
        }

        protected async Task queryDine(NewDineInformProtocol p)
        {
            YummyOnlineManager manager = new YummyOnlineManager();
            string connStr = (await manager.GetHotelById(p.HotelId)).ConnectionString;
            HotelManager hotelManager = new HotelManager(connStr);
            var dine = await hotelManager.GetFormatedDineById(p.DineId);
            string openId = dine.WeChatOpenId;
            decimal price =  dine.Price;
        }

        protected void sendWeChatMsg()
        {
            
        }
    }
}
