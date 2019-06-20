using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd
{
    class Program
    {

        static HttpListener Hlistener = new HttpListener();
        static string uri;
        static void Main(string[] args)
        {
            Server("http://192.168.1.7:80/");
        }
        static void Server(string uri)
        {
            Program.uri = uri;
            Hlistener.Prefixes.Add(uri);
            Hlistener.Start();
            Console.Write("Готово..");
            while (true)
            {
                HttpListenerContext HContext = Hlistener.GetContext();
                new Thread(new ParameterizedThreadStart(new Server().GetResponse)).Start(HContext);
            }
        }
    }
}
