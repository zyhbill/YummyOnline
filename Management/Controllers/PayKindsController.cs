using HotelDAO.Models;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;


namespace Management.Controllers
{
    public class PayKindsController : Controller
    {
        private HotelContext _db = null;
        private HotelContext db { get
            {
                if(_db == null)
                {
                    _db = new HotelContext(Session["ConnectString"] as string);
                }
                return _db;
            }
        }
        // GET: PayKinds
        public ActionResult Index()
        {
           
            //var paykinds = await(from p in db.PayKinds
            //                     select p).ToListAsync();
            //ViewBag.PayKinds = paykinds;
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetPayKinds()
        {
            var paykinds = await db.PayKinds.Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.Usable,
                Discount = t.Discount * 100
            }).ToListAsync();
            return Json(paykinds.ToList());
        }
        //ADD:PayKinds
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="discount"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddPayKinds(string name, string description, double? discount)
        {
            if(name !=null && discount != null) { 
            db.PayKinds.Add(new PayKind
            {
                Name = name,
                Description = description,
                Discount = discount.Value/100,
                Type = PayKindType.Offline,
                Usable = true,
                
            });
            await db.SaveChangesAsync();
            return Json(new { succeeded = true });
            }
            else
            {
                return Json(new { succeeded = false });
            }


        }
        //DEL:PayKinds
        /// <summary>
        /// 切换usable状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<JsonResult> SwitchPayKinds(int id)
        {
            
            PayKind paykinds = await db.PayKinds.FirstOrDefaultAsync(p => p.Id == id);
            if (paykinds == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                paykinds.Usable = !paykinds.Usable;
                await db.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
        }

        //ALT:PayKinds
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="discount"></param>
        /// <returns></returns>
        public async Task<JsonResult> AltPayKinds(int id, string name, string description, double discount)
        {
  
            PayKind paykinds = await db.PayKinds.FirstOrDefaultAsync(p => p.Id == id);
            if (paykinds == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                paykinds.Name = name;
                paykinds.Description = description;
                paykinds.Discount = discount/100;
                await db.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
        }

       
    }
}