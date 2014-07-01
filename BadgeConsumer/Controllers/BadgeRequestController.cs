using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Script.Serialization;
using BadgeHelper;


namespace BadgeConsumer.Controllers
{
    [EnableCors("*", "*", "*")]
    public class BadgeRequestController : ApiController
    {
        string BAPubKey = "2989068685854285";
        string BCPvrKey = "9867546878388418"; //2416493963645987589317 --Pub Key
        byte[] BCPvrKeybytes = Encoding.UTF8.GetBytes("9867546878388418");
        byte[] iv = Encoding.UTF8.GetBytes("9867546878388418");
        EncryptionAndDecryption encryptDecryptObj;

        //GET
        [ActionName("status")]
        public string Get()
        {
            return "status: OK";

        }

        /// <summary>
        /// GET - CompleteBadgeRequest
        /// </summary>
        /// <param name="BadgeRequestID"></param>
        /// <param name="BadgeList"></param>
        /// <param name="Signature"></param>
        /// <returns></returns>string BadgeRequestID, List<Badges> BadgeList, string Signature
        public string GET(BadgesList model)
        {
            if (!string.IsNullOrWhiteSpace(model.Signature))
            {
                encryptDecryptObj = new EncryptionAndDecryption();
                string sign = encryptDecryptObj.DecryptString(model.Signature, BAPubKey);

                if (sign.Equals(model.Signature))
                {
                    BadgesList badgeObj = new BadgesList();
                    badgeObj.BadgeRequestID = model.BadgeRequestID;
                    badgeObj.BadgeList = model.BadgeList;
                    badgeObj.Signature = sign;
                    return badgeObj.ToJSON();
                }
                else
                    return "Error : Invalid Signature";
            }
            else
                return "Error: Invalid Signature";
        }
    }
}
