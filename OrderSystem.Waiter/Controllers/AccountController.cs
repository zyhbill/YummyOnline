using HotelDAO;
using OrderSystem.Models;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;
using Protocal;

namespace OrderSystem.Waiter.Controllers {
	public class AccountController : BaseWaiterController {
		public ActionResult Index() {
			return View();
		}

		public async Task<JsonResult> Signin(string signinName, string password) {
			Staff staff = await StaffManager.FindStaffBySigninName(signinName);
			if(staff == null) {
				return Json(new JsonError("没有此登录名"));
			}
			if(!await StaffManager.CheckPasswordAsync(staff, password)) {
				await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Warning, $"Staff Signin: {signinName} {password} Signin Failed");
				return Json(new JsonError("密码不正确"));
			}

			Hotel hotel = await YummyOnlineManager.GetHotelById(staff.HotelId);
			CurrHotel = hotel;

			if(!await HotelManager.IsStaffHasSchema(staff.Id, HotelDAO.Models.Schema.ReadWaiterData)) {
				await YummyOnlineManager.RecordLog(Log.LogProgram.Identity, Log.LogLevel.Warning, $"Staff Signin: {signinName} {password} No Authority");
				return Json(new JsonError("没有权限"));
			}
			await SigninManager.Signin(staff, true);

			return Json(new JsonSuccess());
		}

		public JsonResult Signout() {
			SigninManager.Signout();
			return Json(new JsonSuccess());
		}

		[Authorize(Roles = nameof(HotelDAO.Models.Schema.ReadWaiterData))]
		public async Task<JsonResult> VerifyCustomer(string phoneNumber, string password) {
			UserManager userManager = new UserManager();
			User user = await userManager.FindByPhoneNumberAsync(phoneNumber);
			if(user == null) {
				return Json(new JsonError("手机号未注册"));
			}
			if(!await userManager.CheckPasswordAsync(user, password)) {
				return Json(new JsonError("密码不正确"));
			}
			if(!await userManager.IsInRoleAsync(user.Id, Role.Customer)) {
				return Json(new JsonError("不是会员"));
			}

			HotelManager hotelManager = new HotelManager(CurrHotel.ConnectionString);

			return Json(new JsonSuccess(await hotelManager.GetOrCreateCustomerOrCreate(user.Id)));
		}
		[Authorize(Roles = nameof(HotelDAO.Models.Schema.ReadWaiterData))]
		public async Task<JsonResult> IsCustomer(string userId) {
			UserManager userManager = new UserManager();
			if(!await userManager.IsInRoleAsync(userId, Role.Customer)) {
				return Json(new JsonError("不是会员"));
			}

			HotelManager hotelManager = new HotelManager(CurrHotel.ConnectionString);
			return Json(new JsonSuccess(await hotelManager.GetOrCreateCustomerOrCreate(userId)));
		}
	}
}