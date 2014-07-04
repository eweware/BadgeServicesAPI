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
    public class BadgeRequestController : ApiController
    {
        BadgeCommon objBadgeCommon;
        EncryptionAndDecryption encryptDecryptObj;
        string BPPubKey = ConfigurationManager.AppSettings["BPPubKey"];
        string BAPvrKey = ConfigurationManager.AppSettings["BAPvrKey"];
        byte[] BAPvrKeybytes = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BAPvrKey"]);
        byte[] iv = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BAPvrKey"]);



        //GET
        /// <summary>
        /// GetBadgeRequest
        /// </summary>
        /// <param name="id">BadgeRequestID</param>
        /// <returns>status</returns>
         //[Route("BadgeAuthority/BadgeRequest/{id}")]
        [ActionName("GetBadgeRequestById")]
        public string GET(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                SqlParameter[] Parm = new SqlParameter[1];
                Parm[0] = new SqlParameter("@BadgeRequestID", id);
                DataSet DataSetTemp = new DataSet();
                SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_GetBadgeRequestStatus", DataSetTemp, new string[1] { "tblResult" }, Parm);

                objBadgeCommon = new BadgeCommon();
                return objBadgeCommon.GetJsonFromDataSet(DataSetTemp);
            }
            return (new JavaScriptSerializer().Serialize("Error: Invalid Request ID"));
        }


        //POST
        /// <summary>
        /// StartBadgeRequest
        /// </summary>
        /// <param name="consumer">the id of the BadgeConsumer</param>
        /// <param name="issuer">the id of the issuer</param>
        /// <param name="signature">the signed digest of the request</param>
        /// <returns></returns>


        public string POST(BadgeConsumerModel consumermodel)
        {
            bool isValidSignature = false;
            try
            {
                //check the signature for correctness :create a digest for the data, and then decrypt the signature 
                //with the public key it has in the database for this consumer and see if the signature matches
                encryptDecryptObj = new EncryptionAndDecryption();
                byte[] data = Convert.FromBase64String(consumermodel.consumer);
                string dConsumerID = Encoding.UTF8.GetString(data);

                DataSet DSTemp = new DataSet();
                SqlParameter[] Parm = new SqlParameter[1];
                Parm[0] = new SqlParameter("@ConsumerID", dConsumerID);
                SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_GetConsumerPublicKey", DSTemp, new string[1] { "tblResult" }, Parm);

                if (DSTemp != null && DSTemp.Tables.Count > 0)
                {
                    if (DSTemp.Tables[0].Rows.Count > 0)
                    {
                        string BCPubKey = DSTemp.Tables[0].Rows[0]["PublicKey"].ToString();
                        byte[] BCPubKeybytes = Encoding.UTF8.GetBytes(BCPubKey);

                        data = Convert.FromBase64String(consumermodel.issuer);
                        string dProviderID = Encoding.UTF8.GetString(data);

                        BadgeInformationModel hc1 = new BadgeInformationModel(); //request ID (param received) wrap it in a data member
                        hc1.ConsumerId = dConsumerID;
                        hc1.ProviderID = dProviderID;
                        string jsnString = hc1.ToJSON();
                        var digest1 = encryptDecryptObj.MD5Hash(Convert.ToBase64String(Encoding.UTF8.GetBytes(System.Text.RegularExpressions.Regex.Replace(jsnString, @"\\", "/")))); // makes a digest

                        //string sign1 = DecryptString(Signature, BCPubKey);

                        data = Convert.FromBase64String(consumermodel.signature);
                        string decodedString = Encoding.UTF8.GetString(data);
                        var encrypted = Convert.FromBase64String(decodedString);
                        string dSignature = encryptDecryptObj.DecryptStringFromBytes(encrypted, BCPubKeybytes, iv);

                        isValidSignature = (!string.IsNullOrWhiteSpace(dSignature)) ? true : false;

                        DataSet DataSetTemp = new DataSet();

                        if (isValidSignature)
                        {
                            SqlParameter[] Parm1 = new SqlParameter[2];
                            Parm1[0] = new SqlParameter("@ConsumerID", dConsumerID);
                            Parm1[1] = new SqlParameter("@ProviderID", dProviderID);
                            SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_InsertBadgeRequest", DataSetTemp, new string[1] { "tblResult" }, Parm1);

                            if (DataSetTemp != null && DataSetTemp.Tables.Count > 0)
                            {
                                string val = DataSetTemp.Tables[0].Rows[0]["BadgeRequestID"].ToString();
                                BadgeInformationModel hc = new BadgeInformationModel();
                                hc.BadgeRequestID = val;
                                hc.Signature = consumermodel.signature;
                                string jsonString = hc.ToJSON();

                                var digest = encryptDecryptObj.MD5Hash(jsnString); // makes a digest
                                string sign = encryptDecryptObj.EncryptDigestwithBAPrivateKey(digest, BAPvrKey); //signs the digest with the badge authority private key

                                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["BadgeProviderService"].ToString() +"/BadgeRequest");
                                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:1730/BadgeProviderService.svc/StartBadgeRequest");
                                string parameters = parameters = "{\"id\": \"" + val + "\",\"signature\": \"" + sign + "\"}"; ; //jsonString; //
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
                                return responseText;
                            }
                        }
                    }
                    else
                    {
                        return (new JavaScriptSerializer().Serialize("Error: Invalid ConsumerId"));
                    }
                    //objBadgeCommon = new BadgeCommon();
                    //return objBadgeCommon.GetJsonFromDataSet(DataSetTemp);
                }

            }
            catch (Exception ex)
            {
                return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
            }
            return (new JavaScriptSerializer().Serialize("Error: Invalid Request"));
        }

        //POST
        /// <summary>
        /// CancelBadgeRequest
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="signature"></param>        
        public void DELETE(BadgeConsumerModel consumermodel)
        {
            try
            {
                bool isValidSignature = false;
                byte[] data = Convert.FromBase64String(consumermodel.consumer);
                string dConsumerID = Encoding.UTF8.GetString(data);

                DataSet DSTemp = new DataSet();
                SqlParameter[] Parm = new SqlParameter[1];
                Parm[0] = new SqlParameter("@ConsumerID", dConsumerID);
                SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_GetConsumerPublicKey", DSTemp, new string[1] { "tblResult" }, Parm);

                if (DSTemp != null && DSTemp.Tables.Count > 0)
                {
                    if (DSTemp.Tables[0].Rows.Count > 0)
                    {
                        byte[] BCPubKeybytes = Encoding.UTF8.GetBytes(DSTemp.Tables[0].Rows[0]["PublicKey"].ToString());
                        encryptDecryptObj = new EncryptionAndDecryption();
                        data = Convert.FromBase64String(consumermodel.signature);
                        string decodedString = Encoding.UTF8.GetString(data);
                        var encrypted = Convert.FromBase64String(decodedString);
                        string dSignature = encryptDecryptObj.DecryptStringFromBytes(encrypted, BCPubKeybytes, iv);

                        isValidSignature = (!string.IsNullOrWhiteSpace(dSignature)) ? true : false;

                        if (isValidSignature)
                        {
                            encryptDecryptObj = new EncryptionAndDecryption();
                            BadgeInformationModel hc = new BadgeInformationModel();
                            hc.ConsumerId = dConsumerID;
                            string jsnString = hc.ToJSON();

                            var digest = encryptDecryptObj.MD5Hash(jsnString); // makes a digest
                            string sign = encryptDecryptObj.EncryptDigestwithBAPrivateKey(digest, BAPvrKey); //signs the digest with the badge authority private key

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["BadgeProviderService"] + "/CancelBadgeRequest");
                            string parameters = parameters = "{\"ID\": \"" + dConsumerID + "\",\"Signature\": \"" + sign + "\"}";
                            request.Method = "POST"; // "DELETE";
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
                            //return responseText;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
            }
            //return (new JavaScriptSerializer().Serialize("Error: Invalid Signature"));
        }

        //PUT
        /// <summary>
        /// VerifyBadgeRequest 
        /// </summary>
        /// <param name="BadgeRequestID"></param>
        /// <param name="BadgeList"></param>
        /// <param name="Signature"></param>
        /// <returns></returns>
        public string PUT(BadgesList model)
        {
            if (!string.IsNullOrWhiteSpace(model.Signature))
            {
                encryptDecryptObj = new EncryptionAndDecryption();
                string sign = encryptDecryptObj.DecryptString(model.Signature, BPPubKey);

                if (sign.Equals(model.Signature))
                {
                    string jsnString = model.BadgeList.ToJSON();

                    var digest = encryptDecryptObj.MD5Hash(jsnString); // makes a digest
                    sign = encryptDecryptObj.EncryptDigestwithBAPrivateKey(digest, BAPvrKey);

                    BadgesList badgeObj = new BadgesList();
                    badgeObj.BadgeRequestID = model.BadgeRequestID;
                    badgeObj.BadgeList = model.BadgeList;
                    badgeObj.Signature = sign;

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["BadgeConsumerService"].ToString() + "/CompleteBadgeRequest");
                    //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:1728/BadgeConsumerService.svc/CompleteBadgeRequest");
                    string parameters = badgeObj.ToJSON(); //parameters = "{\"BadgeRequestID\": \"" + BadgeRequestID + "\",\"BadgeList\": \"" + BadgeList + "\",\"Signature\": \"" + sign + "\"}";
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
                    return responseText;
                }
                else
                {
                    return (new JavaScriptSerializer().Serialize("Error: Invalid Signature"));
                }
            }
            else
            {
                return (new JavaScriptSerializer().Serialize("Error: Invalid Signature"));
            }
        }

    }
}
