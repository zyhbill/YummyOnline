using HotelDAO.Models;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;
using System;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Core.Objects;

namespace Management.Controllers
{
    public class DailySalesController : BaseController
    {
        // GET: DailySales
        public ActionResult Index()
        {
            return View();
        }


        public async Task<JsonResult> GetDailySales(DateTime begintime, DateTime endtime)
        {
            endtime = endtime.AddDays(1);

            var check = db.Dines.Where(dine => dine.BeginTime <= endtime && begintime <= dine.BeginTime).Count();
            if (check == 0)
            {
                return Json(new { succeeded = false });
            }


            var query =
                 from dine in db.Dines
                 where (dine.BeginTime <= endtime && begintime <= dine.BeginTime)
                 group dine by new { dine.BeginTime.Year, dine.BeginTime.Month, dine.BeginTime.Day } into g
                 select new
                 {
                     begindate = g.Key,
                     sumprice = g.Sum(dine => dine.Price),
                     sumoriprice = g.Sum(dine => dine.OriPrice),
                     sumdiscount = g.Sum(dine => dine.OriPrice) - g.Sum(dine => dine.Price),
                     sumheadcount = g.Sum(dine => dine.HeadCount),
                     cpi = g.Sum(dine => dine.OriPrice) / g.Sum(dine => dine.HeadCount)
                 };
            var total = await query.ToListAsync();
            decimal totalprice = 0;
            decimal totaloriprice = 0;
            decimal totaldiscount = 0;
            int totalheadcount = 0;
            decimal totalcpi = 0;
            for (int i = 0; i < total.Count(); i++)
            {
                totalprice = totalprice + total[i].sumprice;
                totaloriprice = totaloriprice + total[i].sumoriprice;
                totaldiscount = totaldiscount + total[i].sumdiscount;
                totalheadcount = totalheadcount + total[i].sumheadcount;
            }
            totalcpi = totaloriprice / totalheadcount;

            return Json(new { aaa = total, totalprice = totalprice, totaloriprice = totaloriprice, totaldiscount = totaldiscount, totalheadcount = totalheadcount, totalcpi = totalcpi });

        }



    }
}