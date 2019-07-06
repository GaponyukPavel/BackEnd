using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Threading;
using System.Security.Cryptography;
using System.Net.Mail;
using System.IO;
using System.Collections.Specialized;

namespace BackEnd
{
    class Authentication
    {
        private SqlConnection SqlConn = new SqlConnection("Data Source = Pasha;Initial Catalog=BookShop;Integrated Security=true;");
        private SqlCommand SqlComm;
        string PasswordHash = "";
        
        public Authentication()
        {
            SqlComm = SqlConn.CreateCommand();
        }
        
        public void AddUser(string mail, string nick, string pass, Cookies cookie)
        {
            MailAddress From = new MailAddress("BookShop43@mail.ru", "BookShop");//данные для письма
            MailAddress To = new MailAddress(mail);//данные для письма

            CheckForSymbols(6, 40, pass, mail, nick);
            mail = mail.ToLower();
            nick = nick.ToLower();
            IsRepeated(mail, nick);
            HashPass(pass);
            SqlComm.CommandText = string.Format("Insert users (Email,NickName,Password,LogCookie,confirmed) Values('{0}','{1}','{2}','{3}','{4}')", mail, nick, PasswordHash,cookie.Value,Hash(mail));
            SqlConn.Open();
            try
            {//отправляет письмо и сохраняет user в БД
                MailMessage mailMessage = new MailMessage(From, To);
                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = "Registration on BookShop";
                string page = "";
                using (StreamReader streamReader = new StreamReader(Environment.CurrentDirectory + "/web/Not/mail.html"))
                {
                    page = streamReader.ReadToEnd();
                    page = page.Replace("NAME", nick);
                    page = page.Replace("test", Hash(mail));
                }
                mailMessage.Body = page;
                SmtpClient smpt = new SmtpClient("smtp.mail.ru");
                smpt.Credentials = new NetworkCredential("BookShop43@mail.ru", "pahanich1");
                smpt.EnableSsl = true;
                SqlComm.ExecuteNonQuery();
                smpt.Send(mailMessage);
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
        public bool Confirmed(NameValueCollection value,HttpListenerResponse HResponse,HttpListenerRequest HRequest )
        {
            if (value["code"] == null)
                return false;
            SqlComm.CommandText = $"Select email,password,confirmed From Users Where confirmed ='{value["code"]}'";
            try
            {
                SqlConn.Open();
                SqlDataReader reader = SqlComm.ExecuteReader();
                if (reader.Read())
                {
                    Cookie cookie = new Cookies().CreateCockieWithHashPass((string)reader["email"], (string)reader["password"], HRequest.UserAgent);
                    HResponse.Cookies.Add(cookie);
                    reader.Close();
                    SqlComm.CommandText = $"Update Users Set confirmed='true' Where confirmed='{value["code"]}'";
                    SqlComm.ExecuteNonQuery();
                    SqlConn.Close();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public void Login(string email,string pass, Cookies cookie)
        {
            CheckForSymbols(6,40,pass,email);
            email = email.ToLower();
            HashPass(pass);
            SqlComm.CommandText =string.Format( "Select Email,Password,confirmed From Users Where Email ='{0}' AND Password ='{1}'",email, PasswordHash);
            SqlConn.Open();
            SqlDataReader sqlReader = SqlComm.ExecuteReader();
            while (sqlReader.Read())
            {
                if ((string)sqlReader["confirmed"] != "true")
                {
                    SqlConn.Close();
                    throw new Exception("Почта не подтверждена");
                }
                if ((string)sqlReader["email"]==email||(string)sqlReader["password"]== PasswordHash)
                {
                    sqlReader.Close();
                    SqlComm.CommandText = String.Format("Update Users Set logcookie='{0}' Where Email='{1}' and Password='{2}'",cookie.Value,email, PasswordHash);
                    SqlComm.ExecuteNonQuery();
                    SqlConn.Close();
                    return;
                }
            }
            SqlConn.Close();
            throw new Exception("Ошибка авторизации.Возможно, Вы неправильно указали Почту или пароль.");
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
                byte[] ByteHash = md5.ComputeHash(Encoding.UTF8.GetBytes(pass));
                foreach (var value in ByteHash)
                {
                    PasswordHash +=value.ToString("x2");
                }
            }
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
    }
}
