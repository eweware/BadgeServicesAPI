using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace BadgeProvider
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableCors();
            // Web API routes
            //config.MapHttpAttributeRoutes();
           

              config.Routes.MapHttpRoute(
               name: "BadgeApi",
               routeTemplate: "BadgeProvider/status",
               defaults: new { controller = "Badge", action = "status" }
           );
              config.Routes.MapHttpRoute(
               name: "BadgeTypeApi",
               routeTemplate: "BadgeProvider/BadgeTypes",
               defaults: new { controller = "BadgeType", action = "GetBadgeTypes" }
           );
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "BadgeProvider/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
