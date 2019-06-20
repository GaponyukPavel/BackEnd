using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Threading;
using System.Security.Cryptography;


namespace BackEnd
{
    class SqlBd
    {
        private SqlConnection SqlConn = new SqlConnection("Data Source = Pasha;Initial Catalog=BookShop;Integrated Security=true;");
        private SqlCommand SqlComm;
        string Hash = "";
        
        public SqlBd()
        {
            SqlComm = SqlConn.CreateCommand();
        }
        
        public void AddUser(string mail, string nick, string pass, Cookies cookie)
        {
            CheckForSymbols(6, 40, pass, mail, nick);
            mail = mail.ToLower();
            nick = nick.ToLower();
            IsRepeated(mail, nick);
            HashPass(pass);
            SqlComm.CommandText = string.Format("Insert users Values('{0}','{1}','{2}','{3}')", mail, nick, Hash,cookie.Value);
            SqlConn.Open();
            try
            {
                SqlComm.ExecuteNonQuery();
            }
            catch
            {
                throw new Exception("Ошибка!Данные введены неверно либо сервер не доступен!");
            }
            finally
            {
                SqlConn.Close();
            }
        }
        public void Login(string email,string pass, Cookies cookie)
        {
            CheckForSymbols(6,40,pass,email);
            email = email.ToLower();
            HashPass(pass);
            SqlComm.CommandText =string.Format( "Select Email,Password From Users Where Email ='{0}' AND Password ='{1}'",email,Hash);
            SqlConn.Open();
            SqlDataReader sqlReader = SqlComm.ExecuteReader();
            while (sqlReader.Read())
            {
                if (sqlReader.GetString(0)==email||sqlReader.GetString(1)==Hash)
                {
                    sqlReader.Close();
                    SqlComm.CommandText = String.Format("Update Users Set logcookie='{0}' Where Email='{1}' and Password='{2}'",cookie.Value,email,Hash);
                    SqlComm.ExecuteNonQuery();
                    SqlConn.Close();
                    return;
                }
            }
            SqlConn.Close();
            throw new Exception("Ошибка авторизации.Возможно, Вы неправильно указали Почту или пароль.");
        }
        public bool CheckCookie(string CookieValue)
        {
            SqlComm.CommandText =string.Format("Select email From Users Where LogCookie='{0}';",CookieValue);
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
        /// Check all for size and null, params cheked for special symbols
        /// </summary>
        /// <param name="minsize"></param>
        /// <param name="maxsize"></param>
        /// <param name="pass"></param>
        /// <param name="strings"></param>
        void CheckForSymbols(int minsize, int maxsize, string pass,params string[] strings)
        {

            if (pass.Length <= minsize || pass.Length >= maxsize|| pass==null)
            {
                throw new Exception("Неверный размер даных.");
            }
            foreach (string value in strings)
            {
                if (value.Length <= minsize || value.Length >= maxsize ||value == null)
                {
                    throw new Exception("Неверный размер даных.");
                }
                Regex regex = new Regex(@"^[^\W\s]+@?[^\W\s]+[.]?[^\W\s]+$");
                if (!regex.IsMatch(value))
                {
                    throw new Exception("Значения содержат не алфавитно цифровые значения!");
                }
            }
        }
        void IsRepeated(string mail, string name)
        {
            SqlConn.Open();
            SqlComm.CommandText = string.Format("Select Email from Users Where Email = '{0}' or NickName = '{1}';", mail, name);
            object Request = SqlComm.ExecuteScalar();
            SqlConn.Close();
            if (Request != null)
            {
                throw new Exception((string)Request == mail ? "Этот email уже использован" : "Это имя уже используется");
            }
        }
        void HashPass (string pass)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] ByteHash = md5.ComputeHash(Encoding.UTF8.GetBytes(pass+"Some Value"));
                foreach (var value in ByteHash)
                {
                    Hash +=value.ToString("x2");
                }
            }
        }
    }
}
