using Protocal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;

namespace YummyOnline.Controllers {
	[Authorize(Roles = nameof(Role.Admin))]
	public class LogController : BaseController {
		private DirectoryInfo bin = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

		// GET: Log
		public ActionResult Index() {
			return View();
		}
		public async Task<ActionResult> _ViewSpecification() {
			SystemConfig config = await YummyOnlineManager.GetSystemConfig();
			Dictionary<string, List<FileInfo>> dirFiles = new Dictionary<string, List<FileInfo>>();

			string[] dirs = Directory.GetDirectories(config.SpecificationDir);

			foreach(string dir in dirs) {
				string[] files = Directory.GetFiles(dir);

				string dirStr = dir.Split('\\').Last();
				dirFiles.Add(dirStr, new List<FileInfo>());

				foreach(string file in files) {
					dirFiles[dirStr].Add(new FileInfo(file));
				}
			}

			ViewBag.DirFiles = dirFiles;

			return View();
		}
		public ActionResult _ViewYummyOnline() {
			return View();
		}
		public ActionResult _ViewHotel() {
			return View();
		}

		public async Task<ActionResult> GetFile(string dir, string name) {
			User user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
			SystemConfig config = await YummyOnlineManager.GetSystemConfig();
			string path = $"{config.SpecificationDir}\\{dir}\\{name}";
			if(name.EndsWith(".html") || name.EndsWith(".htm")) {
				await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Info, $"{user.Id}({user.UserName}) Reads {dir}\\{name}");
				return File(path, "text/html");
			}
			await YummyOnlineManager.RecordLog(Log.LogProgram.System, Log.LogLevel.Info, $"{user.Id}({user.UserName}) Downloads {dir}\\{name}");
			return File(path, "application/octet-stream", name);
		}

		public JsonResult GetYummyOnlinePrograms() {
			List<dynamic> programs = new List<dynamic>();
			foreach(var item in Enum.GetValues(typeof(Log.LogProgram))) {
				programs.Add(new {
					Id = item,
					Name = item.ToString()
				});
			}
			return Json(programs);
		}
		public async Task<JsonResult> GetYummyOnlineLogs(Log.LogProgram? program, DateTime dateTime, int? count) {
			if(!program.HasValue)
				program = Log.LogProgram.TcpServer;

			return Json(new {
				Program = new {
					Id = program.Value,
					Name = program.Value.ToString()
				},
				Logs = await YummyOnlineManager.GetLogs(program.Value, dateTime, count)
			});
		}

		public async Task<JsonResult> GetHotelLogs(int? hotelId, DateTime dateTime, int? count) {
			Hotel hotel;
			if(hotelId.HasValue) {
				hotel = await YummyOnlineManager.GetHotelById(hotelId.Value);
			}
			else {
				hotel = await YummyOnlineManager.GetFirstHotel();
			}

			HotelDAO.HotelManager manager = new HotelDAO.HotelManager(hotel.ConnectionString);
			return Json(new {
				Hotel = new {
					Id = hotel.Id,
					Name = hotel.Name
				},
				Logs = await manager.GetLogs(dateTime, count)
			});
		}

		[AllowAnonymous]
		public async Task<JsonResult> RemoteRecord(int? hotelId, int level, string message) {
			if(hotelId == null) {
				await YummyOnlineManager.RecordLog(Log.LogProgram.Remote, (Log.LogLevel)level, message);
			}
			else {
				string connStr = await YummyOnlineManager.GetHotelConnectionStringById((int)hotelId);
				HotelDAO.HotelManager hotelManager = new HotelDAO.HotelManager(connStr);
				await hotelManager.RecordLog((HotelDAO.Models.Log.LogLevel)level, message);
			}

			Response.Headers.Add("Access-Control-Allow-Origin", "*");
			Response.Headers.Add("Access-Control-Allow-Methods", "POST");
			Response.Headers.Add("Access-Control-Allow-Headers", "x-requested-with,content-type");
			return Json(new JsonSuccess());
		}
	}
}