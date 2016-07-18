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
    }
}