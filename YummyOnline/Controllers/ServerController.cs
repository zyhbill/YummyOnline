using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;
using Protocal;
using Newtonsoft.Json;
using YummyOnline.Utility;

namespace YummyOnline.Controllers {
	[Authorize(Roles = nameof(Role.Admin))]
	public class ServerController : BaseController {

		public ActionResult Index() {
			return View();
		}
		public ActionResult _ViewIISStatus() {
			return View();
		}
		public ActionResult _ViewTcpServerStatus() {
			return View();
		}
		public ActionResult _ViewGuids() {
			return View();
		}

		public JsonResult GetIISInfo() {
			return Json(IISManager.GetSites());
		}
		public JsonResult StartSite(int siteId) {
			if(IISManager.StartSite(siteId)) {
				return Json(new JsonSuccess());
			}
			return Json(new JsonError("无法启动"));
		}
		public JsonResult StopSite(int siteId) {
			if(IISManager.StopSite(siteId)) {
				return Json(new JsonSuccess());
			}
			return Json(new JsonError("无法停止"));
		}

		public async Task<JsonResult> GetTcpServerInfo() {
			return Json(await TcpServerProcess.GetTcpServerInfo());
		}

		public async Task<JsonResult> StartTcpServer() {
			bool result = await TcpServerProcess.StartTcpServer();
			if(result) {
				return Json(new JsonSuccess());
			}
			return Json(new JsonError("开启失败"));
		}

		[Authorize(Roles = nameof(Role.SuperAdmin))]
		public JsonResult StopTcpServer() {
			bool result = TcpServerProcess.StopTcpServer();
			if(result) {
				return Json(new JsonSuccess());
			}
			return Json(new JsonError("关闭失败"));
		}

		private Process getTcpServerProcess() {
			Process[] processes = Process.GetProcessesByName("YummyOnlineTcpServer");
			if(processes.Length == 0) {
				return null;
			}
			return processes[0];
		}

		public async Task<JsonResult> GetGuids() {
			return Json(await YummyOnlineManager.GetGuids());
		}
		[Authorize(Roles = nameof(Role.SuperAdmin))]
		public async Task<JsonResult> AddGuid(Guid guid, string description) {
			if(!await YummyOnlineManager.AddGuid(new NewDineInformClientGuid {
				Guid = guid,
				Description = description
			})) {
				return Json(new JsonError());
			}
			return Json(new JsonSuccess());
		}
		[Authorize(Roles = nameof(Role.SuperAdmin))]
		public async Task<JsonResult> DeleteGuid(Guid guid) {
			if(!await YummyOnlineManager.DeleteGuid(guid)) {
				return Json(new JsonError());
			}
			return Json(new JsonSuccess());
		}
	}
}