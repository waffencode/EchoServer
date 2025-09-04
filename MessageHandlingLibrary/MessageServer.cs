using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MessageHandlingLibrary.Exceptions;

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
        public event Action<string> OnClientConnected;

        /// <summary>
        /// Событие, вызываемое после отключения клиента.
        /// </summary>
        public event Action OnClientDisconnected;

        /// <summary>
        /// Событие, вызываемое после получения и успешной обработки сообщения.
        /// </summary>
        public event Action<string> OnMessageReceivedAndProcessed;

        /// <summary>
        /// Событие, вызываемое при возникновении исключения.
        /// </summary>
        public event Action<string> OnThreadException;

        private readonly TcpListener _listener;

        private readonly Thread _acceptThread;
        private Thread _receiveThread;
        private readonly Thread _processThread;

        private readonly Queue<string> _messageQueue = new Queue<string>();

        // Сигнальное состояние означает завершение работы.
        private readonly ManualResetEvent _shouldShutdownEvent = new ManualResetEvent(false);

        // Сигнальное состояние: доступ к очереди разрешён.
        // Несигнальное состояние: доступ к очереди заблокирован.
        private readonly ManualResetEvent _messageQueueAccessEvent = new ManualResetEvent(false);

        private TcpClient _currentClient;

        private const int MAX_MESSAGE_SIZE = 65536;

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
            _shouldShutdownEvent.Set();

            _processThread.Join();
            _acceptThread.Join();

            _currentClient?.Close();
        }

        private void AcceptThreadWorker()
        {
            while (!_shouldShutdownEvent.WaitOne(0))
            {
                _currentClient = _listener.AcceptTcpClient();
                OnClientConnected.Invoke(_currentClient.Client.RemoteEndPoint.ToString());

                if (_receiveThread == null || _receiveThread.ThreadState == ThreadState.Stopped)
                {
                    _receiveThread = new Thread(ReceiveThreadWorker);

                    // Доступ к очереди разрешается.
                    _messageQueueAccessEvent.Set();
                }

                _receiveThread.Start();
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
                    byte[] _data = new byte[MAX_MESSAGE_SIZE];

                    byte lastByte;
                    int i = 0;

                    do
                    {
                        lastByte = ((byte) stream.ReadByte());
                        _data[i++] = lastByte;

                        if (i >= MAX_MESSAGE_SIZE)
                        {
                            throw new LengthExceededException();
                        }
                    }
                    while (lastByte != '\n');

                    // count = i - 1, чтобы удалить последний символ переноса строки.
                    string result = Encoding.UTF8.GetString(_data, 0, i - 1);

                    // Ожидание возможности доступа к очереди.
                    _messageQueueAccessEvent.WaitOne();

                    _messageQueue.Enqueue(result);

                    // Установка сигнального состояния - следующий поток может получить доступ.
                    _messageQueueAccessEvent.Set();
                }
            }
            catch (LengthExceededException ex)
            {
                OnThreadException.Invoke(ex.Message);
            }
            catch (InvalidDataFormatException ex)
            {
                OnThreadException.Invoke(ex.Message);
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
                if (_shouldShutdownEvent.WaitOne(0))
                {
                    break;
                }

                if (_messageQueue.Count > 0)
                {
                    // Ожидание возможности доступа к очереди.
                    _messageQueueAccessEvent.WaitOne();

                    string result = _messageQueue.Dequeue();

                    // Установка сигнального состояния - следующий поток может получить доступ.
                    _messageQueueAccessEvent.Set();

                    OnMessageReceivedAndProcessed.Invoke(result);
                }
            }
        }
    }
}
