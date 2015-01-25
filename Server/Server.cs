using System;
using System.Collections.Generic;
using System.Net.Sockets;
using log4net;

namespace Server
{
    internal class Server
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Server));
        private readonly List<ClientConnection> clientConnections = new List<ClientConnection>();
        private readonly Configuration configuration;
        private ClientListener listener;

        public Server(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public void Start()
        {
            listener = new ClientListener(configuration.Port);
            listener.ClientConnect += ListenerOnClientConnect;

            log.Info("Starting to listen for connections...");
            listener.Listen();

            if (configuration.InteractiveMode)
            {
                Console.Out.WriteLine("Press a key to disconnect one client");
                while (true)
                {
                    Console.ReadKey();
                    lock (clientConnections)
                    {
                        if (clientConnections.Count <= 0)
                        {
                            Console.Out.WriteLine("No clients connected!");
                            continue;
                        }

                        var client = clientConnections[0];
                        client.Stop();
                        clientConnections.Remove(client);
                    }
                }
            }
        }

        public void Stop()
        {
            log.Info("Shutting down server, disconnecting all clients...");

            listener.Stop();
            lock (clientConnections)
            {
                foreach (var client in clientConnections)
                {
                    client.Stop();
                }

                clientConnections.Clear();
            }
        }

        private void ListenerOnClientConnect(object sender, Socket socket)
        {
            log.Info("New client connected!");
            var client = new ClientConnection(socket);
            client.MessageReceived += ClientOnMessageReceived;
            client.ClientDisconnect += ClientOnClientDisconnect;

            lock (clientConnections)
            {
                clientConnections.Add(client);
            }

            client.Start();
        }

        private void ClientOnClientDisconnect(object sender, EventArgs eventArgs)
        {
            log.Info("Client disconnected, removing it from list!");

            lock (clientConnections)
            {
                clientConnections.Remove(sender as ClientConnection);
            }

            log.Info(string.Format("Total number of connected clients: {0}", clientConnections.Count));
        }

        private void ClientOnMessageReceived(object sender, string message)
        {
            log.Info(string.Format("Received message from client: {0}", message));
        }
    }
}