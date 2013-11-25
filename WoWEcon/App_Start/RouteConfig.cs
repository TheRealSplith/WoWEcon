using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WoWEcon
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "realm",
                url: "{realm}/{faction}/{controller}/{action}/{id}",
                defaults: new
                {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional,
                    realm = UrlParameter.Optional,
                    faction = UrlParameter.Optional
                },
                namespaces: new String[] { "WoWEcon.Controllers" }
            );
            routes.MapRoute(
                name: "api",
                url: "api/{controller}/{action}",
                namespaces: new String[] { "WoWEcon.Controllers.Api" }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new String[] { "WoWEcon.Controllers"}
            );

        }
    }
}