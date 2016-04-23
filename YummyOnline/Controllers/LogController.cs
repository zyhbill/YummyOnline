using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO.Models;
using Protocal;

namespace YummyOnline.Controllers {
	[Authorize(Roles = nameof(Role.Admin))]
	public class LogController : BaseController {
		private DirectoryInfo bin = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

		// GET: Log
		public ActionResult Index() {
			return View();
		}
		public ActionResult _ViewSpecification() {
			Dictionary<string, List<string>> dirFiles = new Dictionary<string, List<string>>();


			string[] dirs = Directory.GetDirectories($"{bin.Parent.FullName}\\Specification");

			foreach(string dir in dirs) {
				string[] files = Directory.GetFiles(dir);

				string dirStr = dir.Split('\\').Last();
				dirFiles.Add(dirStr, new List<string>());

				foreach(string file in files) {
					dirFiles[dirStr].Add(file.Split('\\').Last());
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

		public ActionResult GetFile(string dir, string name) {
			string path = $"{bin.Parent.FullName}\\Specification\\{dir}\\{name}";
			if(name.EndsWith(".html") || name.EndsWith(".htm")) {
				return File(path, "text/html");
			}
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
		public async Task<JsonResult> GetYummyOnlineLogs(Log.LogProgram program, DateTime dateTime, int? count) {
			return Json(await YummyOnlineManager.GetLogsByProgram(program, dateTime, count));
		}

		public async Task<JsonResult> GetHotelLogs(int hotelId, DateTime dateTime, int? count) {
			Hotel hotel = await YummyOnlineManager.GetHotelById(hotelId);
			HotelDAO.HotelManager manager = new HotelDAO.HotelManager(hotel.ConnectionString);
			return Json(await manager.GetLogs(dateTime, count));
		}

		public async Task<ActionResult> RemoteRecord(int? hotelId, int level, string message) {
			if(hotelId == null) {
				await YummyOnlineManager.RecordLog(Log.LogProgram.Remote, (Log.LogLevel)level, message);
			}
			else {
				string connStr = await YummyOnlineManager.GetHotelConnectionStringById((int)hotelId);
				HotelDAO.HotelManager hotelManager = new HotelDAO.HotelManager(connStr);
				await hotelManager.RecordLog((HotelDAO.Models.Log.LogLevel)level, message);
			}
			return Content(null);
		}
	}
}