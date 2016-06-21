using HotelDAO.Models;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;
using YummyOnlineDAO.Models;
using System;
using Management.ObjectClasses;

namespace Management.Controllers
{
    public class HotelsController : BaseController
    {
        //private HotelContext _db = null;
        //private HotelContext db
        //{
        //    get
        //    {
        //        if (_db == null)
        //        {
        //            _db = new HotelContext(Session["ConnectString"] as string);
        //        }
        //        return _db;
        //    }
        //}

        private YummyOnlineContext _yummyOnlineCtx = null;
        private YummyOnlineContext yummyOnlineCtx
        {
            get
            {
                if(_yummyOnlineCtx == null)
                {
                    _yummyOnlineCtx = new YummyOnlineContext();
                }
                return _yummyOnlineCtx;
            }
        }

        // GET: Hotels
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetHotels(string connectionstring)
        {
            int HotelId = (int)(Session["User"] as RStatus).HotelId;

            var hotels = await (from p in yummyOnlineCtx.Hotels where p.Id == HotelId
                                  select p).ToListAsync();

            return Json(hotels.ToList());
        }

        //ALT:Hotels

        public async Task<JsonResult> AltHotels(int id,string name, string address,string tel,DateTime createdate,TimeSpan opentime,TimeSpan closetime)
        {

            Hotel hotels = await yummyOnlineCtx.Hotels.FirstOrDefaultAsync(p => p.Id == id);
            if (hotels == null)
            {
                return Json(new { succeeded = false });
            }
            else
            {
                hotels.Name = name;
                hotels.Address = address;
                hotels.Tel = tel;
                hotels.CreateDate = createdate;
                hotels.OpenTime = opentime;
                hotels.CloseTime = closetime;
                await yummyOnlineCtx.SaveChangesAsync();
                return Json(new { succeeded = true });
            }
        }


    }
}