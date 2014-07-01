using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Script.Serialization;
using BadgeHelper;
using BadgeService.Models;

namespace BadgeService.Controllers
{
    [EnableCors("*", "*", "*")]
    public class BadgeController : ApiController
    {
        #region BadgeAuthorityService
      
        //method to check service status
        /*  POST Methods   - parameters mentioned for each method needs to be send in the json object while making jquery ajax calls or send class object for normal service calls*/
      
        #region GET

       //GET
         [ActionName("status")]
        public string Get()
        {
            return "status: OK";
          
        }

     

        #endregion

        #endregion

    }
}
