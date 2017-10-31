using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon
{
    abstract public class TheMessenger
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(TheMessenger));
        protected abstract void HandleRecievedMessage(byte[] messageBytes);
        protected readonly TcpClient _clientSocket;
        protected TheMessenger(TcpClient tcpSocket)
        {
            this._clientSocket = tcpSocket;
        }

        protected bool ReceiveDataFromClient(NetworkStream stream)
        {
            var buffer = new byte[1024];

            var keepGoing = true;
            var stayConnected = true;

            while (stayConnected)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        _log.Debug("Read data from buffer");
                        HandleRecievedMessage(buffer);
                    }
                }
                catch
                {
                    stayConnected = false;
                }
            }
            return keepGoing;
        }

        protected void SendDataToClient(byte[] messageBytes)
        {
            _log.Debug("Sending data to client");
            NetworkStream networkStream = _clientSocket.GetStream();
            networkStream.Write(messageBytes, 0, messageBytes.Length);
            networkStream.Flush();
        }
    }
}
