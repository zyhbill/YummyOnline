using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YummyOnlineDAO.Models;
using Protocal;

namespace YummyOnline.Controllers {
	public class UsersController : BaseController {
		// GET: Users
		public ActionResult Index() {
			return View();
		}

		public ActionResult _ViewCustomer() {
			return View();
		}
		public ActionResult _ViewAdmin() {
			return View();
		}

		public async Task<JsonResult> GetAdmins() {
			return Json(await YummyOnlineManager.GetUsers(Role.Admin));
		}
		public async Task<JsonResult> AddAdmin(string phoneNumber) {
			User user = await UserManager.FindByPhoneNumberAsync(phoneNumber);
			if(user == null) {
				return Json(new JsonError("此手机号未注册"));
			}
			if(await UserManager.IsInRoleAsync(user.Id, Role.Admin)) {
				return Json(new JsonError("已经为管理员"));
			}
			await UserManager.AddToRoleAsync(user.Id, Role.Admin);
			return Json(new JsonSuccess());
		}
		public async Task<JsonResult> DeleteAdmin(string id) {
			await UserManager.RemoveFromRoleAsync(id, Role.Admin);
			return Json(new JsonSuccess());
		}
	}
}