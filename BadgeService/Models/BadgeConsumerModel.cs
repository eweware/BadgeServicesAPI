using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BadgeService.Models
{
    public class BadgeConsumerModel
    {
        //Consumer Id
    
        public string consumer { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string endpoint { get; set; }

        //Unregister Consumer
        public string signature { get; set; }
        public string issuer { get; set; }


    }
}