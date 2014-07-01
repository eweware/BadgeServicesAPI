using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using BadgeHelper;

namespace BadgeService.Controllers
{
    [EnableCors("*", "*", "*")]
    public class sampleController : ApiController
    {
        // GET: api/sample
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET: api/sample/5
       [HttpGet]
        public string Get(string id)
        {
            BadgeCommon objBadgeCommon;
            SqlParameter[] Parm = new SqlParameter[1];
            Parm[0] = new SqlParameter("@BadgeRequestID", id);
            DataSet DataSetTemp = new DataSet();
            SQLHelper.FillDataset(SQLHelper.ConnectionString, CommandType.StoredProcedure, "SP_GetBadgeRequestStatus", DataSetTemp, new string[1] { "tblResult" }, Parm);

            objBadgeCommon = new BadgeCommon();
            return objBadgeCommon.GetJsonFromDataSet(DataSetTemp);
        }

        // POST: api/sample
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/sample/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/sample/5
        public void Delete(int id)
        {
        }
    }
}
