using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageHandlingLibrary;

namespace EchoServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            MessageServer server = new MessageServer();
            server.Start();
            Console.WriteLine("Server started successfully.");

            server.Stop();
            Console.WriteLine("Server stopped successfully.");
        }
    }
}
