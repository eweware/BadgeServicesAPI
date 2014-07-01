using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Routing;

namespace BadgeService
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.EnableCors();
            // Web API routes
            config.MapHttpAttributeRoutes();


            config.Routes.MapHttpRoute(
               name: "BadgeApi",
               routeTemplate: "BadgeAuthority/status",
               defaults: new { controller = "Badge", action = "status" }
           );

           //Uncomment if parameter needs to be sent
            config.Routes.MapHttpRoute(
             name: "DefaultApi",
             routeTemplate: "BadgeAuthority/{controller}/{issuerId}/{badgeId}/{signature}",
             defaults: new { issuerId = RouteParameter.Optional, badgeId = RouteParameter.Optional, signature = RouteParameter.Optional, controller = "BadgeInfo" }
         );
              config.Routes.MapHttpRoute(
             name: "DefaultBadgeVerApi",
             routeTemplate: "BadgeAuthority/{controller}/{requestId}",
             defaults: new { requestId = RouteParameter.Optional, controller = "BadgeVerificationInfo" }
         );
            
            config.Routes.MapHttpRoute(
               name: "DefaultbadgeApi",
               routeTemplate: "BadgeAuthority/{controller}/{id}",
               defaults: new { id = RouteParameter.Optional }
           );

            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
