using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.Entity;
using Management.Controllers;
using HotelDAO.Models;
using Newtonsoft.Json;
using YummyOnlineDAO.Models;
namespace Management.Models
{
    public class WebSocket:BaseController
    {
        WebSocketServer server;
        List<IWebSocketConnection> allSockets;
        public WebSocket()
        {
             allSockets = new List<IWebSocketConnection>();
             server = new WebSocketServer(WebSocketUrl);
        }
        public async void start()
        {
            await Task.Run(() => server.Start(socket =>
             {
                 socket.OnOpen = () =>
                 {
                     var logs = new YummyOnlineDAO.Models.Log();
                     logs.DateTime = DateTime.Now;
                     logs.Level = YummyOnlineDAO.Models.Log.LogLevel.Success;
                     logs.Program = YummyOnlineDAO.Models.Log.LogProgram.Manager;
                     logs.Message = $"{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort} WebScoket Open";
                     sysdb.Logs.Add(logs);
                     sysdb.SaveChanges();
                     Console.WriteLine("Open!");
                     allSockets.Add(socket);
                 };
                 socket.OnClose = () =>
                 {
                     var logs = new YummyOnlineDAO.Models.Log();
                     logs.DateTime = DateTime.Now;
                     logs.Level = YummyOnlineDAO.Models.Log.LogLevel.Warning;
                     logs.Program = YummyOnlineDAO.Models.Log.LogProgram.Manager;
                     logs.Message = $"{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort} WebScoket Close";
                     sysdb.Logs.Add(logs);
                     sysdb.SaveChanges();
                     Console.WriteLine("Close!");
                    
                     allSockets.Remove(socket);
                 };
                 socket.OnMessage = message =>
                 {

                 };
             }));
        }
        protected async Task<string> AsyncChange(int HotelId, string cg)
        {
            JsonSerializerSettings setting = new  JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string ConnectingStr;
            using (var db = new YummyOnlineContext())
            {
                ConnectingStr = db.Hotels.Where(h => h.Id == HotelId).Select(h => h.ConnectionString).FirstOrDefault();
            }
            using (var db = new HotelContext(ConnectingStr))
            {
                if (cg == "desk")
                {
                    var data = await db.Desks.Where(d => d.Usable == true)
                        .Select(d => new
                    {
                        d.AreaId,
                        d.Name,
                        d.Status,
                        d.Id,
                        d.Order
                    }).OrderBy(d => d.Order).ToListAsync();
                    return JsonConvert.SerializeObject(new {HotelId=HotelId,change="desk",data=data },setting);
                }
                else if (cg == "dine")
                {
                    var Orders = await db.Dines
                    .Include(p => p.DineMenus.Select(pp => pp.Remarks))
                    .Include(p => p.DineMenus.Select(pp => pp.Menu.MenuPrice))
                    .Where(order => order.IsPaid == false && order.IsOnline == false)
                    .Select(d => new {
                        d.Discount,
                        d.DiscountName,
                        d.Id,
                        DineMenus = d.DineMenus.Select(dd => new
                        {
                            Count = dd.Count,
                            DineId = dd.DineId,
                            Id = dd.Id,
                            Menu = dd.Menu,
                            MenuId = dd.MenuId,
                            OriPrice = dd.OriPrice,
                            Price = dd.Price,
                            RemarkPrice = dd.RemarkPrice,
                            Remarks = dd.Remarks,
                            ReturnedWaiterId = dd.ReturnedWaiterId,
                            Status = dd.Status
                        }),
                        Menu = d.DineMenus.Select(dd => new { dd.Menu, dd.Menu.MenuPrice }),
                        d.BeginTime,
                        d.DeskId,
                        d.Remarks,
                        d.HeadCount,
                        d.UserId,
                        d.OriPrice,
                        d.Price
                    })
                    .ToListAsync();
                    return JsonConvert.SerializeObject(new {HotelId=HotelId, change = "dine", data = Orders }, setting);
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task SendToClient(int HotelId,string change)
        {
            string JsonData = await AsyncChange(HotelId, change);
            byte[] buffer = Encoding.UTF8.GetBytes(JsonData);
            string msg = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            allSockets.ToList().ForEach(s => s.Send(msg));
        }

    }
}