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
                d.IsInvoiced,
                d.HeadCount,
                discount = d.OriPrice - d.Price,
                d.DinePaidDetails,
                ReturnPrice  = DineMenus.Where(dd=>dd.DineId==d.Id&&dd.Status == DineMenuStatus.Returned).Sum(dd=>dd.Price * dd.Count),
                GiftPrice  = DineMenus.Where(dd => dd.DineId == d.Id && dd.Type == DineMenuType.Gift).Sum(dd => dd.OriPrice * dd.Count)
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
					t.Type,
                    t.Status,
                    t.Menu.Name,
                    t.Price,
                    t.OriPrice,
                    t.Count

                }).ToListAsync();
            var gift =  linq.Where(p => p.Type ==DineMenuType.Gift).Select(q => new
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
        public async Task<JsonResult> SearchDine(string begindate, string enddate,DateTime? begintime, DateTime? endtime, string waiterid, List<int> payKindIds,int  type)//订单查询
        {

            DateTime BeginTime;
            DateTime EndTime;
            DateTime Begin;
            DateTime End;
            if (begindate == null)
            {
                BeginTime = DateTime.Now;
            }
            else
            {
                BeginTime = Convert.ToDateTime(begindate);
            }
            if (enddate == null)
            {
                EndTime = DateTime.Now;
            }
            else
            {
                EndTime = Convert.ToDateTime(enddate);
            }

            if (begintime.HasValue)
            {
                Begin = new DateTime(BeginTime.Year, BeginTime.Month, BeginTime.Day, begintime.Value.Hour, begintime.Value.Minute, begintime.Value.Second);
            }
            else
            {
                Begin = new DateTime(BeginTime.Year, BeginTime.Month, BeginTime.Day, 0, 0, 0);
            }
            if (endtime.HasValue)
            {
                End = new DateTime(EndTime.Year,EndTime.Month, EndTime.Day, endtime.Value.Hour, endtime.Value.Minute, endtime.Value.Second);
            }
            else
            {
                End = new DateTime(EndTime.Year, EndTime.Month, EndTime.Day, 23 , 59, 59);
            }
            var DineMenus = await db.DineMenus
                   .Include(d => d.Dine)
                   .Where(p => p.Dine.BeginTime >= Begin && p.Dine.BeginTime <= End)
                   .ToListAsync();
            if (type == 1)
            {
                var Unpaid = await db.Dines
                .Include(d => d.Remarks)
                .Include(d => d.Desk)
                .Include(d => d.DineMenus)
                .Include(q => q.DinePaidDetails.Select(x => x.PayKind))
                .Where(d => (d.BeginTime >= Begin && d.BeginTime <= End) && d.IsPaid == false)
                .ToListAsync();

                if (Unpaid.Count == 0) return Json(new { succeeded = false });
                else
                {
                    var flag = Unpaid.Select(d => new
                    {
                        d.Id,
                        d.DeskId,
                        Area = db.Areas.Select(dd => new { Id = dd.Id, Name = dd.Name }).FirstOrDefault(dd => d.Desk.AreaId == dd.Id),
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
                        d.IsInvoiced,
                        ReturnPrice = DineMenus.Where(dd => dd.DineId == d.Id && dd.Status == DineMenuStatus.Returned).Sum(dd => dd.Price * dd.Count),
                        GiftPrice = DineMenus.Where(dd => dd.DineId == d.Id && dd.Type == DineMenuType.Gift).Sum(dd => dd.OriPrice * dd.Count)
                    }).ToList();

                    var UnSum = new
                    {
                        headcount = flag.Where(t => t.IsPaid == true).Sum(t => t.HeadCount),
                        oriprice = flag.Where(t => t.IsPaid == true).Sum(t => t.OriPrice),
                        price = flag.Where(t => t.IsPaid == true).Sum(t => t.Price),
                        ReturnPrice = flag.Where(t => t.IsPaid == true).Sum(t => t.ReturnPrice),
                        GiftPrice = flag.Where(t => t.IsPaid == true).Sum(t => t.GiftPrice),
                        discount = flag.Where(t => t.IsPaid == true).Sum(t => t.discount)
                    };
                    return Json(new { aaa = flag, Sum = UnSum });

                }
            }
            else
            {
                if (payKindIds == null) return Json(new { succeeded = false });
                var Dines = db.Dines
                    .Include(d => d.Remarks)
                    .Include(d => d.Desk)
                    .Include(d => d.DineMenus)
                    .Include(q => q.DinePaidDetails.Select(x => x.PayKind))
                    .Where(d => d.BeginTime >= Begin && d.BeginTime <= End);
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
                    Area = db.Areas.Select(dd => new { Id = dd.Id, Name = dd.Name }).FirstOrDefault(dd => d.Desk.AreaId == dd.Id),
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
                    d.IsInvoiced,
                    ReturnPrice = DineMenus.Where(dd => dd.DineId == d.Id && dd.Status == DineMenuStatus.Returned).Sum(dd => dd.Price * dd.Count),
                    GiftPrice = DineMenus.Where(dd => dd.DineId == d.Id && dd.Type == DineMenuType.Gift).Sum(dd => dd.OriPrice * dd.Count)
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
                else
                {
                    return Json(new { aaa = temp, Sum = Sum });
                }
            }
        }
        public async Task<JsonResult> putInvoice(string Id, string Invoice,decimal Price)
        {
            var Dine = await db.Dines.Where(d => d.Id == Id).FirstOrDefaultAsync();
            Dine.IsInvoiced = true;
            db.Invoices.Add(new Invoice
            {
                DineId = Id,
                Title = Invoice,
                Price = Price,
            });
            db.SaveChanges();
            return null;
        }

    }
}