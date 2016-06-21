using HotelDAO.Models;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;


namespace Management.Controllers
{
    public class SoldOutController : Controller
    {
        private HotelContext _db = null;
        private HotelContext db
        {
            get
            {
                if (_db == null)
                {
                    _db = new HotelContext(Session["ConnectString"] as string);
                }
                return _db;
            }
        }
        // GET: SoldOut
        public ActionResult Index()
        {
            
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetSoldOut()
        {
            var soldout = await db.Menus.Select(t => new
            {
                t.Id,
                
                t.Name,
                
                t.Status
                
            }).ToListAsync();
            return Json(soldout.ToList());
        }


        public async Task<JsonResult> SwitchSoldOut(string id)
        {
            Menu menus = await db.Menus.FirstOrDefaultAsync(p => p.Id == id);
           
            if (menus == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                menus.Status =1-menus.Status;
                await db.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
        }

    }
}