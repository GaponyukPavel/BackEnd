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
    class Cookies
    {
        public Cookie Cookie;
        public string Value;

        public Cookies()
        { }

        public Cookies(string Mail, string Pass, string UserAgent)
        {
            Mail = Mail == null ? "" : Mail;
            Mail = Mail.ToLower();
            Pass = Pass == null ? "" : Pass;
            UserAgent = UserAgent == null ? "" : UserAgent;
            CreateCockie(Mail,Pass,UserAgent);
        }
        
        void CreateCockie(string Mail,string Pass,string UserAgent)
        {
            Console.WriteLine($"COOKIE:\n{Mail}\npass:{Pass}\n{UserAgent}");
            Pass =  Hash(Pass);
            Console.WriteLine($"Hash pass {Pass}");
            Cookie = new Cookie("LogCookie", Hash(Pass + Mail));
            Value = Hash(Cookie.Value+ UserAgent);
            Console.WriteLine("{0}\nзначение в программе : {1}\nзначение в куки : {2}\n{0}",new string('-', 40),Cookie.Value,Value);
        }
        public Cookie CreateCockieWithHashPass(string Mail, string HashPass, string UserAgent)
        {
            Console.WriteLine($"COOKIE:\n{Mail}\nHash pass: {HashPass}\n{UserAgent}");
            Cookie = new Cookie("LogCookie", Hash(HashPass + Mail));
            return Cookie;
        }
        static string Hash(string ValueToHash)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] ByteHash = md5.ComputeHash(Encoding.UTF8.GetBytes(ValueToHash));
                ValueToHash = "";
                foreach (var value in ByteHash)
                {
                    ValueToHash += value.ToString("x2");
                }
                return ValueToHash;
            }
        }
        /// <summary>
        /// Проверка на наличи logcookie
        /// </summary>
        /// <param name="ReqestCookies">Cookie запроса</param>
        /// <param name="HResponse">Используется для отправки нового куки</param>
        /// <param name="UserAgent">Информация о польователе, используется для подсчета cookie hash</param>
        /// <returns></returns>
        public static bool CheckForLogin(HttpListenerRequest HRequest)
        {
            string CookieValue = HRequest.Cookies["logCookie"].Value;
            string UserAgent = HRequest.UserAgent;
            if (UserAgent == null)
                UserAgent = "";
            if (CookieValue != null)
            {
                if (CheckCookie(CookieValue, UserAgent))
                    return true;
            }
            Console.WriteLine("НЕТ КУКИ!!!!");
            return false;
        }
        static bool CheckCookie(string CookieValue, string UserAgent)
        {
            CookieValue= Hash(CookieValue + UserAgent);
            SqlConnection SqlConn = new SqlConnection("Data source=Pasha;Initial Catalog=BookShop;Integrated Security=true;");
            SqlCommand SqlComm = SqlConn.CreateCommand();
            SqlComm.CommandText = string.Format("Select email From Users Where LogCookie='{0}';", CookieValue);
            SqlConn.Open();
            if (SqlComm.ExecuteScalar() != null)
            {
                SqlConn.Close();
                return true;
            }
            SqlConn.Close();
            return false;
        }
        /// <summary>
        /// Возврашает nickname пользователя из запроса или 0 если он не авторизирован
        /// </summary>
        /// <param name="Hrequest"></param>
        /// <returns></returns>
        public static int GetUserIdByRequest(HttpListenerRequest Hrequest)
        {
            string CookieValue = Hrequest.Cookies["logCookie"].Value;
            if (CookieValue != null)
            {
                CookieValue = Hash(CookieValue + Hrequest.UserAgent);
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
