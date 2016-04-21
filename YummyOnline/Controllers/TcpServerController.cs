using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;
using Protocal;
using Newtonsoft.Json;

namespace YummyOnline.Controllers {
	[Authorize(Roles = nameof(Role.SuperAdmin))]
	public class TcpServerController : BaseController {
		static string processName = "YummyOnlineTcpServer";
		static PerformanceCounter curtime = new PerformanceCounter("Process", "% Processor Time", processName);
		static PerformanceCounter curpcp = new PerformanceCounter("Process", "Working Set - Private", processName);

		public ActionResult Index() {
			return View();
		}
		public ActionResult _ViewStatus() {
			return View();
		}
		public ActionResult _ViewGuids() {
			return View();
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
		public async Task<JsonResult> AddGuid(Guid guid, string description) {
			if(!await YummyOnlineManager.AddGuid(new NewDineInformClientGuid {
				Guid = guid,
				Description = description
			})) {
				return Json(new JsonError());
			}
			return Json(new JsonSuccess());
		}
		public async Task<JsonResult> DeleteGuid(Guid guid) {
			if(!await YummyOnlineManager.DeleteGuid(guid)) {
				return Json(new JsonError());
			}
			return Json(new JsonSuccess());
		}
	}
}