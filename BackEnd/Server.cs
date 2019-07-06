using System;
using System.Net;
using System.Web;
using System.Collections.Specialized;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BackEnd
{
    class Server
    {
        FileInfo Folder;
        string RequestBody;
        NameValueCollection Values;
        Cookies cookie;
        bool Login = false;
        byte[] buffer;

        public void GetResponse(object Context)
        {
            HttpListenerContext HContext = (HttpListenerContext)Context;
            HttpListenerRequest HRequest = HContext.Request;
            HttpListenerResponse HResponse = HContext.Response;
           if (HRequest.RawUrl.Contains(".html") || HRequest.Url.AbsolutePath == "/")
            {
                Login = Cookies.CheckForLogin(HRequest);
                Console.WriteLine("CHEKED");
            }
            if (HRequest.HttpMethod == "POST")
            {
                using (StreamReader streamReader = new StreamReader(HRequest.InputStream))
                {
                    RequestBody = streamReader.ReadToEnd();
                    Values = HttpUtility.ParseQueryString(RequestBody);
                    try
                    {
                        cookie = new Cookies(Values.Get("mail"), Values.Get("pass"), HRequest.UserAgent);
                    }
                    catch { }
                    if (HRequest.UrlReferrer.AbsolutePath == "/login.html")
                    {
                        try
                        {
                            new Authentication().Login(Values.Get("mail"), Values.Get("pass"), cookie);
                            HResponse.Cookies.Add(cookie.Cookie);
                            Login = true;
                            HResponse.Redirect("/index.html");
                            HResponse.OutputStream.Close();
                        }
                        catch (Exception e)
                        {
                            GetPage("/login.html", e.Message);
                        }
                        return;
                    }
                    if (HRequest.UrlReferrer.AbsolutePath == "/signup.html")
                    {
                        try
                        {
                            new Authentication().AddUser(Values.Get("mail"), Values.Get("name"), Values.Get("pass"), cookie);
                            HResponse.Redirect("/confirm.html");
                        }
                        catch (Exception e)
                        {
                            GetPage("/signup.html", e.Message);
                            return;
                        }
                        HResponse.OutputStream.Close();
                        return;
                    }
                    if (HRequest.UrlReferrer.AbsolutePath == "/confirm.html")
                    {
                        new Authentication().Confirmed(Values, HResponse, HRequest);
                        HResponse.Redirect("/index.html");
                        HResponse.OutputStream.Close();
                        return;
                    }
                    if (HRequest.UrlReferrer.Segments[1] == "book/")
                    {
                        int UserId=Cookies.GetUserIdByRequest(HRequest);
                        if (UserId == 0)
                        {
                            GetPage(HRequest.UrlReferrer.AbsolutePath);
                            return;
                        }
                        new PageBuilder().AddComment(Values["text"], HRequest.UrlReferrer.Segments[2].Replace(".html",""), UserId);
                        GetPage(HRequest.UrlReferrer.AbsolutePath);
                        return;
                    }
                    GetErrorPage("/400.html", 400);
                    return;
                }
            }//Если метод запоса Post

            if (HRequest.HttpMethod == "GET")
            {
                if (HRequest.Url.AbsolutePath == "/")
                {
                    GetPage("/index.html");
                    return;
                }
                if (!HRequest.Url.AbsolutePath.Contains(".html"))
                {
                    GetStuf(HRequest.Url.AbsolutePath);
                    return;
                }
                GetPage(HRequest.Url.AbsolutePath);
                return;
            }//Если метод запоса Get
            GetErrorPage("/405.html", 405);
            return;

            void GetPage(string url, string message = "", int StatusCode = 200)
            {
                Values = HttpUtility.ParseQueryString(HRequest.Url.Query);
                Regex regex = new Regex(@"^/book/\d+.html$");//Извлекает id товара
                int id = 0;
                if (regex.IsMatch(url))
                {
                    regex = new Regex(@"\d+");
                    id = Int32.Parse(regex.Match(url).Groups[0].Value);
                    url = "/book.html";
                }

                if (Login)
                {
                    Folder = new FileInfo(Environment.CurrentDirectory + "/web/Login" + url);
                }
                else
                {
                    Folder = new FileInfo(Environment.CurrentDirectory + "/web/Not" + url);
                }
                
                if (!Folder.Exists)
                {
                    GetErrorPage("/404.html", 404);
                    return;
                }
                string page;
                switch (url)
                {
                    case "/book.html":
                        try
                        {
                            buffer = new PageBuilder().GetBookPage(id, Folder.FullName);
                        }
                        catch(Exception e)
                        {
                            GetPage("/index.html");
                            Console.WriteLine(e.Message);
                            return;
                        }
                        break;

                    case "/index.html":
                        try
                        {
                            buffer = new PageBuilder().GetIndexPage(Values, Folder.FullName);
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.Message);
                            GetPage("/index.html", e.Message);
                            return;
                        }
                        break;
                }
                if (buffer == null)
                {
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        using (StreamReader StrReader = new StreamReader(Folder.FullName))
                        {
                            page = StrReader.ReadToEnd();
                        }
                        page = page.Replace("<div id=\"error_id\">", "<div id=\"error_id\">" + message);
                        buffer = Encoding.UTF8.GetBytes(page);
                    }
                    else
                    {
                        FileStream fileStream = Folder.OpenRead();
                        buffer = new byte[fileStream.Length];
                        BinaryReader reader = new BinaryReader(fileStream);
                        reader.Read(buffer, 0, buffer.Length);
                        reader.Close();
                    }
                }

                HResponse.StatusCode = StatusCode;
                HResponse.ContentLength64 = buffer.Length;
                
                try
                {
                    HResponse.OutputStream.Write(buffer, 0, buffer.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    HResponse.OutputStream.Close();
                }
                return;
            }
            void GetErrorPage(string url, int StatusCode)
            {
                Folder = new FileInfo(Environment.CurrentDirectory + "/web/errors" + url);
                if (Folder.Exists)
                {
                    FileStream fileStream = Folder.OpenRead();
                    buffer = new byte[fileStream.Length];
                    BinaryReader reader = new BinaryReader(fileStream);
                    reader.Read(buffer, 0, buffer.Length);
                    reader.Close();
                    HResponse.StatusCode = StatusCode;
                    HResponse.ContentLength64 = buffer.Length;
                    HResponse.OutputStream.Write(buffer, 0, buffer.Length);
                    HResponse.OutputStream.Close();
                    return;
                }
                if (url != "/404.html")
                {
                    GetErrorPage("/404.html", 404);
                }
                else
                {
                    HResponse.OutputStream.Close();
                    return;
                }
            }
            void GetStuf(string url)
            {
                Console.WriteLine(url);
                Folder = new FileInfo(Environment.CurrentDirectory + "/web" + url);
                if (Folder.Exists)
                {
                    FileStream fileStream = Folder.OpenRead();
                    buffer = new byte[fileStream.Length];
                    BinaryReader reader = new BinaryReader(fileStream);
                    reader.Read(buffer, 0, buffer.Length);
                    reader.Close();
                    HResponse.ContentLength64 = buffer.Length;
                    try
                    {
                        HResponse.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    finally
                    {
                        HResponse.OutputStream.Close();
                    }
                    return;
                }
                HResponse.OutputStream.Close();
                return;
            }
        }

    }
}
