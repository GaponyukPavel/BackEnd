using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Specialized;
using System.Web;

namespace BackEnd
{
    class PageBuilder
    {
        SqlConnection sqlconn = new SqlConnection("Data source=Pasha;Initial Catalog=BookShop;Integrated Security=true;");
        SqlCommand sqlCommand;
        string page;
        public PageBuilder()
        {
            sqlCommand = sqlconn.CreateCommand();
        }
        //public void AddBookPage(string BookName, string Description, string BookPicPath, double Price, string Info)
        //{
        //    if (string.IsNullOrWhiteSpace(BookPicPath))
        //        BookPicPath = "../img/books.default.jpg";
        //    sqlCommand.CommandText = string.Format("Insert Books (BookName,Description,BookPic,Price,Info) Values('{0}','{1}','{2}','{3}','{4}');", BookName, Description, BookPicPath, Price, Info);
        //    sqlconn.Open();
        //    sqlCommand.ExecuteNonQuery();
        //    sqlconn.Close();
        //}

        /// <summary>
        /// generate book page values array for JavaScript
        /// </summary>
        public byte[] GetBookPageValuesForJS(NameValueCollection Values)
        {
            int id = 1;
            int.TryParse(Values["id"], out id);
            sqlCommand.CommandText = string.Format("Select * from Books Where Bookid='{0}';", id);
            sqlconn.Open();
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            if (!sqlDataReader.Read())
            {
                throw new Exception();
            }
            for (int b = 0; b < sqlDataReader.FieldCount; b++)
            {
                page += $"{sqlDataReader.GetName(b).ToUpper()}::{sqlDataReader[b]}:::";
            }
            page = page.Remove(page.Length - 3);
            sqlDataReader.Close();
            return Encoding.UTF8.GetBytes(page);
        }
        /// <summary>
        /// generate book page comments array for JavaScript
        /// </summary>
        public byte[] GetCommentsValuesForJS(NameValueCollection Values)
        {
            int id = 1;
            int.TryParse(Values["id"], out id);

            sqlCommand.CommandText = $"select top(3) reviewtext,userid from review where bookid={id}";
            sqlconn.Open();
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            string Comment = "";
            while (sqlDataReader.Read())
            {
                Comment += $"{sqlDataReader["reviewtext"]}::";

                SqlConnection sqlConnection = new SqlConnection("Data source=Pasha;Initial Catalog=BookShop;Integrated Security=true;");
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = $"Select nickname,userpic From Users where userid={sqlDataReader["userId"]}";

                sqlConnection.Open();
                SqlDataReader sqlDataReader2 = sqlCommand.ExecuteReader();
                sqlDataReader2.Read();
                Comment += $"{sqlDataReader2["userpic"]}::{sqlDataReader2["nickname"]}";
                sqlConnection.Close();
                Comment += "::\n::";
            }
            if(!string.IsNullOrEmpty(Comment))
                Comment = Comment.Remove(Comment.Length-5);
            sqlconn.Close();
            return Encoding.UTF8.GetBytes(Comment);
        }
        /// <summary>
        /// generate like section array for JavaScript
        /// </summary>
        public byte[] GetLikeValuesForJS(NameValueCollection Values)
        {
            int id = 1;
            int.TryParse(Values["id"], out id);
            string BookName = string.IsNullOrEmpty(Values["BookName"]) ? "" : Values["BookName"];
            string Author = string.IsNullOrEmpty(Values["Author"]) ? "" : Values["Author"];
            if (BookName.Length > 5)
                BookName = BookName.Remove(5);
            sqlCommand.CommandText = $"Select TOP(3) bookid,price,bookname,author,bookpic from Books where bookid != {id} and (bookname like '%{BookName}%' or author like '{Author}') ORDER BY NEWID()";
            sqlconn.Open();
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            string like = "";
            for (int i = 0; i < 3 && sqlDataReader.Read(); i++)
            {
                for (int b = 0; b < sqlDataReader.FieldCount; b++)
                {
                    like += $"{sqlDataReader.GetName(b).ToUpper()}::{sqlDataReader[b]}:::";
                }
                like = like.Remove(like.Length - 3) + "\n";
            }
            if (!string.IsNullOrEmpty(like))
                like = like.Remove(like.Length - 1);
            sqlDataReader.Close();
            sqlconn.Close();
            return Encoding.UTF8.GetBytes(like);
        }

        /// <summary>
        /// generate index page array for JavaScript
        /// </summary>
        /// <param name="Values"></param>
        public byte[] GetIndexPageValuesForJS(NameValueCollection Values)
        {
            string Main = "";
            string Second = "";
            string Command = "";
            string Serch = "";
            page = "";
            int PageCount;
            int CurentPage;
            #region Проверка входящих значений на корректность и создание команды для sql

            if (!int.TryParse(Values["page"], out CurentPage))
                CurentPage = 1;
            if (CurentPage < 0)
            {
                CurentPage = 1;
            }
            switch (Values["main"])
            {
                case "Все":
                    Main += string.Format("Select BookPic,BookName,Discount,BookId,genre,price,author from Books Where Price>0");
                    break;
                case "Бестселлеры":
                    Main += string.Format("Select Top(45) BookPic,BookName,Discount,BookId,genre,price,author From Books where Price>0 order by salecount ");
                    break;
                case "Новинки":
                    Main += string.Format("Select BookPic,BookName,Discount,BookId,genre,price,author from Books Where releaseDate>'{0}' and Price>0", DateTime.Today.AddMonths(-1));
                    break;
                case "Распродажа":
                    Main += string.Format("Select BookPic,BookName,Discount,BookId,genre,price,author from Books Where Discount>0 and Price>0  order by Discount OFFSET 0 ROWS");
                    break;
                default:
                    Values["main"] = "Все";
                    goto case "Все";
            }
            if (Values["second"] == null)
                Values["second"] = "Все";
            Second = string.Format("select * from ({0}) as a where genre like \'%{1}%\'", Main, Values["second"] == "Все" ? "" : Values["second"]);
            if (Values["search"] != null)
                Serch = Values["search"];
            if (Serch.Contains("\'"))
                Serch = Serch.Replace("\'", "");
            Command = string.Format("Select * From ({0}) as b Where Bookname Like \'%{1}%\' or author Like \'%{1}%\'", Second, Serch);
            Command = $"Select Count(*) From ({Command}) as z;{Command} ORDER BY Bookid OFFSET {CurentPage * 15 - 15}ROWS FETCH NEXT 15 ROWS ONLY";
            #endregion
            sqlCommand.CommandText = Command;
            sqlconn.Open();
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            sqlDataReader.Read();
            PageCount = (int)sqlDataReader[0];
            if (PageCount % 15 > 0)
            {
                PageCount /= 15;
                PageCount++;
            }
            else
            {
                PageCount /= 15;
            }
            sqlDataReader.NextResult();
            page += $"TotalPage/CurentPage::{PageCount}::{CurentPage}\n";
            for (int i = 1; i < 16; i++)
            {
                if (!sqlDataReader.Read())
                    break;
                for (int b = 0; b < sqlDataReader.FieldCount; b++)
                {
                    page += $"{sqlDataReader.GetName(b).ToUpper()}::{sqlDataReader[b]}:::";
                }
                page = page.Remove(page.Length - 3) + "\n";
            }
            sqlDataReader.Close();
            sqlconn.Close();
            return Encoding.UTF8.GetBytes(page);
        }
        /// <summary>
        /// generate cart page array for JavaScript
        /// </summary>
        /// <param name="Values"></param>
        public byte[] GetCartPageValuesForJS(NameValueCollection Values)
        {
            string content = "";
            if (!string.IsNullOrEmpty(Values["books"]))
            {
                string BooksInCartValue = "";
                string TotalBooks = "";
                double TotalCost = 0;
                string[] buy = Values["books"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                BooksInCartValue = string.Join(",", buy);
                string command = $"Select Bookid,bookpic,Bookname,author,defaultprice,price,discount from Books where bookid in ({BooksInCartValue})";
                sqlCommand.CommandText = command;
                sqlconn.Open();
                SqlDataReader Reader = sqlCommand.ExecuteReader();
                while (Reader.Read())
                {
                    for (int b = 0; b < Reader.FieldCount; b++)
                    {
                        content += $"{Reader.GetName(b).ToUpper()}::{Reader[b]}:::";
                    }
                    content = content.Remove(content.Length - 3) + "\n";
                    TotalBooks += Reader["bookid"] + ",";
                    TotalCost += (double)Reader["price"];
                }
                content += $"{TotalBooks}:::{TotalCost}";
                sqlconn.Close();
            }
            return Encoding.UTF8.GetBytes(content);
        }
        /// <summary>
        /// generate profile values array for JavaScript
        /// </summary>
        public byte[] GetProfileValuesForJS(HttpListenerRequest HRequest)
        {
            if (HRequest.Cookies["logCookie"] == null || string.IsNullOrEmpty(HRequest.Cookies["logCookie"].Value))
            {
                return new byte[] {}; 
            }
            string CookieValue = HRequest.Cookies["logCookie"].Value;
            string UserAgent = HRequest.UserAgent;
            if (UserAgent == null)
                UserAgent = "";
            CookieValue = Authentication.Hash(CookieValue + UserAgent);
            sqlCommand.CommandText = string.Format("Select NickName,Role,UserPic,email From Users Where LogCookie='{0}';", CookieValue);
            sqlconn.Open();
            SqlDataReader Reader = sqlCommand.ExecuteReader();
            string answer = "";
            if (Reader.Read())
            {
                for (int a = 0; a < Reader.FieldCount; a++)
                {
                    answer += $"{Reader.GetName(a)}={Reader[a]};";
                }
                sqlconn.Close();
                return Encoding.UTF8.GetBytes(answer);
            }
            sqlconn.Close();
            return new byte[] {};

        }
        /// <summary>
        /// MEthod to change user picture
        /// </summary>
        public void ChangeUserPic(HttpListenerRequest Hrequest,string UserEmail)
        {
            string boundary ="--" + Hrequest.ContentType.Split(';')[1].Split('=')[1];
            Stream inputStream = Hrequest.InputStream;
            Encoding contentEnc = Hrequest.ContentEncoding;
            Byte[] boundaryBytes = contentEnc.GetBytes(boundary);
            int boundaryLen = boundaryBytes.Length;
            using (FileStream output = new FileStream(Environment.CurrentDirectory+"/web/img/users/"+UserEmail+".png", FileMode.Create, FileAccess.Write))
            {
                Byte[] buffer = new Byte[1024];
                int len = inputStream.Read(buffer, 0, 1024);
                int startPos = -1;
                string ContentType;

                // Find start pos
                while (true)
                {
                    if (len == 0)
                    {
                        throw new Exception("Start Boundaray Not Found");
                    }

                    startPos = IndexOf(buffer, len, boundaryBytes);
                    if (startPos >= 0)
                    {
                        break;
                    }
                    else
                    {
                        Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen);
                        len = inputStream.Read(buffer, boundaryLen, 1024 - boundaryLen);
                    }
                }

                // Skip four lines (Boundary, Content-Disposition, Content-Type, and a blank)
                for (int i = 0; i < 4; i++)
                {
                    int lastPos;
                    byte[] ContentTypebyte;
                    while (true)
                    {
                        if (len == 0)
                        {
                            throw new Exception("Preamble not Found.");
                        }
                        lastPos = startPos;
                        startPos = Array.IndexOf(buffer, contentEnc.GetBytes("\n")[0], startPos);
                        if (i == 2)
                        {
                            int masSize = startPos - lastPos-1;
                            ContentTypebyte = new byte[masSize];
                            Array.Copy(buffer, lastPos, ContentTypebyte, 0, masSize);
                            ContentType = Encoding.UTF8.GetString(ContentTypebyte);
                            if (ContentType.Contains("Content-Type:"))
                            {
                                ContentType = ContentType.Split('/')[1];
                                if (ContentType != "jpeg" && ContentType != "png")
                                    break;
                            }
                            else
                            {
                                return;
                            }
                        }
                        
                        if (startPos >= 0)
                        {
                            startPos++;
                            break;
                        }
                        else
                        {
                            len = inputStream.Read(buffer, 0, 1024);
                        }
                    }
                }

                Array.Copy(buffer, startPos, buffer, 0, len - startPos);
                len = len - startPos;

                while (true)
                {
                    int endPos = IndexOf(buffer, len, boundaryBytes);
                    if (endPos >= 0)
                    {
                        if (endPos > 0) output.Write(buffer, 0, endPos - 2);
                        break;
                    }
                    else if (len <= boundaryLen)
                    {
                        throw new Exception("End Boundaray Not Found");
                    }
                    else
                    {
                        output.Write(buffer, 0, len - boundaryLen);
                        Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen);
                        len = inputStream.Read(buffer, boundaryLen, 1024 - boundaryLen) + boundaryLen;
                    }
                }
            }
            int IndexOf(Byte[] buffer, int len, Byte[] boundarBytes)
            {
                for (int i = 0; i <= len - boundarBytes.Length; i++)
                {
                    Boolean match = true;
                    for (int j = 0; j < boundarBytes.Length && match; j++)
                    {
                        match = buffer[i + j] == boundarBytes[j];
                    }

                    if (match)
                    {
                        return i;
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// add comment method
        /// </summary>
        /// <param name="Comment"></param>
        /// <param name="BookId">book  to add comment</param>
        /// <param name="UserId"></param>
        public void AddComment(string Comment, string BookId, int UserId)
        {
            if (string.IsNullOrEmpty(Comment))
                return;
            Comment = Comment.Replace("'", "''");
            sqlCommand.CommandText = $"Insert into Review Values ({BookId},'{Comment}',{UserId})";
            sqlconn.Open();
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch { }
            sqlconn.Close();
        }

    }
}