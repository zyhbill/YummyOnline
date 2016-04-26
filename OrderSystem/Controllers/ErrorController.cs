using System.Web.Mvc;

namespace OrderSystem.Controllers {
	public class ErrorController : BaseOrderSystemController {
		// GET: Error
		public ActionResult HotelMissing() {
			return View();
		}
	}
}