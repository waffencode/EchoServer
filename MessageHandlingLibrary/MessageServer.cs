using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MessageHandlingLibrary
{
    public class MessageServer
    {
        private readonly TcpListener _listener;

        public delegate void ConnectEventHandler(string identity);
        public event ConnectEventHandler OnClientConnected;

        public delegate void DisconnectEventHandler(string identity);
        public event DisconnectEventHandler OnClientDisconnected;

        private readonly Thread _acceptThread;
        private readonly Thread _receiveThread;
        private readonly Thread _processThread;

        public MessageServer()
        {
            IPAddress localAddress = IPAddress.Parse("127.0.0.1");
            _listener = new TcpListener(localAddress, 7777);
            _acceptThread = new Thread(AcceptThreadWorker);
        }

        public void Start()
        {
            _listener.Start();
            _acceptThread.Start();
        }

        public void AcceptThreadWorker()
        {
            while (_listener.Server != null)
            {
                if (_listener.Pending())
                {
                    using (TcpClient tcpClient = _listener.AcceptTcpClient())
                    {
                        OnClientConnected.Invoke(tcpClient.Client.RemoteEndPoint.ToString());
                    }
                }             
            }
        }

        public void Stop()
        {
            _listener.Stop();
            _acceptThread.Join();
        }
    }
}
