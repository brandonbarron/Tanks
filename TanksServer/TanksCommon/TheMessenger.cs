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
        protected abstract void HandleRecievedMessage(byte[] messageBytes);
        protected readonly TcpClient _clientSocket;
        protected TheMessenger(TcpClient tcpSocket)
        {
            this._clientSocket = tcpSocket;
        }
        protected void SendToServer(byte[] messageBytes)
        {
            NetworkStream networkStream = _clientSocket.GetStream();
            networkStream.Write(messageBytes, 0, messageBytes.Length);
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
            NetworkStream networkStream = _clientSocket.GetStream();
            networkStream.Write(messageBytes, 0, messageBytes.Length);
            networkStream.Flush();
        }
    }
}
