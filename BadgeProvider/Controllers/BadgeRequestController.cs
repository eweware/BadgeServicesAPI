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
    public class BadgeRequestController : ApiController
    {
        BadgeCommon objBadgeCommon;
        EncryptionAndDecryption encryptDecryptObj;
        string BAPubKey = ConfigurationManager.AppSettings["BAPubKey"];
        string BPPvrKey = ConfigurationManager.AppSettings["BPPvrKey"];
        byte[] BAPvrKeybytes = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BPPvrKey"]);
        byte[] iv = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BPPvrKey"]);

        /// <summary>
        /// StartBadgeRequest -This method is used to start a badge request.  This is called by the badge authority and is signed.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public string POST(BadgeRequestModel model)
        {
            bool isValidSignature = false;
            DataSet DataSetTemp = new DataSet();
            encryptDecryptObj = new EncryptionAndDecryption();
            try
            {
                string sign = encryptDecryptObj.DecryptString(model.signature, BAPubKey);

                if (sign.Equals(model.signature))
                    isValidSignature = true;

                if (isValidSignature)
                {
                    DataSetTemp = new DataSet();
                    SqlParameter[] Parm = new SqlParameter[2];
                    Parm[0] = new SqlParameter("@BadgeRequestID", model.id);
                    Parm[1] = new SqlParameter("@Status", "Stage 1");
                    SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_InsertBadgeRequestStatus", DataSetTemp, new string[1] { "tblResult" }, Parm);
                }
                objBadgeCommon = new BadgeCommon();
                return objBadgeCommon.GetJsonFromDataSet(DataSetTemp);
            }
            catch (Exception ex)
            {
                return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
            }
        }

         //GET 
      /// <summary>
        /// GetBadgeRequest -This method can be called to get the status of the badge request.  It can be “expired”, “rejected”, “approved”, “inprocess”
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
         public string Get(string id)
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
            return (new JavaScriptSerializer().Serialize("Error: Invalid RequestID"));
        }


         //DELETE
    
       /// <summary>
         /// CancelBadgeRequest- This method is used to cancel a badge request.  It is only called by the badge authority.
       /// </summary>
       /// <param name="id"></param>
       /// <param name="signature"></param>
         public void DELETE(BadgeRequestModel model)
         {
             try
             {
                 encryptDecryptObj = new EncryptionAndDecryption();
                 string sign = encryptDecryptObj.DecryptString(model.signature, BAPubKey);

                 if (sign.Equals(model.signature))
                 {
                     SqlParameter[] Parm = new SqlParameter[1];
                     Parm[0] = new SqlParameter("@ConsumerID", model.id);
                     DataSet DataSetTemp = new DataSet();
                     SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_CancelBadgeRequest", DataSetTemp, new string[1] { "tblResult" }, Parm);

                     //clsObj = new Class1();
                     //return clsObj.GetJsonFromDataSet(DataSetTemp);
                 }
             }
             catch (Exception ex)
             {
                 //return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
             }
             //return (new JavaScriptSerializer().Serialize("Invaid Signature Error"));
         }
    }
}
