using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnline.Utility;
using YummyOnlineDAO.Models;

namespace YummyOnline.Controllers {
	[Authorize(Roles = nameof(Role.Admin))]
	public class HotelController : BaseController {
		// GET: Hotel
		public ActionResult Index() {
			return View();
		}
		public ActionResult _ViewHotel() {
			return View();
		}
		public ActionResult _ViewDine() {
			return View();
		}

		public async Task<JsonResult> GetHotelNames() {
			List<Hotel> hotels = await YummyOnlineManager.GetHotels();
			return Json(hotels.Select(p => new {
				p.Id,
				p.Name,
			}));
		}
		public async Task<JsonResult> GetHotels() {
			List<Hotel> hotels = await YummyOnlineManager.GetHotels();
			hotels.AddRange(await YummyOnlineManager.GetHotelReadyForConfirms());

			if(User.IsInRole(nameof(Role.SuperAdmin))) {
				return Json(hotels.Select(p => new {
					p.Id,
					p.Name,
					p.ConnectionString,
					p.AdminConnectionString,
					p.CssThemePath,
					p.CreateDate,
					p.Tel,
					p.Address,
					p.OpenTime,
					p.CloseTime,
					p.Usable
				}));
			}
			return Json(hotels.Select(p => new {
				p.Id,
				p.Name,
				p.ConnectionString,
				p.CssThemePath,
				p.CreateDate,
				p.Tel,
				p.Address,
				p.OpenTime,
				p.CloseTime,
				p.Usable
			}));
		}
		[Authorize(Roles = nameof(Role.SuperAdmin))]
		public async Task<JsonResult> UpdateHotel(Hotel hotel) {
			await YummyOnlineManager.UpdateHotel(hotel);
			await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Warning, $"Hotel {hotel.Id} Updated");
			return Json(new JsonSuccess());
		}
		[Authorize(Roles = nameof(Role.SuperAdmin))]
		public async Task<JsonResult> CreateHotel(int hotelId, string databaseName) {
			// 创建空数据库
			OriginSql originSql = new OriginSql(new YummyOnlineContext().Database.Connection.ConnectionString);
			var result = await originSql.CreateDatabase(databaseName);
			if(!result.Succeeded) {
				return Json(new JsonError(result.Message));
			}
			// 总数据库记录连接字符串等信息
			await YummyOnlineManager.CreateHotel(hotelId, databaseName);

			// 新数据库写入所有表格等架构
			Hotel newHotel = await YummyOnlineManager.GetHotelById(hotelId);
			originSql = new OriginSql(newHotel.AdminConnectionString);
			result = await originSql.InitializeHotel();
			if(!result.Succeeded) {
				return Json(new JsonError(result.Message));
			}

			// 新数据库初始化
			string staffId = await YummyOnlineManager.GetHotelAdminId(hotelId);
			HotelManager hotelManager = new HotelManager(newHotel.AdminConnectionString);
			await hotelManager.InitializeHotel(hotelId, staffId);
			return Json(new JsonSuccess());
		}

		public async Task<JsonResult> GetAllDineIds(int hotelId, DateTime? dateTime) {
			dateTime = dateTime ?? DateTime.Now;

			string connStr = await YummyOnlineManager.GetHotelConnectionStringById(hotelId);

			return Json(await new HotelManager(connStr).GetAllDineIds(dateTime.Value));
		}
		public async Task<JsonResult> GetDines(int hotelId, List<string> dineIds) {
			string connStr = await YummyOnlineManager.GetHotelConnectionStringById(hotelId);

			List<dynamic> dines = new List<dynamic>();
			foreach(string dineId in dineIds) {
				var dine = await new HotelManager(connStr).GetFormatedDineById(dineId);
				if(dine != null)
					dines.Add(dine);
			}

			return Json(dines);
		}
	}
}