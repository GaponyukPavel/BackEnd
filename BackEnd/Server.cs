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
        CookiesClass cookie;
        bool Login = false;
        string Role=""; 
        string Email="";
        byte[] buffer;
        static Regex regex = new Regex(@"^/book/\d+.html$");

        public void GetResponse(object Context)
        {
            HttpListenerContext HContext = (HttpListenerContext)Context;
            HttpListenerRequest HRequest = HContext.Request;
            HttpListenerResponse HResponse = HContext.Response;
            
            //Check user for login if he request html page and return his email and role
            if (HRequest.RawUrl.Contains(".html") || HRequest.Url.AbsolutePath == "/")
            {
                Login = CookiesClass.CheckForLogin(HRequest, out Role,out Email);
                Console.WriteLine($"Login: {Login} Role: {Role}");
            }

            // if request method POST
            if (HRequest.HttpMethod == "POST")
            {
                using (StreamReader streamReader = new StreamReader(HRequest.InputStream))
                {
                    //if user change userpicture 
                    if (HRequest.UrlReferrer.AbsolutePath == "/profile.html" && HRequest.ContentType.Split(new char[] { ';' })[0] == "multipart/form-data")
                    {
                        new PageBuilder().ChangeUserPic(HRequest, Email);
                        GetPage("/profile.html");
                        return;
                    }

                    //read Post data and write it to Values  
                    RequestBody = streamReader.ReadToEnd();
                    Values = HttpUtility.ParseQueryString(RequestBody);

                    //if user try register 
                    if (HRequest.UrlReferrer.AbsolutePath == "/signup.html")
                    {
                        try
                        {
                            new Authentication().AddUser(Values.Get("mail"), Values.Get("name"), Values.Get("pass"));
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

                    //if user confirm his email
                    if (HRequest.UrlReferrer.AbsolutePath == "/confirm.html")
                    {
                        if (!new Authentication().Confirmed(Values))
                        {
                            GetPage("/confirm.html","Неверный код");
                            return;
                        }
                        HResponse.Redirect("/login.html");
                        HResponse.OutputStream.Close();
                        return;
                    }

                    //if user leave a comment on book page 
                    if (HRequest.UrlReferrer.Segments[1] == "book/")
                    {
                        if (!Login)
                        {
                            GetPage(HRequest.UrlReferrer.AbsolutePath);
                            return;
                        }

                        if (Values["type"] == "review")
                        {
                            int UserId = CookiesClass.GetUserIdByRequest(HRequest);
                            if (UserId == 0)
                            {
                                GetPage(HRequest.UrlReferrer.AbsolutePath);
                                return;
                            }
                            new PageBuilder().AddComment(Values["text"], HRequest.UrlReferrer.Segments[2].Replace(".html", ""), UserId);
                            GetPage(HRequest.UrlReferrer.AbsolutePath);
                            return;
                        }
                    }

                    //if requested not a html page
                    if (!HRequest.Url.AbsolutePath.Contains(".html"))
                    {
                        GetStuf(HRequest.Url.AbsolutePath);
                        return;
                    }
                    GetErrorPage("/400.html", 400);
                    return;
                }
            }

            // if request method GET
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
            }

            //if request method not GET or POST return error page
            GetErrorPage("/405.html", 405);
            return;
            
            //send page to user
            void GetPage(string url, string message = "", int StatusCode = 200)
            {
                Values = HttpUtility.ParseQueryString(HRequest.Url.Query);
                foreach (var item in Values)
                {
                    Console.WriteLine($"\t{url} : {item} : {Values[(string)item]}");
                }
                if (regex.IsMatch(url))
                    url = "/book.html";
                if (Login)
                {
                    try
                    {
                        Folder = new FileInfo(Environment.CurrentDirectory + "/web/Login" + url);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        GetErrorPage("/404.html", 404);
                        return;
                    }
                }
                else
                {
                    try
                    {
                        Folder = new FileInfo(Environment.CurrentDirectory + "/web/Not" + url);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        GetErrorPage("/404.html", 404);
                        return;
                    }
                }

                if (!Folder.Exists)
                {
                    GetErrorPage("/404.html", 404);
                    return;
                }

                if (buffer == null)
                {
                    string page;
                    using (StreamReader StrReader = new StreamReader(Folder.FullName))
                    {
                        page = StrReader.ReadToEnd();
                    }
                    page = page.Replace("<div id=\"error_id\">", "<div id=\"error_id\">" + message);
                    buffer = Encoding.UTF8.GetBytes(page);

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
            //send error page to user
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
                    return;
                }
                HResponse.OutputStream.Close();
                  
            }
            //send page stuf(.cs .js img etc)
            void GetStuf(string url)
            {
                switch (url)
                {
                    case "/Login":
                        try
                        {
                            HResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                            cookie = new CookiesClass(Values.Get("mail"), Values.Get("pass"), HRequest.UserAgent);
                            HResponse.Cookies.Add(cookie.MyCookie);
                            Login = true;
                            buffer = new byte[] {1};
                        }
                        catch (Exception e)
                        {
                            buffer = new byte[] { };
                            Console.WriteLine(e);
                        }
                        break;
                    case "/InCart":
                        try
                        {
                            HResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                            Values = HttpUtility.ParseQueryString(HRequest.Url.Query);
                            buffer = new PageBuilder().GetCartPageValuesForJS(Values);
                        }
                        catch (Exception e)
                        {
                            buffer = new byte[] { };
                            Console.WriteLine(e);
                        }
                        break;
                    case "/Index":
                        try
                        {
                            HResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                            Values = HttpUtility.ParseQueryString(HRequest.Url.Query);
                            buffer = new PageBuilder().GetIndexPageValuesForJS(Values);
                        }
                        catch (Exception e)
                        {
                            buffer = new byte[] { };
                            Console.WriteLine(e);
                        }
                        break;
                    case "/BookPage":
                        try
                        {
                            HResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                            Values = HttpUtility.ParseQueryString(HRequest.Url.Query);
                            buffer = new PageBuilder().GetBookPageValuesForJS(Values);
                        }
                        catch (Exception e)
                        {
                            buffer = new byte[] { };
                            Console.WriteLine(e);
                        }
                        break; 
                        case "/GetComments":
                        try
                        {
                            HResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                            Values = HttpUtility.ParseQueryString(HRequest.Url.Query);
                            buffer = new PageBuilder().GetCommentsValuesForJS(Values);
                        }
                        catch (Exception e)
                        {
                            buffer = new byte[] { };
                            Console.WriteLine(e);
                        }
                        break;
                    case "/GetLike":
                        try
                        {
                            HResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                            Values = HttpUtility.ParseQueryString(HRequest.Url.Query);
                            buffer = new PageBuilder().GetLikeValuesForJS(Values);
                        }
                        catch (Exception e)
                        {
                            buffer = new byte[] { };
                            Console.WriteLine(e);
                        }
                        break;
                    case "/Profile":
                        try
                        {
                            HResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                            buffer = new PageBuilder().GetProfileValuesForJS(HRequest);
                        }
                        catch (Exception e)
                        {
                            buffer = new byte[] {};
                            Console.WriteLine(e);
                        }
                        break;
                    default:
                        Folder = new FileInfo(Environment.CurrentDirectory + "/web" + url);
                        if (!Folder.Exists)
                        {
                            HResponse.OutputStream.Close();
                            return;
                        }
                        FileStream fileStream = Folder.OpenRead();
                        buffer = new byte[fileStream.Length];
                        BinaryReader reader = new BinaryReader(fileStream);
                        reader.Read(buffer, 0, buffer.Length);
                        reader.Close();
                        break;
                }
                try
                {
                    HResponse.ContentLength64 = buffer.Length;
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
        }

    }
}
