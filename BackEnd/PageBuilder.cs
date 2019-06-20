using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Data.SqlClient;
using System.IO;

namespace BackEnd
{
    class PageBuilder
    {
        SqlConnection sqlconn = new SqlConnection("Data source=Pasha;Initial Catalog=BookShop;Integrated Security=true;");
        SqlCommand sqlCommand;
        public PageBuilder()
        {
            sqlCommand = sqlconn.CreateCommand();
        }
        public void AddBookPage(string BookName, string Description, string BookPicPath, double Price, string Info)
        {
            if (string.IsNullOrWhiteSpace(BookPicPath))
                BookPicPath ="../img/books.default.jpg";
            sqlCommand.CommandText =string.Format( "Insert Books (BookName,Description,BookPic,Price,Info) Values('{0}','{1}','{2}','{3}','{4}');",BookName,Description,BookPicPath,Price,Info);
            sqlconn.Open();
            sqlCommand.ExecuteNonQuery();
            sqlconn.Close();
        }
        public byte[] GetBookPage(int id)
        {
            sqlCommand.CommandText = string.Format("Select BookName,Description,BookPic,Price,Info from Books Where Bookid='{0}';", id);
            sqlconn.Open();
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            string page;
            using (StreamReader StrReader = new StreamReader(Environment.CurrentDirectory + "/web/Login/book.html" ))
            {
                page = StrReader.ReadToEnd();
            }
            if (!sqlDataReader.Read())
            {
                Console.WriteLine("Нет данных");
                return Encoding.UTF8.GetBytes(page);
            }
            page = page.Replace("<h1 id=\"Name\">", "<h1 id=\"Name\">" + sqlDataReader["BookName"]);//BookName
            page = page.Replace("<p id=\"description\">", "<p id=\"description\">" + sqlDataReader["Description"]);//Description
            page = page.Replace("<img src=\"book-pic", "<img src=\"" + sqlDataReader["BookPic"]);//BookPic
            page = page.Replace("<p id=\"price\">", "<p id=\"price\">Цена: " + sqlDataReader["Price"]);//Price
            page = page.Replace("<p id=\"info\">", "<p id=\"info\">" + sqlDataReader["Info"]);//Info
            sqlconn.Close();
            byte[] buffer = Encoding.UTF8.GetBytes(page);
            return buffer;
        }
    }
}
