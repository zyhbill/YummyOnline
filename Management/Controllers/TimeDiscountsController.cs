using HotelDAO.Models;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;
using System;

namespace Management.Controllers
{
    public class TimeDiscountsController : BaseController
    {

        // GET: TimeDiscounts
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetTimeDiscounts()
        {
            var timediscounts = await db.TimeDiscounts.Select(t => new
            {
                t.From,
                t.To,
                t.Week,
                Discount = t.Discount * 100,
                t.Name
            }).ToListAsync();

            return Json(timediscounts.ToList());
        }
        //ADD:TimeDiscounts
        public async Task<JsonResult> AddTimeDiscounts(TimeSpan from, TimeSpan to, DayOfWeek? week, double? discount,string name)
        {
            if(from !=null && to != null  &&discount != null && name != null ) { 
            db.TimeDiscounts.Add(new TimeDiscount
            {
                From = from,
                To = to,
                Week = week.Value,
                Discount = discount.Value/100,
                Name = name,
            });
            await db.SaveChangesAsync();
            return Json(new { succeeded = true });
        }
            else
            {
                return Json(new { succeeded = false });
            }


    }
        //DEL:TimeDiscounts
        public async Task<JsonResult> DelTimeDiscounts(TimeSpan from, TimeSpan to, DayOfWeek week)
        {

            TimeDiscount timediscounts = await db.TimeDiscounts.FirstOrDefaultAsync(p => p.From == from && p.To == to && p.Week == week);
            if (timediscounts == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                db.TimeDiscounts.Remove(timediscounts);
                await db.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
        }

        //ALT:Timediscounts
        public async Task<JsonResult> AltTimediscounts(TimeSpan from, TimeSpan to, DayOfWeek week, double discount, string name)
        {

            TimeDiscount timediscounts = await db.TimeDiscounts.FirstOrDefaultAsync(p => p.From == from && p.To == to && p.Week == week);
            if (timediscounts == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                timediscounts.Name = name;
                timediscounts.Discount = discount/100;
                await db.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
        }


    }
}