using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using log4net;
using log4net.Config;

namespace Client
{
    internal class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Program));

        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var configuration = new Configuration
            {
                Hostname = "localhost",
                ServerPort = 2400
            };

            log.Info(string.Format("Connecting to {0} on port {1}", configuration.Hostname, configuration.ServerPort));

            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(configuration.Hostname, configuration.ServerPort);
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    var payload = Encoding.UTF8.GetBytes("Hello from client!");
                    writer.Write(IPAddress.HostToNetworkOrder(payload.Length));
                    writer.Write(payload);

                    socket.Send(stream.GetBuffer(), (int) stream.Length, SocketFlags.None);
                }
            }
            //Thread.Sleep(100);

            Console.Out.WriteLine("Press a key to continue and close the socket");
            Console.ReadKey();

            log.Debug("Closing socket");
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}