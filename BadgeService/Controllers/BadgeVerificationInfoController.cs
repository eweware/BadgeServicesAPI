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
    public class BadgeVerificationInfoController : ApiController
    {
        //BadgeCommon objBadgeCommon;
        EncryptionAndDecryption encryptDecryptObj;
        string BPPubKey = ConfigurationManager.AppSettings["BPPubKey"];
        string BAPvrKey = ConfigurationManager.AppSettings["BAPvrKey"];
        byte[] BAPvrKeybytes = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BAPvrKey"]);
        byte[] iv = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BAPvrKey"]);

        public string Get(string requestId)
        {
            if (!string.IsNullOrWhiteSpace(requestId))
            {
                try
                {
                    if (String.IsNullOrEmpty(BAPvrKey))
                        return ("Error: A Private Key must be configured to use the Recurly Transparent Post API.");

                    SqlParameter[] Parm = new SqlParameter[1];
                    Parm[0] = new SqlParameter("@BadgeRequestID", requestId);
                    DataSet DataSetTemp = new DataSet();
                    SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_GetBadgeProviderEndPoint", DataSetTemp, new string[1] { "tblResult" }, Parm);

                    if (DataSetTemp != null && DataSetTemp.Tables.Count > 0)
                    {
                        encryptDecryptObj = new EncryptionAndDecryption();
                        BadgeInformationModel hc = new BadgeInformationModel(); //request ID (param received) wrap it in a data member
                        hc.BadgeRequestID = requestId;
                        string jsnString = hc.ToJSON();

                        var digest = encryptDecryptObj.MD5Hash(jsnString); // makes a digest
                        string sign = encryptDecryptObj.EncryptDigestwithBAPrivateKey(digest, BAPvrKey); //signs the digest with the badge authority private key

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["BadgeProviderService"].ToString() + "/GetValidationInfo");
                        string parameters = parameters = "{\"BadgeRequestID\": \"" + requestId + "\",\"Signature\": \"" + sign + "\"}"; ; //jsonString; //
                        request.Method = "POST";
                        request.ContentLength = 0;
                        request.ContentType = "application/json";
                        if (!string.IsNullOrEmpty(parameters))
                        {
                            byte[] byteArray = Encoding.UTF8.GetBytes(parameters);
                            request.ContentLength = byteArray.Length;
                            Stream dataStream = request.GetRequestStream();
                            dataStream.Write(byteArray, 0, byteArray.Length);
                            dataStream.Close();
                        }
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        var encoding = ASCIIEncoding.ASCII;
                        string responseText;
                        using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
                        {
                            responseText = reader.ReadToEnd();
                        }
                        return responseText; // (new JavaScriptSerializer().Serialize(responseText));
                    }
                }
                catch (Exception ex)
                {
                    return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
                }
            }
            return (new JavaScriptSerializer().Serialize("Error: No Response"));
        }


        //POST
        /// <summary>
        /// SubmitBadgeVerificationInfo 
        /// </summary>
        /// <param name="BadgeRequestID"></param>
        /// <param name="EncryptedJSON"></param>
        /// <returns></returns>        
        public string POST(BadgeInformationModel model)
        {
            encryptDecryptObj = new EncryptionAndDecryption();
            string BadgeReqID = Encoding.UTF8.GetString(Convert.FromBase64String(model.BadgeRequestID));
            BadgeInformationModel objBadgeInfo = new BadgeInformationModel();
            objBadgeInfo.BadgeRequestID = BadgeReqID;
            objBadgeInfo.EncryptedJSON = model.EncryptedJSON;
            string jsnString = objBadgeInfo.ToJSON();

            var digest = encryptDecryptObj.MD5Hash(jsnString); // makes a digest
            string sign = encryptDecryptObj.EncryptDigestwithBAPrivateKey(digest, BAPvrKey); //signs the digest with the badge authority private key

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["BadgeProviderService"].ToString() + "/VerifyBadge");
            string parameters = parameters = "{\"BadgeRequestID\": \"" + BadgeReqID + "\",\"data\": \"" + model.EncryptedJSON + "\",\"Signature\": \"" + sign + "\"}";//"{\"BadgeRequestID\": \"" + BadgeReqID + "\",\"Signature\": \"" + sign + "\"}";

            request.Method = "POST";
            request.ContentLength = 0;
            request.ContentType = "application/json";
            if (!string.IsNullOrEmpty(parameters))
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(parameters);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var encoding = ASCIIEncoding.ASCII;
            string responseText;
            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
            {
                responseText = reader.ReadToEnd();
            }
            return responseText; // "resultType : accepted";
        }

    }
}
