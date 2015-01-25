using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;

namespace Server
{
    public class ClientListener
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (ClientListener));
        private Socket serverSocket;
        private int listenPort;

        public ClientListener(int port)
        {
            listenPort = port;
        }

        public void Listen()
        {
            serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, listenPort));
            serverSocket.Listen(100);

            new Thread(AcceptLoop).Start();
        }

        public void Stop()
        {
            serverSocket.SafeClose();
        }

        protected void AcceptLoop()
        {
            try
            {
                while (true)
                {
                    var clientSocket = serverSocket.Accept();
                    OnClientConnect(clientSocket);
                }
            }
            catch (Exception ex)
            {
                if (serverSocket.IsServerCloseException(ex))
                {
                    log.Debug("Accept socket has been closed from our side");
                }
                else
                {
                    log.Error("Unknown error, see exception for details", ex);
                }

                serverSocket.SafeClose();
            }
        }

        public event EventHandler<Socket> ClientConnect;

        protected virtual void OnClientConnect(Socket clientSocket)
        {
            var handler = ClientConnect;
            if (handler != null)
            {
                handler(this, clientSocket);
            }
        }
    }
}