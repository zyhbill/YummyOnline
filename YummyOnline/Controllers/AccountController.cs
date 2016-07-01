using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YummyOnlineDAO.Models;

namespace YummyOnline.Controllers {
	public class AccountController : BaseController {
		// GET: Account
		public ActionResult Index() {
			return View();
		}

		public async Task<JsonResult> Signin(string userName, string password, bool rememberMe) {
			User user = null;
			if(userName.Contains('@')) {
				user = await UserManager.FindByEmailAsync(userName);
			}
			else {
				user = await UserManager.FindByPhoneNumberAsync(userName);
			}
			if(user == null) {
				await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Warning, $"Admin Signin: {userName} No UserName, Host: {Request.UserHostAddress}");
				return Json(new JsonError("未找到此用户"));
			}
			if(!await UserManager.CheckPasswordAsync(user, password)) {
				await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Warning, $"Admin Signin: {userName} Password Error, Host: {Request.UserHostAddress}",
					$"Password: {password}");
				return Json(new JsonError("密码不正确"));
			}
			if(!await UserManager.IsInRoleAsync(user.Id, Role.Admin)) {
				await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Warning, $"Admin Signin: {userName} No Authority, Host: {Request.UserHostAddress}");
				return Json(new JsonError("没有权限"));
			}
			SigninManager.Signin(user, rememberMe);
			await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Success, $"Admin Signin: {user.Id} ({user.UserName }), Host: {Request.UserHostAddress}");
			return Json(new JsonSuccess());
		}

		public JsonResult Signout() {
			SigninManager.Signout();
			return Json(new JsonSuccess());
		}
	}
}