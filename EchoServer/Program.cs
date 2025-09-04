using System;
using MessageHandlingLibrary;

namespace EchoServer
{
    internal class Program
    {
        public static void Main()
        {
            MessageServer server = new MessageServer("127.0.0.1", 7777);

            server.OnClientConnected += ShowClientConnectMessage;
            server.OnClientDisconnected += ShowClientDisconnectMessage;
            server.OnThreadException += ShowExceptionText;

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

        private static void ShowClientDisconnectMessage()
        {
            Console.WriteLine($"Клиент отключен.");
        }

        private static void ShowExceptionText(string message)
        {
            Console.WriteLine(message);
        }
    }
}
