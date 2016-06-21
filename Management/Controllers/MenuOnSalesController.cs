using HotelDAO.Models;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;


namespace Management.Controllers
{
    public class MenuOnSalesController : BaseController
    {

        // GET: MenuOnSales
        public ActionResult Index()
        {

            return View();
        }

        public async Task<JsonResult> GetMenuOnSales()
        {
            var menuonsales = await db.MenuOnSales.Include(p =>p.Menu).Select(t => new
            {
                t.Id,
                t.Menu.Name,
                t.OnSaleWeek,
                t.Price,
                OriPrice = t.Menu.MenuPrice.Price
                
            }).ToListAsync();
           
            return Json(menuonsales.ToList());
        }
        //ADD:MenuOnSales

        public async Task<JsonResult> AddMenuOnSales(string id, int onsaleweek, decimal? price)
        {
            if (id != null && price != null)
            {
                db.MenuOnSales.Add(new MenuOnSale
                {
                    Id = id,
                    OnSaleWeek = (System.DayOfWeek)onsaleweek,
                    Price = price.Value
                });
                await db.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
            else
            {
                return Json(new { succeeded = false });
            }


        }
        public async Task<JsonResult> SearchById(string id)
        {
            var linq = await db.Menus.FirstOrDefaultAsync(p => p.Id == id);
            return Json(linq);
        }
        //DEL:MenuOnSales

        public async Task<JsonResult> DelMenuOnSales(string id,int onsaleweek)
        {
            
            MenuOnSale menuonsales = await db.MenuOnSales.FirstOrDefaultAsync(p => p.Id == id && (int)p.OnSaleWeek == onsaleweek);
            if (menuonsales == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                db.MenuOnSales.Remove(menuonsales);
                await db.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
        }

        public async Task<JsonResult> AltMenuOnSales(string id, int onsaleweek, decimal price)
        {
            MenuOnSale menuonsales = await db.MenuOnSales.FirstOrDefaultAsync(p => p.Id == id && (int)p.OnSaleWeek == onsaleweek);
            if (menuonsales == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                menuonsales.Price = price;
                await db.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
        }
    }
}