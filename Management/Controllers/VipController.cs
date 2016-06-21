using HotelDAO.Models;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;
using System;

namespace Management.Controllers
{
    public class VipController : BaseController
    {

        // GET: Vip
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetVip()
        {
            var vipdiscounts =
                from v2 in db.VipDiscounts
                from v1 in db.VipLevels
                where v1.Id == v2.Id
                select new
                {
                    discountname = v2.Name,
                    v1.Id,
                    discount = v2.Discount * 100,
                    levelname = v1.Name
                };
            return Json(await vipdiscounts.ToListAsync());

        }
        //ADD:Vip
        public async Task<JsonResult> AddVip(string levelname,double? discount,string discountname)
        {
            if(levelname!=null && discount != null&&discountname != null) { 
            var vip = new VipLevel
            {

                Name = levelname,
            };
            db.VipLevels.Add(vip);
            await db.SaveChangesAsync();

            db.VipDiscounts.Add(new VipDiscount
            {
                Id = vip.Id,
                Discount = discount.Value/100,
                Name= discountname
            });
            await db.SaveChangesAsync();
            return Json(new { succeeded = true });
            }
            else {
                return Json(new { succeeded = false });
            }
        }

        //}
        //DEL:Vip
        public async Task<JsonResult> DelVip(int id)
        {

            VipLevel viplevels = await db.VipLevels.FirstOrDefaultAsync(p => p.Id == id);
            if (viplevels == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                db.VipLevels.Remove(viplevels);
                await db.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
        }

        //ALT:Vip
        public async Task<JsonResult> AltVip(int id, string levelname, double discount, string discountname)
        {

            VipLevel viplevels = await db.VipLevels.Include(p => p.VipDiscount).FirstOrDefaultAsync(p => p.Id == id);
            if (viplevels == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                viplevels.Name = levelname;
                viplevels.VipDiscount.Discount = discount / 100;
                viplevels.VipDiscount.Name = discountname;
                await db.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
        }


    }
}