using System;
using MessageHandlingLibrary;

namespace EchoServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            MessageServer server = new MessageServer();
            server.Start();
            Console.WriteLine("Сервер успешно запущен.");

            while (true)
            {   
                if (Console.ReadKey().Key == ConsoleKey.Enter)
                {
                    server.Stop();
                    Console.WriteLine("Сервер успешно остановлен.");
                    return;
                }
            }
            
        }
    }
}
