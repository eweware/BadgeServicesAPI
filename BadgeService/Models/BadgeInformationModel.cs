using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BadgeService.Models
{
    public class BadgeInformationModel
    {
        public string BadgeRequestID { get; set; }

        public string EncryptedJSON { get; set; }
        public string ProviderID { get; set; }
        public string BadgeID { get; set; }
        public string Signature { get; set; }   
        public string ConsumerId { get; set; }   
    }
    public class GetBadgeInformationModel
    {
        public string issuerId { get; set; }
        public string badgeId { get; set; }
        public string signature { get; set; }       
    }
}