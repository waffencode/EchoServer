using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageHandlingLibrary
{
    public class MessageServer
    {
        private TcpListener _listener;

        public MessageServer()
        {
            IPAddress localAddress = IPAddress.Parse("127.0.0.1");
            _listener = new TcpListener(localAddress, 8888);
        }

        public void Start()
        {
            _listener.Start();
        }

        public void Stop()
        {
            _listener.Stop();
        }
    }
}
