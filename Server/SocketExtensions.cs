using System;
using System.Net.Sockets;

namespace Server
{
    public static class SocketExtensions
    {
        public static void ReceiveAll(this Socket socket, int totalBytes, byte[] buffer)
        {
            var bytesReadTotal = 0;
            while (bytesReadTotal < totalBytes)
            {
                var bytesRead = socket.Receive(buffer, bytesReadTotal, totalBytes - bytesReadTotal, SocketFlags.None);
                if (bytesRead == 0)
                {
                    throw new RemoteHostDisconnectException("The remote host closed the connection while reading data");
                }
                bytesReadTotal += bytesRead;
            }
        }

        public static void SafeClose(this Socket socket)
        {
            try
            {
                if (socket.IsBound)
                {
                    // Only need to shutdown implicitly bound sockets ("client" sockets)
                    socket.Shutdown(SocketShutdown.Both);
                }
            }
            catch (Exception)
            {
                // Swallow exception because it's not useful
            }
            finally
            {
                try
                {
                    socket.Close();
                }
                catch (Exception)
                {
                    // Nothing to do
                }
            }
        }

        public static bool IsServerCloseException(this Socket socket, Exception ex)
        {
            return ex is ObjectDisposedException ||
                   (ex is SocketException && ((SocketException) ex).SocketErrorCode == SocketError.Interrupted);
        }

        public class RemoteHostDisconnectException : Exception
        {
            public RemoteHostDisconnectException()
            {
            }

            public RemoteHostDisconnectException(string message) : base(message)
            {
            }

            public RemoteHostDisconnectException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }
    }
}