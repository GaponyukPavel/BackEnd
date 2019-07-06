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
        SqlConnection sqlconn2 = new SqlConnection("Data source=Pasha;Initial Catalog=BookShop;Integrated Security=true;");
        SqlCommand sqlCommand;
        string page;
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
        /// <summary>
        /// Возвращает массив байтов сгенерированной страницы Book.html
        /// </summary>
        /// <param name="id">id страницы</param>
        /// <returns></returns>
        public byte[] GetBookPage(int id, string Folder)
        {
            sqlCommand.CommandText = string.Format("Select * from Books Where Bookid='{0}';", id);
            sqlconn.Open();
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            if (!sqlDataReader.Read())
            {
                throw new Exception();
            }
            using (StreamReader StrReader = new StreamReader(Folder))
            {
                page = StrReader.ReadToEnd();
            }
            //main section
            string BookName = (string)sqlDataReader["BookName"];
            string Author = (string)sqlDataReader["Author"];
            page = page.Replace("<h1 id=\"Name\">", "<h1 id=\"Name\">" + BookName);//BookName
            page = page.Replace("<p id=\"description\">", "<p id=\"description\">" + sqlDataReader["Description"]);//Description
            page = page.Replace("<img src=\"/book-pic", "<img src=\"" + sqlDataReader["BookPic"]);//BookPic
            page = page.Replace("<p id=\"price\">", "<p id=\"price\">Цена: " + sqlDataReader["Price"].ToString().Remove(5) + "$");//Price
            if ((int)sqlDataReader["discount"] > 0)
                page = page.Replace("<p id=\"dis\">", "<p id=\"dis\">Скидка: " + sqlDataReader["discount"] + "%");//discount
            page = page.Replace("<p id=\"infop\">", "<p id=\"infop\">" + sqlDataReader["Info"]);//Info
            page = page.Replace("a href=\"\" id=\"author\">", $"a href=\"/index.html?search={Author}\" id=\"author\">" + Author);//author
            string detailsFromDB = (string)sqlDataReader["details"];
            string endDetails = string.Format("<tr> <td>Жанр</td> <td><a>{0}</a></td> <tr/>", sqlDataReader["Genre"]);//Genre
            try
            {
                DateTime release = (DateTime)sqlDataReader["releasedate"];
                endDetails += string.Format("<tr> <td>Дата выпуска</td> <td>{0}</td> <tr/>", release.ToShortDateString());
            }
            catch
            { }
            sqlDataReader.Close();
            //details section
            NameValueCollection collection = HttpUtility.ParseQueryString(detailsFromDB);
            foreach (string key in collection.AllKeys)
            {
                endDetails += string.Format("<tr> <td>{0}</td> <td>{1}</td> <tr/>", key, collection[key]);
            }
            page = page.Replace("<table id=\"table\">", "<table id=\"table\">" + endDetails);
            //like section
            if (BookName.Length > 7)
                BookName = (BookName.Remove(BookName.Length - 3, 3)).Remove(0, 3);
            sqlCommand.CommandText = $"Select top(3) bookid,price,bookname,author,bookpic from Books where bookname like '%{BookName}%' or author like '{Author}' order by author";
            sqlDataReader = sqlCommand.ExecuteReader();
            string like = "";
            for (int i = 0; i < 3 && sqlDataReader.Read(); i++)
            {
                like += $"<div><img alt=\"book pic\"src=\"{sqlDataReader["bookpic"]}\"><div><h5>{sqlDataReader["bookname"]}</h5><p>{sqlDataReader["price"].ToString().Remove(5)}$</p><a href=\"/book/{sqlDataReader["bookid"]}.html\" class=\"book-btn\">Перейти</a></div></div>";
            }
            sqlDataReader.Close();
            page=page.Replace("<h4>Посмотрите также</h4>", "<h4>Посмотрите также</h4>" + like);
            //comment section
            sqlCommand.CommandText = $"select top(3) * from review where bookid={id}";
            sqlDataReader = sqlCommand.ExecuteReader();
            string Comment = "";
            sqlconn2.Open();
            while (sqlDataReader.Read())
            {
                SqlCommand command = new SqlCommand($"Select nickname,userpic From Users where userid={sqlDataReader["userid"]}", sqlconn2);
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();
                Comment += $"<div class=\"comment\"><div><img id=\"userpic\"src=\"{reader["userpic"]}\"alt=\"User picture\"><p>{reader["nickname"]}</p></div><p>{sqlDataReader["reviewtext"]}</p></div>";
            }
            sqlconn2.Close();

            page = page.Replace("<div class=\"comment\"/>",Comment);
            sqlconn.Close();
            return Encoding.UTF8.GetBytes(page);
        }
        /// <summary>
        /// Генерирует Index page
        /// </summary>
        /// <param name="Values">Параметры для генерации</param>
        /// <returns></returns>
        public byte[] GetIndexPage(NameValueCollection Values,string Folder)
        {
            string Main = "";
            string Second = "";
            string Command = "";
            int RowCount;
            int CurentPage =Values["page"] == null? 1:int.Parse(Values["page"]);
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
                    Main += string.Format("Select BookPic,BookName,Discount,BookId,genre,price,author from Books Where Discount>0 and Price>0");
                    break;
                default:
                    Values["main"] = "Все";
                    goto case "Все";
            }
            if (Values["second"] == null)
                Values["second"] = "Все";
            Second = string.Format("select * from ({0}) as a where genre like \'%{1}%\'",Main, Values["second"]=="Все"?"": Values["second"]);
            if(Values["search"]==null)
                Values["search"]="";
            if (Values["search"].Contains("\'"))
                Values["search"] = Values["search"].Replace("\'", "");
            Command = string.Format("Select * From ({0}) as b Where Bookname Like \'%{1}%\' or author Like \'%{1}%\'", Second, Values["search"]);
            Command = $"Select Count(*) From ({Command}) as z;"+Command;
            using (StreamReader StrReader = new StreamReader(Folder))
            {
                page = StrReader.ReadToEnd();
            }
            sqlCommand.CommandText = Command;
            sqlconn.Open();
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            sqlDataReader.Read();
            RowCount = (int)sqlDataReader[0];
            if (RowCount % 15 > 0)
            {
                RowCount /= 15;
                RowCount++;
            }
            else
            {
                RowCount /= 15;
            }
            string RowString="";
            for (int i = CurentPage-5<1 ? 1:CurentPage-5;i< CurentPage + 5; i++)
            {
                if (i > RowCount)
                    break;
                if (i == CurentPage)
                {
                    RowString += string.Format("<input id=\"{0}\"name=\"page\"type=\"radio\"checked=\"true\" value={1}><label for=\"{2}\"><a>{3}</a></label>",i,i,i,i);
                    continue;
                }
                RowString += string.Format("<input id=\"{0}\"name=\"page\"type=\"radio\"value={1}><label for=\"{2}\"><a>{3}</a></label>", i, i, i, i);
            }
            page = page.Replace("<div class=\"pages\">", "<div class=\"pages\">"+RowString);
            sqlDataReader.NextResult();

            for (int i=1;i<CurentPage; i++)
            {
                for (int a = 0; a < 15; a++)
                {
                    sqlDataReader.Read();
                }
            }
            for (int i = 1;i<16;i++)
            {
                if (!sqlDataReader.Read())
                    break;
                if ((int)sqlDataReader[2] > 0)
                {
                    page = page.Replace($"<td id=\"book{i}\">", $"<td id=\"book{i}\">" + $"<a href=\"/book/{sqlDataReader[3]}.html\"><div class=\"discount\"><div class=\"discount-img\"><p>-{sqlDataReader[2]}%</p><img src=\"/img/discount.png\"></div><img src=\"{sqlDataReader[0]}\"></div><h4>{sqlDataReader[1]}</h4><p>{sqlDataReader[5].ToString().Remove(5)} $</p></a>");
                }
                else
                {
                page = page.Replace($"<td id=\"book{i}\">", $"<td id=\"book{i}\">"+$"<a href=\"/book/{sqlDataReader[3]}.html\"><div class=\"discount\"><img src=\"{sqlDataReader[0]}\"></div><h4>{sqlDataReader[1]}</h4><p>{sqlDataReader[5].ToString().Remove(5)} $</p></a>");
                }
            }
            page = page.Replace("<input type=\"text\" name=\"search\"", "<input type=\"text\" name=\"search\"" + Values["search"]);
            page = page.Replace($"name=\"main\" value=\"{Values["main"]}\"", $"name=\"main\" value=\"{Values["main"]}\""+ "checked=\"true\"");
            page = page.Replace($"name=\"second\" value=\"{Values["second"]}\"", $"name=\"second\" value=\"{Values["second"]}\""+ "checked=\"true\"");
            page = page.Replace($"class=\"top-form-text\"", $"class=\"top-form-text\" value=\"{Values["search"]}\"");
            sqlDataReader.Close();
            sqlconn.Close();
            return Encoding.UTF8.GetBytes(page);
        }
        public void AddComment(string Comment,string BookId,int UserId)
        {
            if (string.IsNullOrEmpty(Comment))
                return;
            Comment = Comment.Replace("'","''");
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