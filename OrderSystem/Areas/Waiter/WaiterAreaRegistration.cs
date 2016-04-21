using System.Web.Mvc;

namespace OrderSystem.Areas.Waiter {
	public class WaiterAreaRegistration : AreaRegistration {
		public override string AreaName {
			get {
				return "Waiter";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context) {
			context.MapRoute(
				"Waiter_default",
				"Waiter/{controller}/{action}/{id}",
				new { controller = "Account", action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}