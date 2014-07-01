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
    public class BadgeIssuersController : ApiController
    {
        BadgeCommon objBadgeCommon;
        EncryptionAndDecryption encryptDecryptObj;
        string BPPubKey = ConfigurationManager.AppSettings["BPPubKey"];
        string BAPvrKey = ConfigurationManager.AppSettings["BAPvrKey"];
        byte[] BAPvrKeybytes = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BAPvrKey"]);
        byte[] iv = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BAPvrKey"]);

        #region BadgeIssuer
        //POST
        /// <summary>
        /// RegisterBadgeIssuer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="description"></param>
        /// <param name="image"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>       
        public string POST(BadgeIssuerModel badgeprovider)
        {
            try
            {
                encryptDecryptObj = new EncryptionAndDecryption();

                //DECRYPT FROM CRIPTOJS
                byte[] data = Convert.FromBase64String(badgeprovider.name);
                string decodedString = Encoding.UTF8.GetString(data);
                var encrypted = Convert.FromBase64String(decodedString);
                string dName = encryptDecryptObj.DecryptStringFromBytes(encrypted, BAPvrKeybytes, iv);

                data = Convert.FromBase64String(badgeprovider.key);
                decodedString = Encoding.UTF8.GetString(data);
                encrypted = Convert.FromBase64String(decodedString);
                string dKey = encryptDecryptObj.DecryptStringFromBytes(encrypted, BAPvrKeybytes, iv);

                data = Convert.FromBase64String(badgeprovider.description);
                decodedString = Encoding.UTF8.GetString(data);
                encrypted = Convert.FromBase64String(decodedString);
                string dDescription = encryptDecryptObj.DecryptStringFromBytes(encrypted, BAPvrKeybytes, iv);

                data = Convert.FromBase64String(badgeprovider.image);
                decodedString = Encoding.UTF8.GetString(data);
                encrypted = Convert.FromBase64String(decodedString);
                string dImage = encryptDecryptObj.DecryptStringFromBytes(encrypted, BAPvrKeybytes, iv);

                data = Convert.FromBase64String(badgeprovider.endpoint);
                decodedString = Encoding.UTF8.GetString(data);
                encrypted = Convert.FromBase64String(decodedString);
                string dendPoint = encryptDecryptObj.DecryptStringFromBytes(encrypted, BAPvrKeybytes, iv);

                SqlParameter[] Parm = new SqlParameter[5];
                Parm[0] = new SqlParameter("@Name", dName);
                Parm[1] = new SqlParameter("@Key", dKey);
                Parm[2] = new SqlParameter("@Description", dDescription);
                Parm[3] = new SqlParameter("@Image", dImage);
                Parm[4] = new SqlParameter("@endPoint", dendPoint);
                DataSet DataSetTemp = new DataSet();
                SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_RegisterBadgeProvider", DataSetTemp, new string[1] { "tblResult" }, Parm);

                objBadgeCommon = new BadgeCommon();
                return objBadgeCommon.GetJsonFromDataSet(DataSetTemp);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrWhiteSpace(badgeprovider.name) || string.IsNullOrWhiteSpace(badgeprovider.key) || string.IsNullOrWhiteSpace(badgeprovider.description)
                    || string.IsNullOrWhiteSpace(badgeprovider.image) || string.IsNullOrWhiteSpace(badgeprovider.endpoint))
                    return (new JavaScriptSerializer().Serialize("Error: One of the paramter is empty."));

                return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
            }
        }

        //DELETE
        /// <summary>
        /// UnregisterBadgeIssuer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="signature"></param>
        /// <returns></returns>       
        public string DELETE(BadgeIssuerModel badgeprovider)
        {
            try
            {
                bool isValidSignature = false;
                byte[] data = Convert.FromBase64String(badgeprovider.id);
                string dProviderID = Encoding.UTF8.GetString(data);

                DataSet DSTemp = new DataSet();
                SqlParameter[] Parm = new SqlParameter[1];
                Parm[0] = new SqlParameter("@ProviderID", dProviderID);
                SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_GetProviderPublicKey", DSTemp, new string[1] { "tblResult" }, Parm);

                if (DSTemp != null && DSTemp.Tables.Count > 0)
                {
                    if (DSTemp.Tables[0].Rows.Count > 0)
                    {
                        byte[] BPPubKeybytes = Encoding.UTF8.GetBytes(DSTemp.Tables[0].Rows[0]["PublicKey"].ToString());
                        encryptDecryptObj = new EncryptionAndDecryption();
                        data = Convert.FromBase64String(badgeprovider.signature);
                        string decodedString = Encoding.UTF8.GetString(data);
                        var encrypted = Convert.FromBase64String(decodedString);
                        string dSignature = encryptDecryptObj.DecryptStringFromBytes(encrypted, BPPubKeybytes, iv);

                        isValidSignature = (!string.IsNullOrWhiteSpace(dSignature)) ? true : false;

                        if (isValidSignature)
                        {
                            Parm = new SqlParameter[1];
                            Parm[0] = new SqlParameter("@ProviderID", dProviderID);
                            DataSet DataSetTemp = new DataSet();
                            SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_UnregisterBadgeProvider", DataSetTemp, new string[1] { "tblResult" }, Parm);

                            objBadgeCommon = new BadgeCommon();
                            return objBadgeCommon.GetJsonFromDataSet(DataSetTemp);
                        }
                    }
                      else return (new JavaScriptSerializer().Serialize("Error: Invalid Issuer Id"));
                }
            }
            catch (Exception ex)
            {
                return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
            }
            return (new JavaScriptSerializer().Serialize("Error: Invalid Signature"));
        }

         //GET
        // Write method for GetBadgeIssuers 

        //GetBadgeProviders
        public string GET()
        {
            try
            {
                DataSet DataSetTemp = new DataSet();
                SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_GetBadgeIssuers", DataSetTemp, new string[1] { "tblResult" });

                objBadgeCommon = new BadgeCommon();
                return System.Text.RegularExpressions.Regex.Replace((objBadgeCommon.GetJsonFromDataSet(DataSetTemp)), @"\\", "/");

                //return objBadgeCommon.GetJsonFromDataSet(DataSetTemp);

                //var objReturn = objBadgeCommon.GetJsonFromDataSet(DataSetTemp);
                //return objReturn;
            }
            catch (Exception ex)
            {
                return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
            }
        }

        #endregion
    }
}
