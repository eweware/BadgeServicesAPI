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
using BadgeService.Models;

namespace BadgeService.Controllers
{
    [EnableCors("*", "*", "*")]
    public class BadgeInfoController : ApiController
    {

        BadgeCommon objBadgeCommon;
        EncryptionAndDecryption encryptDecryptObj;
        string BPPubKey = ConfigurationManager.AppSettings["BPPubKey"];
        string BAPvrKey = ConfigurationManager.AppSettings["BAPvrKey"];
        byte[] BAPvrKeybytes = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BAPvrKey"]);
        byte[] iv = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BAPvrKey"]);


        //GET 
        // Write method for GetBadgeInfo  
        //Method mapping needs to be verfied      
        /// <summary>
        /// GetBadgeInformation  - POST Method
        /// </summary>
        /// <param name="issuerId"></param>
        /// <param name="badgeId"></param>
        /// <param name="signature"></param>
        /// <returns></returns>


      //GetBadgeInformation
      //  public string Get(GetBadgeInformationModel model)
        public string Get(string issuerId, string badgeId, string signature)
        {
            try
            {
                bool isValidSignature = false;
                encryptDecryptObj = new EncryptionAndDecryption();

                //DECRYPT FROM CRIPTOJS
                byte[] data = Convert.FromBase64String(issuerId);
                string dProviderID = Encoding.UTF8.GetString(data);

                data = Convert.FromBase64String(badgeId);
                string dBadgeID = Encoding.UTF8.GetString(data);

                byte[] BAPubKeybytes = Encoding.UTF8.GetBytes("2989068685854285"); //Encoding.UTF8.GetBytes(DSTemp.Tables[0].Rows[0]["PublicKey"].ToString());
                encryptDecryptObj = new EncryptionAndDecryption();
                data = Convert.FromBase64String(signature);
                string decodedString = Encoding.UTF8.GetString(data);
                var encrypted = Convert.FromBase64String(decodedString);
                string dSignature = encryptDecryptObj.DecryptStringFromBytes(encrypted, BAPubKeybytes, iv);

                isValidSignature = (!string.IsNullOrWhiteSpace(dSignature)) ? true : false;

                if (isValidSignature)
                {
                    SqlParameter[] Parm = new SqlParameter[2];
                    Parm[0] = new SqlParameter("@ProviderID", dProviderID);
                    Parm[1] = new SqlParameter("@BadgeID", dBadgeID);
                    DataSet DataSetTemp = new DataSet();
                    SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_GetBadgeInformation", DataSetTemp, new string[1] { "tblResult" }, Parm);

                    objBadgeCommon = new BadgeCommon();
                    return objBadgeCommon.GetJsonFromDataSet(DataSetTemp);

                }
            }
            catch (Exception ex)
            {
                return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
            }
            return (new JavaScriptSerializer().Serialize("Error: Invalid either Issuer id or BadgeRequestID"));//provider id
        }





      
    }
}
