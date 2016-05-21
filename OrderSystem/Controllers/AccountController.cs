using HotelDAO;
using OrderSystem.Models;
using OrderSystem.Utility;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;
using Protocal;

namespace OrderSystem.Controllers {
	[RequireHotel]
	[HotelAvailable]
	public class AccountController : BaseOrderSystemController {
		// GET: Account
		public ActionResult Index() {
			return View();
		}
		public ActionResult _ViewUser() {
			return View();
		}
		public ActionResult _ViewSignup() {
			return View();
		}
		public async Task<ActionResult> _ViewSignin() {
			ViewBag.NeedCodeImg = (await HotelManager.GetHotelConfig()).NeedCodeImg;
			return View();
		}
		public ActionResult _ViewForget() {
			return View();
		}

		#region 用户注册
		public async Task<JsonResult> SendSMS(string PhoneNumber) {
			if(await UserManager.IsPhoneNumberDuplicated(PhoneNumber)) {
				return Json(new JsonError("此号码已注册"));
			}

			Random rand = new Random(unchecked((int)DateTime.Now.Ticks));
			string code = "";
			for(int i = 0; i < 6; i++) {
				code += rand.Next(10);
			}
			Session["SmsCode"] = code;

#if DEBUG
			await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Debug, PhoneNumber + " ： " + code);
#else
			if(!SMS.SMSSender.Send(PhoneNumber, code)) {
				return Json(new JsonError());
			}
#endif

			return Json(new JsonSuccess());
		}
		public async Task<JsonResult> Signup(SignupViewModel model) {
			if(Session["SmsCode"] == null || Session["SmsCode"].ToString() != model.Code) {
				return Json(new JsonError("验证码不正确", "code"));
			}
			User user;
			bool succeeded;
			if(await SigninManager.IsAuthenticated()) {
				user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
				user.PhoneNumber = model.PhoneNumber;
				user.UserName = model.PhoneNumber;
				user.PasswordHash = UserManager.GetMd5(model.Password);
				succeeded = await UserManager.UpdateAsync(user);
				await UserManager.RemoveFromRoleAsync(user.Id, Role.Nemo);

				await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Warning, $"User Update: {User.Identity.GetUserId()}");
			}
			else {
				user = new User {
					PhoneNumber = model.PhoneNumber,
					UserName = model.PhoneNumber
				};
				succeeded = await UserManager.CreateAsync(user, model.Password);
			}

			if(!succeeded) {
				return Json(new JsonError("注册失败"));
			}
			await UserManager.AddToRoleAsync(user.Id, Role.Customer);
			SigninManager.Signin(user, true);
			await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Success, $"User Signup: {user.Id} ({user.PhoneNumber})");
			return Json(new JsonSuccess());
		}
		#endregion

		#region 用户登录
		public FileContentResult CodeImage() {
			string code = CodeImg.CreateRandomCode();
			Session["CodeImg"] = code.ToLower();
			return File(CodeImg.CreateCheckCodeImage(code), "image/jpeg");
		}
		public async Task<JsonResult> Signin(SigninViewModel model) {
			bool needCodeImg = (await HotelManager.GetHotelConfig()).NeedCodeImg;
			if(needCodeImg && (Session["CodeImg"] == null || model.CodeImg.ToLower() != Session["CodeImg"].ToString())) {
				return Json(new JsonError("验证码不正确"));
			}
			User user = await UserManager.FindByPhoneNumberAsync(model.PhoneNumber);
			if(user == null) {
				await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Warning, $"User Signin: {model.PhoneNumber} {model.Password} No PhoneNumber");
				return Json(new JsonError("手机未注册"));
			}
			if(!await UserManager.CheckPasswordAsync(user, model.Password)) {
				await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Warning, $"User Signin: {model.PhoneNumber} {model.Password} Password Error");
				return Json(new JsonError("密码不正确"));
			}
			if(User.Identity.IsAuthenticated) {
				User oldUser = await UserManager.FindByIdAsync(User.Identity.GetUserId());
				if(oldUser != null && await UserManager.IsInRoleAsync(oldUser.Id, Role.Nemo)) {
					await HotelManager.TransferOrders(oldUser.Id, user.Id);
					await UserManager.DeleteAsync(oldUser);

					await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Warning, $"User Transfer: {oldUser.Id} -> {user.Id}");
				}
			}
			SigninManager.Signin(user, true);
			await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Success, $"User Signin: {user.Id} ({user.PhoneNumber})");
			return Json(new JsonSuccess());
		}
		#endregion

		#region 登出
		public JsonResult Signout() {
			SigninManager.Signout();
			return Json(new JsonSuccess());
		}
		#endregion

		#region 忘记密码
		public async Task<ActionResult> SendForgetSMS(string PhoneNumber) {
			if(!await UserManager.IsPhoneNumberDuplicated(PhoneNumber)) {
				return Json(new JsonError("此号码未注册"));
			}

			Random rand = new Random(unchecked((int)DateTime.Now.Ticks));
			string code = "";
			for(int i = 0; i < 6; i++) {
				code += rand.Next(10);
			}
			Session["SMSForgetCode"] = code;

#if DEBUG
			await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Debug, PhoneNumber + " ： " + code);
#else
			if(!SMS.SMSSender.Send(PhoneNumber, code)) {
				return Json(new JsonError());
			}
#endif

			return Json(new JsonSuccess());
		}
		public async Task<JsonResult> Forget(ForgetViewModel model) {
			if(Session["SMSForgetCode"] == null || Session["SMSForgetCode"].ToString() != model.Code) {
				return Json(new JsonError("验证码不正确", "code"));
			}
			await UserManager.ChangePasswordAsync(model.PhoneNumber, model.Password);

			await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Warning, $"{model.PhoneNumber} {model.Password} Change Password");

			return Json(new JsonSuccess());
		}
		#endregion

		public async Task<JsonResult> IsAuthenticated() {
			if(!await SigninManager.IsAuthenticated()) {
				return Json(new JsonError());
			}
			return Json(new JsonSuccess(await getCustomerInfo()));
		}

		public async Task<JsonResult> GetCustomer() {
			return Json(await getCustomerInfo());
		}

		private async Task<dynamic> getCustomerInfo() {
			string userId = User.Identity.GetUserId();
			User user = await UserManager.FindByIdAsync(userId);
			if(user == null) {
				return null;
			}
			HotelDAO.Models.Customer customer = await HotelManager.GetOrCreateCustomer(userId);

			return new {
				user.Id,
				user.Email,
				user.PhoneNumber,
				user.UserName,
				customer.Points,
				customer.VipLevelId,
				DinesCount = await HotelManager.GetHistoryDinesCount(userId)
			};
		}

	}
}