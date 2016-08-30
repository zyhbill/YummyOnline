using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using YummyOnlineTcpClient;
//此类用于接收订单消息
namespace WeChat
{
    public class WeChatTcpInit
    {
        private static TcpClient client;
        public async static Task Initialize(Action callBackWhenConnected, Action<string,object> CallBackWhenMessageReceived)
        {
            YummyOnlineDAO.Models.SystemConfig config = await new YummyOnlineManager().GetSystemConfig();
            string guid = System.Configuration.ConfigurationManager.AppSettings["NewDineInformClientGuid"];
            client = new TcpClient(System.Net.IPAddress.Parse(config.TcpServerIp), config.TcpServerPort, new Protocol.NewDineInformClientConnectProtocol(guid));

            client.CallBackWhenConnected = () => {
                callBackWhenConnected();
            };

            client.CallBackWhenMessageReceived = (t,p) => {
                CallBackWhenMessageReceived( t ,p );
            };

            client.Start();
        }
      
    }
}