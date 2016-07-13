using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HotelDAO;
using HotelDAO.Models;

namespace WeChat
{
    public class HotelManager : BaseHotelManager    {
        public HotelManager(string connString) : base(connString) { }
    }
}