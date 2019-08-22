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
        static void Main(string[] args)
        {
            Server("*",80);
        }
        /// <summary>
        /// Start Listener on ip:port
        /// </summary>
        /// <param name="ip">server ip</param>
        /// <param name="port">server port</param>
        static void Server(string ip,int port)
        {
            Hlistener.Prefixes.Add($"http://{ip}:{port}/");
            Hlistener.Start();
            Console.Write("Ready..");
            while (true)
            {
                HttpListenerContext HContext = Hlistener.GetContext();
                new Thread(new ParameterizedThreadStart(new Server().GetResponse)).Start(HContext);
            }
        }
    }
}
