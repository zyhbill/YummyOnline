using HotelDAO.Models;
using Management.Models;
using Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using YummyOnlineDAO.Models;
using YummyOnlineTcpClient;

namespace Management
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static WebSocket ws = new WebSocket();
        public static TcpClient client = null;
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            ws.start();
            string guid = ConfigurationManager.AppSettings["guid"];
            var db = new YummyOnlineContext();
            var tcp = db.SystemConfigs.Select(sys => new { sys.TcpServerIp, sys.TcpServerPort }).ToList();
            client = new TcpClient(
               IPAddress.Parse(tcp.FirstOrDefault()?.TcpServerIp),
               tcp.FirstOrDefault().TcpServerPort,
               new NewDineInformClientConnectProtocol(guid)
           );
            client.CallBackWhenMessageReceived = async (t, p) =>
            {
                if (t != TcpProtocolType.NewDineInform)
                {
                    return;
                }
                NewDineInformProtocol protocol = (NewDineInformProtocol)p;
                string Cstr = await (db.Hotels.Where(h => h.Id == protocol.HotelId).Select(h => h.ConnectionString)).FirstOrDefaultAsync();
                var hotel = new HotelContext(Cstr);
                var temp = await hotel.Dines.Where(dine => dine.Id == protocol.DineId).Select(dine => new { dine.DeskId, dine.IsOnline }).FirstOrDefaultAsync();
                if (!temp.IsOnline)
                {
                    var desk = hotel.Desks.FirstOrDefault(d => d.Id == temp.DeskId);
                    desk.Status = DeskStatus.Used;
                    var clean = await hotel.Dines.Where(d => d.DeskId == desk.Id && d.IsOnline == false && d.IsPaid == false).ToListAsync();
                    if (clean.Count() == 0)
                    {
                        desk.Status = DeskStatus.StandBy;
                    }
                    hotel.SaveChanges();
                    await ws.SendToClient(protocol.HotelId, "desk");
                    await ws.SendToClient(protocol.HotelId, "dine");
                }
            };

            client.CallBackWhenConnected = () =>
            {
                Console.WriteLine("Connected");
            };

            client.CallBackWhenExceptionOccured = u =>
            {
                Console.WriteLine(u);
            };

            client.Start();

            var url = db.SystemConfigs.FirstOrDefault()==null?"127.0.0.1": db.SystemConfigs.FirstOrDefault().ManagementUrl;
            var ordUrl  = db.SystemConfigs.FirstOrDefault() == null ? "http://ordersystem.yummyonline.net" : db.SystemConfigs.FirstOrDefault().OrderSystemUrl;
            System.Timers.Timer timer = new System.Timers.Timer(1000*60*10);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(async(a,b)=> {
                await Method.postHttp(url, new { });
                await Method.postHttp(ordUrl, new{ });
            });
            //到达时间的时候执行事件；   
            timer.AutoReset = true;
            //设置是执行一次（false）还是一直执行(true)；   
            timer.Enabled = true;
            //是否执行System.Timers.Timer.Elapsed事件；   
            timer.Start();

            
        }
        protected void Application_End()
        {
            var logs = new YummyOnlineDAO.Models.Log();
            logs.Program = YummyOnlineDAO.Models.Log.LogProgram.Manager;
            logs.Level = YummyOnlineDAO.Models.Log.LogLevel.Error;
            logs.DateTime = DateTime.Now;
            logs.Message = $"Thread was killed";
            var db = new YummyOnlineContext();
            db.Logs.Add(logs);
            db.SaveChanges(); 
        }
    }
}
