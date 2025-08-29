using System.Net;
using System.Net.Sockets;

namespace MessageHandlingLibrary
{
    public class MessageServer
    {
        private TcpListener _listener;

        public MessageServer()
        {
            IPAddress localAddress = IPAddress.Parse("127.0.0.1");
            _listener = new TcpListener(localAddress, 7777);
        }

        public void Start()
        {
            _listener.Start();
        }

        public void Stop()
        {
            // Safely stop all threads.

            // Close sockets and threads.
            _listener.Stop();

            // Release all unmanages resources.
        }
    }
}
