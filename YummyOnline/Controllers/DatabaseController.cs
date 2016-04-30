using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YummyOnline.Utility;
using YummyOnlineDAO.Models;

namespace YummyOnline.Controllers {
	public class DatabaseController : BaseController {
		// GET: Database
		public ActionResult Index() {
			return View();
		}

		public ActionResult _ViewPartitionDetail() {
			return View();
		}
		public ActionResult _ViewPartitionHandle() {
			return View();
		}

		public async Task<JsonResult> GetDbPartitionDetails() {
			var hotels = await YummyOnlineManager.GetHotels();
			List<dynamic> partitionDetails = new List<dynamic>();

			foreach(Hotel hotel in hotels) {
				partitionDetails.Add(new {
					Hotel = new {
						hotel.Id,
						hotel.Name
					},
					DbPartitionInfos = await new DbPartition(hotel.ConnectionString).GetDbPartitionInfos()
				});
			}

			return Json(partitionDetails);
		}

		public async Task<JsonResult> GetDbPartitionDetailByHotelId(int? hotelId) {
			Hotel hotel;
			if(hotelId.HasValue) {
				hotel = await YummyOnlineManager.GetHotelById(hotelId.Value);
			}
			else {
				hotel = await YummyOnlineManager.GetFirstHotel();
			}

			return Json(new {
				Hotel = new {
					hotel.Id,
					hotel.Name
				},
				DbPartitionInfos = await new DbPartition(hotel.ConnectionString).GetDbPartitionInfos()
			});
		}
	}
}