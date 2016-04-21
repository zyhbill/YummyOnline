using System.Web.Mvc;
using System.Web.Routing;

namespace OrderSystem {
	public class RouteConfig {
		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			//routes.MapMvcAttributeRoutes();
			routes.MapRoute(
				name: "HomePartial",
				url: "Home/{action}",
				defaults: new { controller = "Home"},
				namespaces: new[] { "OrderSystem.Controllers" }
			);
			routes.MapRoute(
				name: "Home",
				url: "Home/{HotelId}/{QrCode}",
				defaults: new { controller = "Home", action = "Index" },
				namespaces: new[] { "OrderSystem.Controllers" }
			);

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
				namespaces: new[] { "OrderSystem.Controllers" }
			);

		}
	}
}
