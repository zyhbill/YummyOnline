using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;

namespace WeChat
{
    public class YummyOnlineManager:YummyOnlineDAO.YummyOnlineManager
    {
        public List<int> GetHotelIds()
        {
           return ctx.Hotels.Select(p => p.Id).ToList();
        }

        public void Log(string Message,string Detail)
        {
            ctx.Logs.Add(new YummyOnlineDAO.Models.Log
            {
                DateTime = DateTime.Now,
                Message = Message,
                Level = YummyOnlineDAO.Models.Log.LogLevel.Info,
                Detail= Detail,
                Program = YummyOnlineDAO.Models.Log.LogProgram.
            });
            ctx.SaveChanges();
        }
    }
}