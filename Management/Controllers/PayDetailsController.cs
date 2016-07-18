using HotelDAO.Models;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;
using System;
using System.Data.Entity.SqlServer;
using System.Collections.Generic;

namespace Management.Controllers
{
    public class PayDetailsController : BaseController
    {
        // GET: PayDetails
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetPayDetails (DateTime begintime, DateTime endtime, List<int> payKindIds)
        {
            endtime = endtime.AddDays(1);
            var linq = db.DinePaidDetails.Include(p => p.Dine).Include(p => p.PayKind).Where(t => t.Dine.BeginTime <= endtime && begintime <= t.Dine.BeginTime);
            if (payKindIds != null && payKindIds.Count > 0)
            {
                linq = linq.Where(x => payKindIds.Contains(x.PayKindId));

            }
            var total = await linq.ToListAsync();
            if (total.Count() == 0)
            {
                return Json(new { succeeded = false });
            }
            decimal oriprice = 0;
            decimal price = 0;
           
            for (int i = 0; i < total.Count(); i++)
            {
                
                oriprice = oriprice + total[i].Dine.OriPrice;
                price = price + total[i].Dine.Price;
                
                
            }
            var StaffInfo = await db.Staffs.Select(p => new
            {
                p.Id,
                p.Name
            }).ToListAsync();
            return Json(new { aaa = total,  oriprice = oriprice, price = price,bbb= StaffInfo });

        }

        public async Task<JsonResult> GetPayKindName()
        {
            var linq = await db.PayKinds
                .Select(p => new
                {
                    p.Id,
                    p.Name
                })
                .ToListAsync();
            return Json(linq.ToList());
        }
        //public async Task<JsonResult> GetClerkName(string clerkid)
        //{
        //    var linq = await db.Staffs
        //        .Where(q => q.Id == clerkid)
        //        .Select(p => p.Name).ToListAsync();
        //    return Json(linq.ToList());
        //}

       


    }


  
}