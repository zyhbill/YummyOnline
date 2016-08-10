using HotelDAO.Models;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;


namespace Management.Controllers
{
    public class MenuSetMealsController : BaseController
    {

        // GET: MenuSetMeals
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetMenuSetMeals()
        {
            var linq = await db.Menus.Where(p=>p.IsSetMeal==true).Select(p => new
            {
                p.Id,
                p.Name,
                p.Usable,
            }).ToListAsync();
            return Json(linq.ToList());
        }


        public async Task<JsonResult> SwitchMenuSetMeals(string MenuSetId)
        {
            Menu menu = await db.Menus.FirstOrDefaultAsync(p => p.Id == MenuSetId && p.IsSetMeal ==true);
            if (menu == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                if(menu.Status == MenuStatus.Normal)
                {
                    menu.Status = MenuStatus.SellOut;
                }
                else
                {
                    menu.Status = MenuStatus.Normal;
                }
                await db.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
        }

        public async Task<JsonResult> AllClose()
        {
            var linq = await db.Menus.Where(p => p.IsSetMeal == true && p.Usable == true).ToListAsync();
            for(int i=0;i<linq.Count();i++)
            {
                linq[i].Status = MenuStatus.SellOut;

            }
            await db.SaveChangesAsync();
            return Json(new { succeeded = true });
        }
        public async Task<JsonResult> AllOpen()
        {
            var linq = await db.Menus.Where(p => p.IsSetMeal == true && p.Usable == false).ToListAsync();
            for (int i = 0; i < linq.Count(); i++)
            {
                linq[i].Status = MenuStatus.Normal;

            }
            await db.SaveChangesAsync();
            return Json(new { succeeded = true });
        }

    }
}