using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd
{
    class CookiesClass
    {
        public Cookie MyCookie;
        public string LoginValue;

        public CookiesClass()
        { }
        /// <summary>
        /// try to login user and create log cookie
        /// </summary>
        public CookiesClass(string Mail, string Pass, string UserAgent)
        {
            Mail = Mail ?? "";
            Mail = Mail.ToLower();
            Pass = Pass ?? "";
            UserAgent = UserAgent ?? "";
            CreateLogCockie(Mail,Pass,UserAgent);
            new Authentication().Login(Mail,Pass, this);
        }
        void CreateLogCockie(string Mail,string Pass,string UserAgent)
        {
            MyCookie = new Cookie("LogCookie", Authentication.Hash(Pass + Mail));
            LoginValue = Authentication.Hash(MyCookie.Value+ UserAgent);
        }
        //public void CartCookie(HttpListenerRequest HRequest, HttpListenerResponse HResponse)
        //{
        //    string id = "";
        //    id = HRequest.UrlReferrer.Segments[2].Replace(".html", "");
        //    if (id == "")
        //        return;
        //    if (HRequest.Cookies["CartCookie"] == null)
        //    {
        //        MyCookie = new Cookie("CartCookie", id+".","/");
        //        HRequest.Cookies.Add(MyCookie);
        //    }
        //    else
        //    {
        //        MyCookie = new Cookie("CartCookie", HRequest.Cookies["CartCookie"].Value+ id + ".", "/");
        //        HRequest.Cookies["CartCookie"].Value = MyCookie.Value;
        //    }
        //    HResponse.Cookies.Add(MyCookie);
        //}
        /// <summary>
        /// Check requester for logcookie
        /// </summary>
        /// <param name="HRequest"></param>
        /// <param name="role">if user login return his role otherwise empty string</param>
        /// <param name="email">if user email return his role otherwise empty string</param>
        /// <returns></returns>
        public static bool CheckForLogin(HttpListenerRequest HRequest,out string role, out string email)
        {
            email = "";
            role = "";
            if (HRequest.Cookies["logCookie"]==null || string.IsNullOrEmpty(HRequest.Cookies["logCookie"].Value))
                return false;

            string CookieValue = HRequest.Cookies["logCookie"].Value;
            string UserAgent = HRequest.UserAgent;

            UserAgent = UserAgent ?? "";
            
            if (CheckCookie(CookieValue, UserAgent,ref role, ref email))
                return true;
            Console.WriteLine("Пользователь не авторизирован");
            return false;
        }
        /// <summary>
        /// Search user in DB if found return true and his role and email otherwise return false and empty strings
        /// </summary>
        /// <param name="CookieValue"></param>
        /// <param name="UserAgent"></param>
        /// <param name="role">If found user return user role otherwise empty string</param>
        /// <param name="email">If found user return user email otherwise empty string</param>
        /// <returns></returns>
        static bool CheckCookie(string CookieValue, string UserAgent,ref string role,ref string email)
        {
            CookieValue= Authentication.Hash(CookieValue + UserAgent);
            SqlConnection SqlConn = new SqlConnection("Data source=Pasha;Initial Catalog=BookShop;Integrated Security=true;");
            SqlCommand SqlComm = SqlConn.CreateCommand();
            SqlComm.CommandText = string.Format("Select role,email From Users Where LogCookie='{0}';", CookieValue);
            SqlConn.Open();
            SqlDataReader Reader = SqlComm.ExecuteReader();
            if (Reader.Read())
            {
                role =(string) Reader["role"];
                email =(string) Reader["email"];
                role = role ?? "";
                email = email ?? "";
                Reader.Close();
                SqlConn.Close();
                return true;
            }
            role = "";
            email = "";
            Reader.Close();
            SqlConn.Close();
            return false;
        }
        /// <summary>
        /// return logged user id otherwise 0
        /// </summary>
        /// <param name="Hrequest"></param>
        /// <returns></returns>
        public static int GetUserIdByRequest(HttpListenerRequest Hrequest)
        {
            string CookieValue = Hrequest.Cookies["logCookie"].Value;
            if (CookieValue != null)
            {
                CookieValue = Authentication.Hash(CookieValue + Hrequest.UserAgent);
                SqlConnection SqlConn = new SqlConnection("Data source=Pasha;Initial Catalog=BookShop;Integrated Security=true;");
                SqlCommand SqlComm = SqlConn.CreateCommand();
                SqlComm.CommandText = string.Format($"Select UserId From Users Where LogCookie='{CookieValue}';");
                SqlConn.Open();
                int Id = (int)SqlComm.ExecuteScalar();
                SqlConn.Close();
                return Id;                
            }
            return 0;
        }

    }
}
