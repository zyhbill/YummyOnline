using Protocal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using YummyOnline.Utility;
using YummyOnlineDAO.Models;

namespace YummyOnline.Controllers {
	[Authorize(Roles = nameof(Role.Admin))]
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

			partitionDetails.Add(new {
				DbPartitionInfos = await new OriginSql(getYummyOnlineConntionString()).GetDbPartitionInfos()
			});

			foreach(Hotel h in hotels) {
				if(h.ConnectionString == null)
					continue;
				partitionDetails.Add(new {
					Hotel = new {
						h.Id,
						h.Name
					},
					DbPartitionInfos = await new OriginSql(h.AdminConnectionString).GetDbPartitionInfos()
				});
			}

			return Json(partitionDetails);
		}

		public async Task<JsonResult> GetDbPartitionDetailByHotelId(int? hotelId) {
			Hotel hotel;
			OriginSql dbPartition;
			if(hotelId.HasValue) {
				hotel = await YummyOnlineManager.GetHotelById(hotelId.Value);
				dbPartition = new OriginSql(hotel.AdminConnectionString);
			}
			else {
				hotel = null;
				dbPartition = new OriginSql(getYummyOnlineConntionString());
			}

			return Json(new {
				Hotel = hotel == null ? null : new {
					hotel.Id,
					hotel.Name
				},
				DbPartitionInfos = await dbPartition.GetDbPartitionInfos(),
				FileGroupInfos = await dbPartition.GetFileGroupInfos()
			});
		}

		public async Task<JsonResult> CreateDbPartition(int? hotelId) {
			FunctionResult result;
			if(hotelId.HasValue) {
				Hotel hotel = await YummyOnlineManager.GetHotelById(hotelId.Value);
				result = await new OriginSql(hotel.AdminConnectionString).CreateHotelPartition();
			}
			else {
				result = await new OriginSql(getYummyOnlineConntionString()).CreateYummyOnlinePartition();
			}

			if(!result.Succeeded) {
				await logPartition(hotelId, nameof(CreateDbPartition), result.Message);
				return Json(new JsonError(result.Message));
			}
			await logPartition(hotelId, nameof(CreateDbPartition));
			return Json(new JsonSuccess());
		}

		public async Task<JsonResult> SplitPartition(int? hotelId, DateTime dateTime) {
			FunctionResult result;
			if(hotelId.HasValue) {
				Hotel hotel = await YummyOnlineManager.GetHotelById(hotelId.Value);
				result = await new OriginSql(hotel.AdminConnectionString).Split(dateTime);
			}
			else {
				result = await new OriginSql(getYummyOnlineConntionString()).Split(dateTime);
			}

			if(!result.Succeeded) {
				await logPartition(hotelId, nameof(SplitPartition), result.Message);
				return Json(new JsonError(result.Message));
			}
			await logPartition(hotelId, nameof(SplitPartition));
			return Json(new JsonSuccess());
		}
		public async Task<JsonResult> MergePartition(int? hotelId, DateTime dateTime) {
			FunctionResult result;
			if(hotelId.HasValue) {
				Hotel hotel = await YummyOnlineManager.GetHotelById(hotelId.Value);
				result = await new OriginSql(hotel.AdminConnectionString).Merge(dateTime);
			}
			else {
				result = await new OriginSql(getYummyOnlineConntionString()).Merge(dateTime);
			}

			if(!result.Succeeded) {
				await logPartition(hotelId, nameof(MergePartition), result.Message);
				return Json(new JsonError(result.Message));
			}
			await logPartition(hotelId, nameof(MergePartition));
			return Json(new JsonSuccess());
		}

		private string getYummyOnlineConntionString() {
			return new YummyOnlineContext().Database.Connection.ConnectionString;
		}

		private async Task logPartition(int? hotelId, string method) {
			if(hotelId.HasValue) {
				Hotel hotel = await YummyOnlineManager.GetHotelById(hotelId.Value);
				await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Success, $"{method} On {hotel.Name} ({hotel.Id}) Succeeded");
			}
			else {
				await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Success, $"{method} On YummyOnlineDB Succeeded");
			}
		}

		private async Task logPartition(int? hotelId, string method, string errorMessage) {
			if(hotelId.HasValue) {
				Hotel hotel = await YummyOnlineManager.GetHotelById(hotelId.Value);
				await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Error, $"{method} On {hotel.Name} ({hotel.Id}) Failed, {errorMessage}");
			}
			else {
				await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Error, $"{method} On YummyOnlineDB Failed, {errorMessage}");
			}
		}
	}
}