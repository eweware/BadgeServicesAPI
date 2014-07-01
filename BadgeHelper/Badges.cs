using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadgeHelper
{
    public class Badges
    {
        public string BadgeID { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string ExpiryDate { get; set; }
        public string Description { get; set; }
    }

    public class BadgesList
    {
        public string BadgeRequestID { get; set; }
        public List<Badges> BadgeList { get; set; }
        public string Signature { get; set; }
    }
}
