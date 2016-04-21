using System.Web.Mvc;

namespace OrderSystem.Controllers {
	public class ErrorController : BaseCommonController {
		// GET: Error
		public ActionResult HotelMissing() {
			return View();
		}
	}
}