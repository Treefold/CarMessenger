using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CarMessenger
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Cars",
            //    url: "Cars/{action=Index}/{id?}",  //"Manage/Cars/{action=Index}/{id?}",
            //    defaults: new { controller = "CarsController"}
            //);

            routes.MapRoute(
                name: "Remove Owner",
                url: "Cars/RemoveCoOwner/{id}/{email}",
                new { Controller = "Cars", action = "RemoveCoOwners"}
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
