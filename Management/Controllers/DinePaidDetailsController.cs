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
    public class DinePaidDetailsController : BaseController
    {
        // GET: DinePaidDetails
        public ActionResult Index()
        {
            return View();
        }


        public async Task<JsonResult> GetDinePaidDetails(/*int i*/)
        {

            var linq = db.Dines
                .Include(d=>d.Remarks)
                .Include(d=>d.Desk)
                .Include(d=>d.DineMenus)
                .Include(q => q.DinePaidDetails.Select(x=>x.PayKind))
                .Where(p => SqlFunctions.DateDiff("day", p.BeginTime, DateTime.Now) == 0);
            var DineMenus = await db.DineMenus
                .Include(d => d.Dine)
                .Where(p => SqlFunctions.DateDiff("day", p.Dine.BeginTime, DateTime.Now) == 0)
                .ToListAsync();
            var total = await linq.ToListAsync();
            if (total.Count() == 0)
            {
                return Json(new { succeeded = false });
            }
            var temp = total.Select(d => new
            {
                d.Id,
                d.DeskId,
                Area = db.Areas.FirstOrDefault(dd => d.Desk.AreaId == dd.Id),
                Waiter = db.Staffs.FirstOrDefault(dd=>dd.Id==d.WaiterId),
                Clerk = db.Staffs.FirstOrDefault(dd => dd.Id == d.ClerkId),
                d.BeginTime,
                d.OriPrice,
                d.Price,
                d.Remarks,
                d.IsPaid,
                d.Discount,
                d.IsOnline,
                d.HeadCount,
                discount = d.OriPrice - d.Price,
                d.DinePaidDetails,
                ReturnPrice  = DineMenus.Where(dd=>dd.DineId==d.Id&&dd.Status == DineMenuStatus.Returned).Sum(dd=>dd.Price * dd.Count),
                GiftPrice  = DineMenus.Where(dd => dd.DineId == d.Id && dd.Status == DineMenuStatus.Gift).Sum(dd => dd.OriPrice * dd.Count)
            }).ToList();
            var Sum = new
            {
                headcount = temp.Where(t=>t.IsPaid==true).Sum(t => t.HeadCount),
                oriprice = temp.Where(t => t.IsPaid == true).Sum(t => t.OriPrice),
                price = temp.Where(t => t.IsPaid == true).Sum(t => t.Price),
                ReturnPrice = temp.Where(t => t.IsPaid == true).Sum(t => t.ReturnPrice),
                GiftPrice = temp.Where(t => t.IsPaid == true).Sum(t => t.GiftPrice),
                discount = temp.Where(t=>t.IsPaid==true).Sum(t=>t.discount)
            };
            
            return Json(new { aaa = temp, Sum = Sum});

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dineid"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetDineDetails(string dineid)
        {
            var linq = await db.DineMenus
                .Include(p => p.Menu)
                .Where(q => q.DineId == dineid)
                .Select(t => new
                {
                    t.MenuId,
                    t.Status,
                    t.Menu.Name,
                    t.Price,
                    t.OriPrice,
                    t.Count

                }).ToListAsync();
            var gift =  linq.Where(p => p.Status ==DineMenuStatus.Gift).Select(q => new
            {
                q.OriPrice,
                q.Price

            }).ToList();
            decimal Gift = 0;
            decimal Back = 0;
            for (int i = 0; i < gift.Count; i++)
            {
                Gift = Gift + gift[i].OriPrice ;
            }
            var back = linq.Where(p => p.Status == DineMenuStatus.Returned).Select(q => new
            {
                q.OriPrice,
                q.Price

            }).ToList();
            for (int i = 0; i < back.Count; i++)
            {
                Back = Back + back[i].Price;
            }
            return Json(new { aaa=linq,Gift=Gift,Back=Back});
        }

        //public async Task<JsonResult> GetClerkName(string clerkid)
        //{
        //    var linq = await db.Staffs
        //        .Where(q => q.Id == clerkid)
        //        .Select(p => p.Name).ToListAsync();
        //    return Json(linq.ToList());
        //}
        //public async Task<JsonResult> GetWaiterName(string waiterid)
        //{
        //    var linq = await db.Staffs
        //        .Where(q => q.Id == waiterid)
        //        .Select(p => p.Name).ToListAsync();
        //    return Json(linq.ToList());
        //}
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

        public async Task<JsonResult> SearchPayKindName(string dineid)
        {
            var linq = await db.DinePaidDetails
                .Include(p => p.PayKind)
                .Where(q => q.DineId == dineid)
                .Select(s =>new { s.PayKind.Name,s.Price}).ToListAsync();
            return Json(linq.ToList());
        }



        public async Task<JsonResult> GetStaffName()
        {
            var linq = await db.Staffs
                .Select(p => new
                {
                    p.Name,
                    p.Id
                }).ToListAsync();
            return Json(linq.ToList());
        }
        public async Task<JsonResult> SearchDine(DateTime? begindate, DateTime? enddate,DateTime? begintime, DateTime? endtime, string waiterid, List<int> payKindIds)//订单查询
        {
            
            if (begintime.HasValue && begindate.HasValue)
            {
                begintime = new DateTime(begindate.Value.Year, begindate.Value.Month, begindate.Value.Day, begintime.Value.Hour, begintime.Value.Minute, 0);
            }
            if (endtime.HasValue && enddate.HasValue)
            {
                endtime = new DateTime(enddate.Value.Year, enddate.Value.Month, enddate.Value.Day, endtime.Value.Hour, endtime.Value.Minute, 0);
            }


            var Dines = db.Dines
                .Include(d => d.Remarks)
                .Include(d => d.Desk)
                .Include(d => d.DineMenus)
                .Include(q => q.DinePaidDetails.Select(x => x.PayKind))
                .Where(d => d.BeginTime >= begintime && d.BeginTime <= endtime);
            var DineMenus = await db.DineMenus
                .Include(d => d.Dine)
                .Where(p=>p.Dine.BeginTime >= begintime && p.Dine.BeginTime <= endtime).ToListAsync();
            //var total = await Dines.ToListAsync();

            var DineIds = Dines.Select(d => d.Id);

            var Pays = await db.DinePaidDetails
                .Where(d => DineIds.Contains(d.DineId) && payKindIds.Contains(d.PayKindId))
                .Select(p => p.DineId)
                .ToListAsync();
            var total = Dines.Where(d => Pays.Contains(d.Id)).ToList();
            if (waiterid == null || waiterid == "")
            {

            }
            else
            {
                total = total.Where(d => d.WaiterId == waiterid).ToList();
            }

            if (total.Count() == 0)
            {
                return Json(new { succeeded = false });
            }
            var temp = total.Select(d => new
            {
                d.Id,
                d.DeskId,
                Area = db.Areas.FirstOrDefault(dd => d.Desk.AreaId == dd.Id),
                Waiter = db.Staffs.FirstOrDefault(dd => dd.Id == d.WaiterId),
                Clerk = db.Staffs.FirstOrDefault(dd => dd.Id == d.ClerkId),
                d.BeginTime,
                d.OriPrice,
                d.Price,
                d.Remarks,
                d.IsPaid,
                d.Discount,
                d.IsOnline,
                d.HeadCount,
                discount = d.OriPrice - d.Price,
                d.DinePaidDetails,
                ReturnPrice = DineMenus.Where(dd => dd.DineId == d.Id && dd.Status == DineMenuStatus.Returned).Sum(dd => dd.Price * dd.Count),
                GiftPrice = DineMenus.Where(dd => dd.DineId == d.Id && dd.Status == DineMenuStatus.Gift).Sum(dd => dd.OriPrice * dd.Count)
            }).ToList();
            var Sum = new
            {
                headcount = temp.Where(t => t.IsPaid == true).Sum(t => t.HeadCount),
                oriprice = temp.Where(t => t.IsPaid == true).Sum(t => t.OriPrice),
                price = temp.Where(t => t.IsPaid == true).Sum(t => t.Price),
                ReturnPrice = temp.Where(t => t.IsPaid == true).Sum(t => t.ReturnPrice),
                GiftPrice = temp.Where(t => t.IsPaid == true).Sum(t => t.GiftPrice),
                discount = temp.Where(t => t.IsPaid == true).Sum(t => t.discount)
            };
            if (temp.Count() == 0)
            {
                return Json(new { succeeded = false });
            }
            else { 
            return Json(new { aaa = temp, Sum = Sum });
            }


            //var Dines = await db.Dines
            //    .Include(d=>d.DinePaidDetails.Select(dd=>dd.PayKind))
            //    .Where(d => d.BeginTime >= begintime && d.BeginTime <= endtime).ToListAsync();
            //var DineIds = Dines.Select(d => d.Id);

            //var Pays =await  db.DinePaidDetails
            //    .Where(d => DineIds.Contains(d.DineId) && payKindIds.Contains(d.PayKindId))
            //    .Select(p => p.DineId)
            //    .ToListAsync();

            //var total = Dines.Where(d => Pays.Contains(d.Id)).ToList();
            //if (waiterid == null||waiterid=="")
            //{

            //}
            //else
            //{
            //    total = total.Where(d => d.WaiterId == waiterid).ToList();
            //}
            //if (total.Count() == 0)
            //{
            //    return Json(new { succeeded = false });
            //}
            //int headcount = 0;
            //decimal oriprice = 0;
            //decimal price = 0;
            //for (int i = 0; i < total.Count(); i++)
            //{
            //    if (total[i].IsPaid == true)
            //    {
            //        headcount = headcount + total[i].HeadCount;
            //    oriprice = oriprice + total[i].OriPrice;
            //    price = price + total[i].Price;
            //    }
            //}
            //decimal discount = 0;
            //discount = oriprice - price;
            ////decimal Sum = await linq.SumAsync(o => o.Price);
            //return Json(new { aaa = total, headcount = headcount, oriprice = oriprice, price = price, discount = discount });



        }


    }
}