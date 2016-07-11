using HotelDAO.Models;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;


namespace Management.Controllers
{
    public class PrintersController : Controller
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
        // GET: Printers
        public ActionResult Index()
        {
            return View();
        }
      
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetPrinters()
        {
            var printers = await (from p in db.Printers
                                  select p).ToListAsync();

            return Json(printers.ToList());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ipaddress"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddPrinters(string name/*, string ipaddress*/)
        {

            db.Printers.Add(new Printer
            {
                Name = name,
                //IpAddress = ipaddress,
                Usable = true,

            });
            await db.SaveChangesAsync();
            return Json(new { succeeded = true });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<JsonResult> SwitchPrinters(int id)
        {

            Printer printers = await db.Printers.FirstOrDefaultAsync(p => p.Id == id);
            if (printers == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                printers.Usable = !printers.Usable;
                await db.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="ipaddress"></param>
        /// <returns></returns>
        public async Task<JsonResult> AltPrinters(int id, string name, string ipaddress)
        {

            Printer printers = await db.Printers.FirstOrDefaultAsync(p => p.Id == id);
            if (printers == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                printers.Name = name;
                printers.IpAddress = ipaddress;
                await db.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
        }
        /// <summary>
        /// 删除打印机
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<JsonResult> DeletePrinters(int id)
        {

            Printer printers = await db.Printers.FirstOrDefaultAsync(p => p.Id == id);
            if (printers == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                db.Printers.Remove(printers);
                db.SaveChanges();
                return Json(new { succeeded = true });
            }
        }


    }
}