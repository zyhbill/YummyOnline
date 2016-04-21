using HotelDAO;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;

namespace OrderSystem.Areas.Waiter.Controllers {
	public class BaseWaiterController : OrderSystem.Controllers.BaseController {
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

		private HotelManagerForWaiter _hotelManager;
		public HotelManagerForWaiter HotelManager {
			get {
				if(_hotelManager == null) {
					Hotel hotel = CurrHotel;
					if(hotel == null) {
						HotelMissingError();
					}
					string connString = hotel.ConnectionString;
					_hotelManager = new HotelManagerForWaiter(connString);
				}
				return _hotelManager;
			}
		}
	}
}