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
    public class BadgeTypeController : ApiController
    {
        BadgeCommon objBadgeCommon;
        EncryptionAndDecryption encryptDecryptObj;
        string BAPubKey = ConfigurationManager.AppSettings["BAPubKey"];
        string BPPvrKey = ConfigurationManager.AppSettings["BPPvrKey"];
        byte[] BAPvrKeybytes = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BPPvrKey"]);
        byte[] iv = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BPPvrKey"]);


        //GET-GetBadgeTypes

        /// <summary>
        /// Returns a list of all of the badge types this authority can issue.       
        /// </summary>
        /// <returns></returns>

      [ActionName("GetBadgeTypes")]
        public string Get()
        {
            try
            {
                DataSet DataSetTemp = new DataSet();
                SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_GetBadgeTypes", DataSetTemp, new string[1] { "tblResult" });

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
        /// GetBadgeTypeInfo - Returns information about a single badge type.
        /// </summary>
        /// <returns>Returns information about a single badge type.</returns>
        public string Get(string id)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    SqlParameter[] Parm = new SqlParameter[1];
                    Parm[0] = new SqlParameter("@BadgeID", id);
                    DataSet DataSetTemp = new DataSet();
                    SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_GetBadgeTypeInfo", DataSetTemp, new string[1] { "tblResult" }, Parm);

                    objBadgeCommon = new BadgeCommon();
                    return objBadgeCommon.GetJsonFromDataSet(DataSetTemp);
                }
            }
            catch (Exception ex)
            {
                return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
            }
            return (new JavaScriptSerializer().Serialize("Error: Invalid Badge Type ID"));
        }

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

    }
}
