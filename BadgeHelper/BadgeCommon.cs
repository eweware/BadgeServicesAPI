using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace BadgeHelper
{
   public class BadgeCommon
    {
        public string GetJsonFromDataTable(DataTable dt)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row = null;

            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName.Trim(), dr[col]);
                }
                rows.Add(row);
            }
            return serializer.Serialize(rows);
        }

        public string GetJsonFromDataSet(DataSet ds)
        {
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                Dictionary<string, object> row = null;

                foreach (DataRow dr in dt.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        row.Add(col.ColumnName.Trim(), dr[col]);
                    }
                    rows.Add(row);
                }
                return serializer.Serialize(rows);
            }
            return null;
        }


        public string GenerateRandomCode(int size)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            StringBuilder builder = new StringBuilder();
            string allowedNumericChars = "0,1,2,3,4,5,6,7,8,9";
            char[] sep = { ',' };
            string[] AlphaChars = allowedNumericChars.Split(sep);
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                //builder.Append(ch);
                builder.Append(string.Concat(ch, AlphaChars[random.Next(0, AlphaChars.Length)]));
            }

            return builder.ToString();
            //return code;
        }

        public bool SendEmailMethod(string content, string FromId, string FromPass, string ToId)
        {
            MailMessage mailmsg;
            SmtpClient smtp;
            try
            {
                mailmsg = new MailMessage();
                mailmsg.To.Add(ToId);
                mailmsg.From = new MailAddress(FromId);
                mailmsg.Subject = "Verification Code";
                mailmsg.Body = content;
                AlternateView htmlview = AlternateView.CreateAlternateViewFromString(content, null, MediaTypeNames.Text.Html);

                mailmsg.AlternateViews.Add(htmlview);
                mailmsg.IsBodyHtml = true;
                smtp = new SmtpClient();
                smtp.Port = 587; //25; // 
                smtp.Host = "smtp.gmail.com"; //"elevatebizsolutions.com"; //
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential(FromId, FromPass);
                smtp.EnableSsl = true;
                smtp.Send(mailmsg);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
