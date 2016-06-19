using Protocal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

		public ActionResult _ViewBackup() {
			return View();
		}
		public ActionResult _ViewPartitionDetail() {
			return View();
		}
		public ActionResult _ViewPartitionHandle() {
			return View();
		}
		public ActionResult _ViewExecution() {
			return View();
		}

		#region 备份
		public async Task<JsonResult> GetBackups() {
			SystemConfig config = await YummyOnlineManager.GetSystemConfig();
			string path = $"{config.SpecificationDir}\\Database";
			string[] files = Directory.GetFiles(path, "*.bak");

			List<dynamic> fileInfos = new List<dynamic>();
			foreach(string file in files) {
				FileInfo fileInfo = new FileInfo(file);
				fileInfos.Add(new {
					fileInfo.Name,
					fileInfo.LastWriteTime,
					fileInfo.Length
				});
			}

			return Json(fileInfos);
		}
		public async Task<JsonResult> Backup(bool isYummyOnline, List<int> hotelIds) {
			SystemConfig config = await YummyOnlineManager.GetSystemConfig();
			string path = $"{config.SpecificationDir}\\Database";

			StringBuilder sb = new StringBuilder();

			if(isYummyOnline) {
				OriginSql originSql = new OriginSql(YummyOnlineManager.ConnectionString);
				FunctionResult result = await originSql.Backup(path);
				if(!result.Succeeded) {
					sb.Append($"YummyOnlineDB Error, {result.Message}</br>");
					await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Error, "Backup YummyOnlineDB Failed", result.Message);
				}
				else {
					await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Success, "Backup YummyOnlineDB Successfully");
				}
			}

			foreach(int id in hotelIds) {
				Hotel hotel = await YummyOnlineManager.GetHotelById(id);
				OriginSql originSql = new OriginSql(hotel.AdminConnectionString);
				FunctionResult result = await originSql.Backup(path);
				if(!result.Succeeded) {
					sb.Append($"{hotel.Name}({hotel.Id}) Error, {result.Message}</br>");
					await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Error, $"Backup {hotel.Name}({hotel.Id}) Failed", result.Message);
				}
				else {
					await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Success, $"Backup {hotel.Name}({hotel.Id}) Successfully");
				}
			}
			if(sb.Length != 0) {
				return Json(new JsonError(sb.ToString()));
			}
			return Json(new JsonSuccess());
		}
		#endregion

		#region 分区
		public async Task<JsonResult> GetDbPartitionDetails() {
			var hotels = await YummyOnlineManager.GetHotels();
			List<dynamic> partitionDetails = new List<dynamic>();

			partitionDetails.Add(new {
				DbPartitionInfos = await new OriginSql(YummyOnlineManager.ConnectionString).GetDbPartitionInfos()
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
				dbPartition = new OriginSql(YummyOnlineManager.ConnectionString);
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

		[Authorize(Roles = nameof(Role.SuperAdmin))]
		public async Task<JsonResult> CreateDbPartition(int? hotelId) {
			FunctionResult result;
			if(hotelId.HasValue) {
				Hotel hotel = await YummyOnlineManager.GetHotelById(hotelId.Value);
				result = await new OriginSql(hotel.AdminConnectionString).CreateHotelPartition();
			}
			else {
				result = await new OriginSql(YummyOnlineManager.ConnectionString).CreateYummyOnlinePartition();
			}

			if(!result.Succeeded) {
				await logPartition(hotelId, nameof(CreateDbPartition), result.Message);
				return Json(new JsonError(result.Message));
			}
			await logPartition(hotelId, nameof(CreateDbPartition));
			return Json(new JsonSuccess());
		}
		[Authorize(Roles = nameof(Role.SuperAdmin))]
		public async Task<JsonResult> SplitPartition(int? hotelId, DateTime dateTime) {
			FunctionResult result;
			if(hotelId.HasValue) {
				Hotel hotel = await YummyOnlineManager.GetHotelById(hotelId.Value);
				result = await new OriginSql(hotel.AdminConnectionString).Split(dateTime);
			}
			else {
				result = await new OriginSql(YummyOnlineManager.ConnectionString).Split(dateTime);
			}

			if(!result.Succeeded) {
				await logPartition(hotelId, nameof(SplitPartition), result.Message);
				return Json(new JsonError(result.Message));
			}
			await logPartition(hotelId, nameof(SplitPartition));
			return Json(new JsonSuccess());
		}
		[Authorize(Roles = nameof(Role.SuperAdmin))]
		public async Task<JsonResult> MergePartition(int? hotelId, DateTime dateTime) {
			FunctionResult result;
			if(hotelId.HasValue) {
				Hotel hotel = await YummyOnlineManager.GetHotelById(hotelId.Value);
				result = await new OriginSql(hotel.AdminConnectionString).Merge(dateTime);
			}
			else {
				result = await new OriginSql(YummyOnlineManager.ConnectionString).Merge(dateTime);
			}

			if(!result.Succeeded) {
				await logPartition(hotelId, nameof(MergePartition), result.Message);
				return Json(new JsonError(result.Message));
			}
			await logPartition(hotelId, nameof(MergePartition));
			return Json(new JsonSuccess());
		}

		private async Task logPartition(int? hotelId, string method) {
			if(hotelId.HasValue) {
				Hotel hotel = await YummyOnlineManager.GetHotelById(hotelId.Value);
				await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Success, $"{method} On {hotel.Name} ({hotel.Id}) Successfully");
			}
			else {
				await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Success, $"{method} On YummyOnlineDB Successfully");
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
		#endregion

		[Authorize(Roles = nameof(Role.SuperAdmin))]
		public async Task<JsonResult> ExecuteSql(List<int> hotelIds, string sql) {
			StringBuilder sb = new StringBuilder();
			foreach(int id in hotelIds) {
				Hotel hotel = await YummyOnlineManager.GetHotelById(id);
				OriginSql originSql = new OriginSql(hotel.AdminConnectionString);
				FunctionResult result = await originSql.ExecuteSql(sql);
				if(!result.Succeeded) {
					sb.Append($"{hotel.Name}({hotel.Id}) Error, {result.Message}</br>");
					await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Error, $"Execute SQL {hotel.Name}({hotel.Id}) Failed",
						$"Error: {result.Message}, SQL: {sql}");
				}
				else {
					await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Success, $"Execute SQL {hotel.Name}({hotel.Id}) Successfully", sql);
				}
			}
			if(sb.Length != 0) {
				return Json(new JsonError(sb.ToString()));
			}
			return Json(new JsonSuccess());
		}
	}
}