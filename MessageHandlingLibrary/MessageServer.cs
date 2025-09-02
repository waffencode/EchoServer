using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MessageHandlingLibrary
{
    /// <summary>
    /// Представляет многопоточный сервер для обработки сообщений, поступающих от TCP-клиента через сокет.
    /// </summary>
    public class MessageServer
    {
        /// <summary>
        /// Событие, вызываемое в момент подключения клиента.
        /// </summary>
        public event ConnectEventHandler OnClientConnected;
        public delegate void ConnectEventHandler(string identity);

        /// <summary>
        /// Событие, вызываемое после отключения клиента.
        /// </summary>
        public event DisconnectEventHandler OnClientDisconnected;
        public delegate void DisconnectEventHandler();

        /// <summary>
        /// Событие, вызываемое после получения и успешной обработки сообщения.
        /// </summary>
        public event MessageEventHandler OnMessageReceivedAndProcessed;
        public delegate void MessageEventHandler(string message);
        
        private readonly TcpListener _listener;

        private readonly Thread _acceptThread;
        private readonly Thread _receiveThread;
        private readonly Thread _processThread;

        private readonly Queue<string> _messageQueue = new Queue<string>();

        public MessageServer()
        {
            IPAddress localAddress = IPAddress.Parse("127.0.0.1");
            _listener = new TcpListener(localAddress, 7777);
            _acceptThread = new Thread(AcceptThreadWorker);
        }

        /// <summary>
        /// Запускает сервер для приёма сообщений и инициализирует потоки обработки сообщений.
        /// </summary>
        public void Start()
        {
            _listener.Start();
            _acceptThread.Start();
        }

        /// <summary>
        /// Останавливает сервер приёма сообщений и безопасно освобождает неуправляемые ресурсы.
        /// </summary>
        public void Stop()
        {
            _listener.Stop();
            _acceptThread.Join();
        }

        private void AcceptThreadWorker()
        {
            using (TcpClient _currentClient = _listener.AcceptTcpClient())
            {
                OnClientConnected.Invoke(_currentClient.Client.RemoteEndPoint.ToString());

                while (_listener.Server != null)
                {
                    try
                    {
                        NetworkStream stream = _currentClient.GetStream();
                        byte[] _data = new byte[65536];

                        byte lastByte;
                        int i = 0;

                        do
                        {
                            lastByte = ((byte) stream.ReadByte());
                            _data[i++] = lastByte;
                        }
                        while (lastByte != '\n');

                        string result = Encoding.UTF8.GetString(_data, 0, i + 1);
                        _messageQueue.Enqueue(result);
                        OnMessageReceivedAndProcessed.Invoke(result);
                    }
                    catch (Exception)
                    {
                        OnClientDisconnected.Invoke();
                        break;
                    }
                }
            }
        }

        private void ReceiveThreadWorker()
        {

        }

        private void ProcessThreadWorker()
        {

        }
    }
}
