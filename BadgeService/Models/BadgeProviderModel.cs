using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BadgeService.Models
{
    public class BadgeProviderModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string key { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public string endpoint { get; set; }
        public string signature { get; set; }
    }
}