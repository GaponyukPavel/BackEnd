using System;
using System.Collections.Generic;
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
        
        public Cookies(string Mail, string Pass, string UserAgent)
        {
            if (UserAgent == null)
                UserAgent = "";
            CreateCockie(Mail,Pass,UserAgent);
        }

        void CreateCockie(string Mail,string Pass,string UserAgent)
        {
            Pass =  Hash(Pass);
            Cookie = new Cookie("LogCookie", Hash(Pass + Mail));
            //Cookie.Expires = DateTime.Now.AddHours(1);
            Value = Hash(Cookie.Value+ UserAgent);
            Console.WriteLine("{0}\ncookie.value : {1}\nCookies.value : {2}\n{0}",new string('-', 40),Cookie.Value,Value);
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
        public static bool CheckForLogin(CookieCollection ReqestCookies, HttpListenerResponse HResponse,string UserAgent)
        {
            if (UserAgent == null)
                UserAgent = "";
            if (ReqestCookies["LogCookie"] != null)
            {
                if (new SqlBd().CheckCookie(Hash(ReqestCookies["LogCookie"].Value + UserAgent)))
                {
                   // ReqestCookies["LogCookie"].Expires = DateTime.Now.AddHours(1);
                    ReqestCookies["logCookie"].Path = "/";
                    HResponse.Cookies = ReqestCookies;
                    return true;
                }
            }
            Console.WriteLine("НЕТ КУКИ!!!!");
            return false;
        }

    }
}
