using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Script.Serialization;
using BadgeHelper;
namespace BadgeProvider.Controllers
{
    [EnableCors("*", "*", "*")]
    public class BadgeController : ApiController
    {
        BadgeCommon objBadgeCommon;
        EncryptionAndDecryption encryptDecryptObj;
        string BAPubKey = ConfigurationManager.AppSettings["BAPubKey"];
        string BPPvrKey = ConfigurationManager.AppSettings["BPPvrKey"];
        byte[] BAPvrKeybytes = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BPPvrKey"]);
        byte[] iv = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["BPPvrKey"]);

        /// <summary>
        /// Returns information about a single badgeId.
        /// </summary>
        /// <param name="BadgeId"></param>
        /// <returns></returns>
        public string Get(string id)
        {
            try
            {
                SqlParameter[] Parm = new SqlParameter[1];
                Parm[0] = new SqlParameter("@BadgeId", id);
                DataSet DataSetTemp = new DataSet();
                SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_GetBadgeInfo", DataSetTemp, new string[1] { "tblResult" }, Parm);

                objBadgeCommon = new BadgeCommon();
                return objBadgeCommon.GetJsonFromDataSet(DataSetTemp);
            }
            catch (Exception ex)
            {
                return (new JavaScriptSerializer().Serialize(ex.Message + "------" + ex.Data));
            }
        }

        //GET
        [ActionName("status")]
        public string Get()
        {
            return "status: OK";

        }


    }
}
