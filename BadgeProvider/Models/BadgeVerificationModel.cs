using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BadgeProvider.Models
{
    public class BadgeVerificationModel
    {
        public string requestId { get; set; }
        public string data { get; set; }
        public string signature { get; set; }
    }

    public class BadgeInformationModel
    {
        public string BadgeRequestID { get; set; }
        public string EncryptedJSON { get; set; }
        public string ProviderID { get; set; }
        public string BadgeID { get; set; }
        public string Signature { get; set; }
        public string ConsumerId { get; set; }
    }
}