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
using BadgeProvider.Models;

namespace BadgeProvider.Controllers
{
    [EnableCors("*", "*", "*")]
    public class BadgeVerificationInfoController : ApiController
    {
        BadgeCommon objBadgeCommon;
        EncryptionAndDecryption encryptDecryptObj;
        string BAPubKey = ConfigurationManager.AppSettings["BAPubKey"];
        string BPPvrKey = ConfigurationManager.AppSettings["BPPvrKey"];
        byte[] BPPvrKeybytes = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BPPvrKey"]);
        byte[] iv = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BPPvrKey"]);

        /// <summary>
        /// GetValidationInfo - This method is called by the badge authority to get the validation info for the request.  
        /// This method may be called multiple times, depending on how many validation steps are required.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public string Get(BadgeVerificationModel model)
        {
            try
            {
                encryptDecryptObj = new EncryptionAndDecryption();
                string sign = encryptDecryptObj.DecryptString(model.signature, BAPubKey);

                if (sign.Equals(model.signature))
                {
                    DataSet DataSetTemp = new DataSet();
                    SqlParameter[] Parm = new SqlParameter[1];
                    Parm[0] = new SqlParameter("@BadgeRequestID", model.requestId);

                    SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_GetBadgeVerificationInformation", DataSetTemp, new string[1] { "tblResult" }, Parm);

                    objBadgeCommon = new BadgeCommon();
                    return objBadgeCommon.GetJsonFromDataSet(DataSetTemp);
                }
            }
            catch (Exception ex)
            {
                return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
            }
            return (new JavaScriptSerializer().Serialize("Error: Invalid Signature"));
        }


        //POST - VerifyBadge

        // <summary>
        /// VerifyBadge - This method is called from the badge authority to pass the encrypted verification data to the issuer.  
        /// The issuer should verify the data and return either “accepted” if it is OK, “rejected” if it fails, or “inprocess”
        /// if an additional validation request is required.
        /// </summary>
        /// <param name="BadgeRequestID"></param>
        /// <param name="Signature"></param>
        /// <returns></returns>
        public string POST(BadgeVerificationModel model)
        {
            try
            {
                //need to get these two "isConfirmationCodeReceived" and "cCode" params from the front end so that one can proceed further.
                //if isConfirmationCodeReceived = true then the ConfCode has been sent to the receiver email.
                //if isConfirmationCodeReceived = false then the ConfCode needs to be sent to the receiver email.
                //cCode will come from the
                bool isConfirmationCodeReceived = true;
                string cCode = "O0P5I8";
                if (!string.IsNullOrWhiteSpace(model.signature))
                {
                    encryptDecryptObj = new EncryptionAndDecryption();
                    string sign = encryptDecryptObj.DecryptString(model.signature, BAPubKey);

                    if (sign.Equals(model.signature))
                    {
                        if (!isConfirmationCodeReceived)
                        {
                            bool IsEmailSent = false;
                            objBadgeCommon = new BadgeCommon();
                            byte[] encrpData = Convert.FromBase64String(model.data);
                            string decodedString = Encoding.UTF8.GetString(encrpData);
                            var encrypted = Convert.FromBase64String(decodedString);
                            string dEmail = encryptDecryptObj.DecryptStringFromBytes(encrypted, BPPvrKeybytes, iv);

                            string randCode = objBadgeCommon.GenerateRandomCode(3);

                            DataSet DataSetTemp = new DataSet();
                            SqlParameter[] Parm = new SqlParameter[2];
                            Parm[0] = new SqlParameter("@BadgeRequestID", model.requestId);
                            Parm[1] = new SqlParameter("@CCode", randCode);

                            SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_InsertBadgeRequestConfirmationCode", DataSetTemp, new string[1] { "tblResult" }, Parm);

                            if (DataSetTemp != null && DataSetTemp.Tables.Count > 0)
                            {
                                string res = DataSetTemp.Tables[0].Rows[0]["ResultType"].ToString();
                                if (!string.IsNullOrWhiteSpace(res) || !res.Contains("Cancelled"))
                                {
                                    IsEmailSent = objBadgeCommon.SendEmailMethod("Code Generated: " + randCode, "azure.service.app@gmail.com", "azure@123", dEmail);
                                }
                            }
                            if (IsEmailSent)
                                return (new JavaScriptSerializer().Serialize("In Process: Code successfully sent to the provided email."));
                            else
                                return (new JavaScriptSerializer().Serialize("Error: Invalid email"));
                        }
                        else
                        {
                            DataSet DataSetTemp = new DataSet();
                            SqlParameter[] Parm = new SqlParameter[2];
                            Parm[0] = new SqlParameter("@BadgeRequestID", model.requestId);
                            Parm[1] = new SqlParameter("@CCode", cCode);

                            SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_MatchConfirmationCode", DataSetTemp, new string[1] { "tblResult" }, Parm);

                            if (DataSetTemp != null && DataSetTemp.Tables.Count > 0)
                            {
                                if (DataSetTemp.Tables[0].Rows[0]["ResultType"].ToString().Equals("Accepted"))
                                {
                                    int BadgeProviderID = Convert.ToInt32(DataSetTemp.Tables[0].Rows[0]["BadgeProviderID"]);
                                    DataSetTemp = new DataSet();
                                    Parm = new SqlParameter[1];
                                    Parm[0] = new SqlParameter("@BadgeProviderID", BadgeProviderID);

                                    SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_GetApprovedBadges", DataSetTemp, new string[1] { "tblResult" }, Parm);

                                    List<Badges> lstBadges = new List<Badges>();
                                    foreach (DataRow dr in DataSetTemp.Tables[0].Rows)
                                    {
                                        lstBadges.Add(new Badges
                                        {
                                            BadgeID = Convert.ToString(dr["BadgeID"]),
                                            Name = Convert.ToString(dr["Name"]),
                                            Image = Convert.ToString(dr["Image"]),
                                            ExpiryDate = Convert.ToString(dr["ExpiryDate"]),
                                            Description = Convert.ToString(dr["Description"]),
                                        });
                                    }

                                    string jsnString = lstBadges.ToJSON();
                                    var digest = encryptDecryptObj.MD5Hash(jsnString);
                                    string signed = encryptDecryptObj.EncryptDigestwithBAPrivateKey(digest, BPPvrKey);

                                    BadgesList badgeObj = new BadgesList();
                                    badgeObj.BadgeRequestID = model.requestId;
                                    badgeObj.BadgeList = lstBadges;
                                    badgeObj.Signature = signed;

                                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://projects.elevatebizsolutions.com/BadgeAuthorityService/BadgeAuthorityService.svc/VerifyBadgeRequest");
                                    //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:1727/BadgeAuthorityService.svc/VerifyBadgeRequest");
                                    string parameters = badgeObj.ToJSON();
                                    //string parameters = "{\"BadgeRequestID\": \"" + BadgeRequestID + "\",\"BadgeList\": \"" + jsnString + "\",\"Signature\": \"" + signed + "\"}";

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
                            else
                            {
                                return (new JavaScriptSerializer().Serialize("Error: Invalid Confirmation Code"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
            }
            return (new JavaScriptSerializer().Serialize("Error: Invalid Signature"));
        }
    }
}
