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
        private Thread _receiveThread;
        private readonly Thread _processThread;

        private readonly Queue<string> _messageQueue = new Queue<string>();

        private ManualResetEvent _stopProcessingThreadEvent = new ManualResetEvent(false);

        // Сигнальное состояние (true): доступ к очереди разрешен.
        // Несигнальное состояние (false): доступ к очереди заблокирован.
        private ManualResetEvent _messageQueueAccessEvent = new ManualResetEvent(true);

        private TcpClient _currentClient;

        /// <summary>
        /// Инициализирует новый экземпляр сервера сообщений, прослушивающего соединения по указанному адресу и порту.
        /// </summary>
        /// <param name="ipString">Строка, содержащая IP-адрес.</param>
        /// <param name="port">Номер TCP-порта.</param>
        public MessageServer(string ipString, int port)
        {
            IPAddress localAddress = IPAddress.Parse(ipString);
            _listener = new TcpListener(localAddress, port);
            _acceptThread = new Thread(AcceptThreadWorker);
            _processThread = new Thread(ProcessThreadWorker);
        }

        /// <summary>
        /// Запускает сервер для приёма сообщений и инициализирует потоки обработки сообщений.
        /// </summary>
        public void Start()
        {
            _listener.Start();
            _acceptThread.Start();
            _processThread.Start();
        }

        /// <summary>
        /// Останавливает сервер приёма сообщений и безопасно освобождает неуправляемые ресурсы.
        /// </summary>
        public void Stop()
        {
            _listener.Stop();
            _stopProcessingThreadEvent.Set();

            _processThread.Join();
            _acceptThread.Join();

            _currentClient?.Close();
        }

        private void AcceptThreadWorker()
        {
            _currentClient = _listener.AcceptTcpClient();
            
            OnClientConnected.Invoke(_currentClient.Client.RemoteEndPoint.ToString());

            while (_listener.Server != null)
            {
                try
                {
                    if ((_receiveThread == null))
                    {
                        _receiveThread = new Thread(ReceiveThreadWorker);
                        _receiveThread.Start();
                    }
                }
                catch (Exception)
                {
                    OnClientDisconnected.Invoke();
                    break;
                }
            }
        }

        private void ReceiveThreadWorker()
        {
            NetworkStream stream = _currentClient.GetStream();

            try
            {
                while (_currentClient.Connected)
                {
                    // Буфер размером 64 кБ.
                    byte[] _data = new byte[65536];

                    byte lastByte;
                    int i = 0;

                    do
                    {
                        lastByte = ((byte) stream.ReadByte());
                        _data[i++] = lastByte;
                    }
                    while (lastByte != '\n');

                    // count = i - 1, чтобы удалить последний символ переноса строки.
                    string result = Encoding.UTF8.GetString(_data, 0, i - 1);

                    // Вход в критическую секцию.
                    _messageQueueAccessEvent.WaitOne();

                    _messageQueue.Enqueue(result);

                    // Выход из критической секции.
                    _messageQueueAccessEvent.Set();
                }
            }
            catch (Exception)
            {
                OnClientDisconnected.Invoke();
            }
            finally
            {
                stream.Close();
            }
        }

        private void ProcessThreadWorker()
        {
            while (true)
            {
                if (_stopProcessingThreadEvent.WaitOne(0))
                {
                    break;
                }

                if (_messageQueue.Count > 0)
                {
                    // Вход в критическую секцию.
                    _messageQueueAccessEvent.WaitOne();

                    string result = _messageQueue.Dequeue();

                    // Выход из критической секции.
                    _messageQueueAccessEvent.Set();

                    OnMessageReceivedAndProcessed.Invoke(result);
                }
            }
        }
    }
}
