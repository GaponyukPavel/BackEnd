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
        static Regex regex = new Regex(@"^[^\W\s]+@?[^\W\s]+[-]?[^\W\s]+[.]?[^\W\s]+$");

        public Authentication()
        {
            SqlComm = SqlConn.CreateCommand();
        }
        
        //add new user to DB
        public void AddUser(string mail, string nick, string pass)
        {
            CheckForSymbols(6, 40, mail, nick);
            IsRepeated(mail.ToLower(), nick.ToLower());
            PasswordHash=Hash(pass);    
            SqlComm.CommandText = string.Format("Insert users (Email,NickName,Password,LogCookie,confirmed) Values('{0}','{1}','{2}','{3}','{4}')", mail.ToLower(), nick.ToLower(), PasswordHash,"",Hash(mail));
            SqlConn.Open();
            try
            {
                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress("ponypromasterpro@gmail.com", "BookShop"),
                    IsBodyHtml = true,
                    Subject = "Registration on BookShop"
                };
                mailMessage.To.Add(mail);
                string page = "";
                using (StreamReader streamReader = new StreamReader(Environment.CurrentDirectory + "/web/Not/mail.html"))
                {
                    page = streamReader.ReadToEnd();
                    page = page.Replace("NAME", nick);
                    page = page.Replace("test", Hash(mail));
                }
                mailMessage.Body = page;

                SmtpClient smpt = new SmtpClient("smtp.gmail.com")
                {
                    EnableSsl = true,
                    Port = 587,
                    DeliveryMethod= SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("ponypromasterpro@gmail.com", "ponypromasterpro43")
                };

                smpt.Send(mailMessage);
                SqlComm.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("Ошибка!Данные введены неверно либо сервер не доступен!");
            }
            finally
            {
                SqlConn.Close();
            }

        }
        //confirm user email. Return true if everything ok
        public bool Confirmed(NameValueCollection value)
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
        /// <summary>
        /// Try login user by email and pass 
        /// </summary>
        /// <param name="email">user email</param>
        /// <param name="pass">user pass </param>
        /// <param name="cookie"></param>
        public void Login(string email,string pass, CookiesClass cookie)
        {
            CheckForSymbols(6,40,pass,email);
            email = email.ToLower();
            PasswordHash=Hash(pass);
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
                    SqlComm.CommandText = String.Format("Update Users Set logcookie='{0}' Where Email='{1}' and Password='{2}'",cookie.LoginValue, email, PasswordHash);
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
        void CheckForSymbols(int minsize, int maxsize, string pass,params string[] strings)
        {
            if (pass == null)
            {
                throw new Exception("Неверный размер даных.");
            }
            foreach (string value in strings)
            {
                if (value == null || value.Length <= minsize || value.Length >= maxsize)
                {
                    throw new Exception("Неверный размер даных.");
                }
                if (!regex.IsMatch(value))
                {
                    throw new Exception("Значения содержат не алфавитно цифровые значения!");
                }
            }
        }
        /// <summary>
        /// Check if in DB alredy user with that email or nickname
        /// </summary>
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
        /// <summary>
        /// Hash Method
        /// </summary>
        /// <param name="ValueToHash">Value you whant to hash</param>
        public static string Hash(string ValueToHash)
        {
            ValueToHash = "S@!7"+ValueToHash+ "S@!72";
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
