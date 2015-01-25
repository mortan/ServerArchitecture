using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using log4net;

namespace Server
{
    public class ClientConnection
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (ClientConnection));
        private readonly Socket socket;

        public ClientConnection(Socket socket)
        {
            this.socket = socket;
        }

        public void Start()
        {
            new Thread(ReadLoop).Start();
        }

        public void Stop()
        {
            socket.SafeClose();
        }

        protected virtual void ReadLoop()
        {
            try
            {
                while (true)
                {
                    var buffer = new byte[4];
                    socket.ReceiveAll(4, buffer);

                    var messageSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0));
                    buffer = new byte[messageSize];
                    socket.ReceiveAll(messageSize, buffer);

                    var message = Encoding.UTF8.GetString(buffer);
                    OnMessageReceived(message);
                }
            }
            catch (Exception ex)
            {
                bool notify = true;
                if (ex is SocketExtensions.RemoteHostDisconnectException)
                {
                    log.Debug("Client closed the connection to the server!");
                }
                else if (socket.IsServerCloseException(ex))
                {
                    log.Debug("Client connection was closed from our side");
                    notify = false;
                }
                else
                {
                    log.Error("Unknown error while reading data, see exception for details", ex);
                }

                socket.SafeClose();

                if (notify)
                {
                    OnClientDisconnect();
                }
            }
        }

        public event EventHandler<string> MessageReceived;
        public event EventHandler ClientDisconnect;

        protected virtual void OnMessageReceived(string message)
        {
            var handler = MessageReceived;
            if (handler != null) handler(this, message);
        }

        protected virtual void OnClientDisconnect()
        {
            var handler = ClientDisconnect;
            if (handler != null) handler(this, null);
        }
    }
}