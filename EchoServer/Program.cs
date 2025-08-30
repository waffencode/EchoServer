using System;
using MessageHandlingLibrary;

namespace EchoServer
{
    internal class Program
    {
        public static void Main()
        {
            MessageServer server = new MessageServer();

            server.OnClientConnected += ShowClientConnectMessage;
            server.OnClientDisconnected += ShowClientDisconnectMessage;

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

        private static void ShowClientConnectMessage(string identity)
        {
            Console.WriteLine($"Клиент подключен: {identity}");
        }

        private static void ShowClientDisconnectMessage(string identity)
        {
            Console.WriteLine($"Клиент отключен: {identity}");
        }
    }
}
