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

namespace BackEnd
{
    class Server
    {
        FileInfo Folder;
        string RequestBody;
        NameValueCollection values;
        Cookies cookie;
        bool Login = false;
        byte[] buffer;

        public void GetResponse(object Context)
        {
            HttpListenerContext HContext = (HttpListenerContext)Context;
            HttpListenerRequest HRequest = HContext.Request;
            HttpListenerResponse HResponse = HContext.Response;
            Console.WriteLine("rawurl: "+HRequest.RawUrl);
            if (HRequest.RawUrl.Contains(".html")||HRequest.RawUrl=="/")
            {
                Login = Cookies.CheckForLogin(HRequest.Cookies,HResponse, HRequest.UserAgent);
                Console.WriteLine("CHEKED");
            }
            if (HRequest.HttpMethod == "POST")
            {
                using (StreamReader streamReader = new StreamReader(HRequest.InputStream))
                {
                    RequestBody = streamReader.ReadToEnd();
                    values = HttpUtility.ParseQueryString(RequestBody);
                    foreach (var a in values.AllKeys)
                    {
                        Console.WriteLine("\t\t" + values[a]);
                    }
                    try
                    {
                        cookie = new Cookies(values.Get("mail"), values.Get("pass"), HRequest.UserAgent);
                    }
                    catch { }
                    if (HRequest.RawUrl == "/login.html")
                    {
                        try
                        {
                            new SqlBd().Login(values.Get("mail"), values.Get("pass"), cookie);
                            HResponse.Cookies.Add(cookie.Cookie);
                            Login = true;
                            GetPage("/index.html");
                        }
                        catch (Exception e)
                        {
                            GetPage("/login.html", e.Message);
                        }
                        return;
                    }
                    if (HRequest.RawUrl == "/signup.html")
                    {
                        try
                        {
                            new SqlBd().AddUser(values.Get("mail"), values.Get("name"), values.Get("pass"), cookie);
                        }
                        catch (Exception e)
                        {
                            GetPage("/signup.html", e.Message);
                            return;
                        }
                        HResponse.Cookies.Add(cookie.Cookie);
                        Login = true;
                        GetPage("/index.html");
                        return;
                    }
                    GetErrorPage("/400.html", 400);
                    return;
                }
            }

            if (HRequest.HttpMethod == "GET")
            {
                values = HttpUtility.ParseQueryString(HRequest.RawUrl);
                foreach (var a in values.AllKeys)
                {
                    Console.WriteLine("\t\t" + values[a]);
                }
                if (HRequest.RawUrl == "/")
                {
                    GetPage("/index.html");
                    return;
                }
                else
                {
                    GetPage(HRequest.Url.AbsolutePath);
                    return;
                }
            }
            GetErrorPage("/405.html", 405);
            return;

            void GetPage(string url, string message = "", int StatusCode = 200)
            {
                if (!url.Contains(".html"))
                {
                    GetStuf(url);
                    return;
                }
                if (Login)
                {
                     Folder = new FileInfo(Environment.CurrentDirectory + "/web/Login" + url);
                }
                else
                {
                     Folder = new FileInfo(Environment.CurrentDirectory + "/web/Not" + url);
                }

                if (Folder.Exists)
                {
                    string page;
                    if (url=="/book.html")
                    {
                        buffer = new PageBuilder().GetBookPage(3);
                    }
                    else {
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
                    HResponse.OutputStream.Write(buffer, 0, buffer.Length);
                    HResponse.OutputStream.Close();
                    return;
                }
                GetErrorPage("/404.html", 404);
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
                }
                return;
            }
            void GetStuf(string url)
            {
                Folder = new FileInfo(Environment.CurrentDirectory + "/web"+url);
                if (Folder.Exists)
                {
                    FileStream fileStream = Folder.OpenRead();
                    buffer = new byte[fileStream.Length];
                    BinaryReader reader = new BinaryReader(fileStream);
                    reader.Read(buffer, 0, buffer.Length);
                    reader.Close();
                    HResponse.ContentLength64 = buffer.Length;
                    HResponse.OutputStream.Write(buffer, 0, buffer.Length);
                    HResponse.OutputStream.Close();
                    return;
                }
                HResponse.OutputStream.Close();
                return;
            }
        }

    }
}
