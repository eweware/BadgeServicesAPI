using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
    public class BadgeConsumerController : ApiController
    {
        BadgeCommon objBadgeCommon;
        EncryptionAndDecryption encryptDecryptObj;
        string BPPubKey = ConfigurationManager.AppSettings["BPPubKey"];
        string BAPvrKey = ConfigurationManager.AppSettings["BAPvrKey"];
        byte[] BAPvrKeybytes = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BAPvrKey"]);
        byte[] iv = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BAPvrKey"]);

        #region BadgeConsumer
      

        #region BadgeConsumer

        //POST
        /// <summary>
        /// RegisterBadgeConsumer 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="endpoint"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        //[ActionName("BadgeConsumer")]RegisterBadgeConsumer
        public string POST(BadgeConsumerModel consumermodel)  //public string RegisterBadgeConsumer(string Name, string EndPoint, string Key)
        {
            try
            {
                encryptDecryptObj = new EncryptionAndDecryption();

                //DECRYPT FROM CRIPTOJS
                byte[] data = Convert.FromBase64String(consumermodel.name);
                string decodedString = Encoding.UTF8.GetString(data);
                var encrypted = Convert.FromBase64String(decodedString);
                string dName = encryptDecryptObj.DecryptStringFromBytes(encrypted, BAPvrKeybytes, iv);

                data = Convert.FromBase64String(consumermodel.endpoint);
                decodedString = Encoding.UTF8.GetString(data);
                encrypted = Convert.FromBase64String(decodedString);
                string dendPoint = encryptDecryptObj.DecryptStringFromBytes(encrypted, BAPvrKeybytes, iv);

                data = Convert.FromBase64String(consumermodel.key);
                decodedString = Encoding.UTF8.GetString(data);
                encrypted = Convert.FromBase64String(decodedString);
                string dKey = encryptDecryptObj.DecryptStringFromBytes(encrypted, BAPvrKeybytes, iv);

                SqlParameter[] Parm = new SqlParameter[3];
                Parm[0] = new SqlParameter("@Name", dName);
                Parm[1] = new SqlParameter("@endPoint", dendPoint);
                Parm[2] = new SqlParameter("@Key", dKey);
                DataSet DataSetTemp = new DataSet();
                SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_RegisterBadgeConsumer", DataSetTemp, new string[1] { "tblResult" }, Parm);

                objBadgeCommon = new BadgeCommon();
                return objBadgeCommon.GetJsonFromDataSet(DataSetTemp);
            }
            catch (Exception ex)
            {
                return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data)); //ex.Message.ToString();
            }
        }

        //DELETE 
        /// <summary>
        /// UnregisterBadgeConsumer
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="signature"></param>
        /// <returns></returns>       
        //UnregisterBadgeConsumer
        public string DELETE(BadgeConsumerModel consumermodel)
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
                            Parm = new SqlParameter[1];
                            Parm[0] = new SqlParameter("@ConsumerID", dConsumerID);
                            DataSet DataSetTemp = new DataSet();
                            SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_UnregisterBadgeConsumer", DataSetTemp, new string[1] { "tblResult" }, Parm);

                            objBadgeCommon = new BadgeCommon();
                            return objBadgeCommon.GetJsonFromDataSet(DataSetTemp);
                        }
                    }
                    else return (new JavaScriptSerializer().Serialize("Error: Invalid ConsumerId"));

                }
            }
            catch (Exception ex)
            {
                return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
            }
            return (new JavaScriptSerializer().Serialize("Error: Invalid Signature"));
        }

        #endregion
        #endregion
    }
}
