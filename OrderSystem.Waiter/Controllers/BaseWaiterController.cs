using OrderSystem.Utility;
using System.Web.Mvc;
using Utility;
using YummyOnlineDAO;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;

namespace OrderSystem.Waiter.Controllers {
	public abstract class BaseWaiterController : OrderSystem.Controllers.BaseController {
		private StaffManager _staffManager;
		public StaffManager StaffManager {
			get {
				if(_staffManager == null) {
					_staffManager = new StaffManager();
				}
				return _staffManager;
			}
		}

		private StaffSigninManager _signinManager;
		public StaffSigninManager SigninManager {
			get {
				if(_signinManager == null) {
					_signinManager = new StaffSigninManager(HttpContext);
				}
				return _signinManager;
			}
		}

		private HotelManager _hotelManager;
		public HotelManager HotelManager {
			get {
				if(_hotelManager == null) {
					_hotelManager = new HotelManager(CurrHotel.ConnectionString);
				}
				return _hotelManager;
			}
		}

		public new Hotel CurrHotel {
			get {
				if(Session["Hotel"] == null) {
					Staff staff = AsyncInline.Run(() => new StaffManager().FindStaffById(User.Identity.GetUserId()));

					Session["Hotel"] = AsyncInline.Run(() => new YummyOnlineManager().GetHotelById(staff.HotelId));
				}
				return (Hotel)Session["Hotel"];
			}
			set {
				Session["Hotel"] = value;
			}
		}
	}

	/// <summary>
	/// 饭店必须可用
	/// </summary>
	public class HotelAvailableAttribute : OrderSystem.Controllers.HotelAvailableAttribute { }
}