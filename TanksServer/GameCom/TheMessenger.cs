using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameCom
{
    abstract public class TheMessenger
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(TheMessenger));
        private readonly int _clientId;

        public delegate void ReceivedDataDelegateForLog(string logString);
        public static event ReceivedDataDelegateForLog ReceivedDataLog;
        public delegate void RecievedMessage(byte[] messageBytes);
        public event RecievedMessage HandleRecievedMessage;
        protected TcpClient _clientSocket;
        protected TheMessenger(TcpClient tcpSocket, int clientId)
        {
            _clientId = clientId;
            _clientSocket = tcpSocket;
        }

        protected bool ReceiveDataFromClient(NetworkStream stream)
        {
            try
            {
                stream.ReadTimeout = 500;
                var message = stream.ReadStreamMessage();
                if (message.Length > 0)
                {
                    HandleRecievedMessage(message);
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error while reading data -- {0}", ex.Message);
            }
            return false;
        }

        public void GetStream(System.Threading.CancellationToken token)
        {
            var keepGoing = true;
            while (/*keepGoing &&*/ !token.IsCancellationRequested)
            {
                try
                {
                    NetworkStream networkStream = _clientSocket.GetStream();
                    keepGoing = ReceiveDataFromClient(networkStream);
                }
                catch
                {
                    _log.Debug($"Connection closed by client, id: {_clientId}");
                    this.CallReceivedDataLog($"Connection closed by client, id: {_clientId}");
                    keepGoing = false;
                }
            }
        }

        protected void SendDataToClient(byte[] messageBytes)
        {
            _log.Debug("Sending data to client");
            NetworkStream networkStream = _clientSocket.GetStream();
            networkStream.WriteStreamMessage(messageBytes);
        }

        public void SendObjectToTcpClient<T>(T theObject, [System.Runtime.CompilerServices.CallerMemberName] string sendingFrom = "") where T : TanksCommon.SharedObjects.IMessage
        {
            using (var stream = new System.IO.MemoryStream())
            {
                _log.Debug($"Sending object from: {sendingFrom}");
                var messageStream = TanksCommon.MessageEncoder.EncodeMessage(stream, theObject);
                messageStream.Seek(0, System.IO.SeekOrigin.Begin);
                this.SendDataToClient(messageStream.ToArray());
            }
        }

        public void CallReceivedDataLog(string message)
        {
            ReceivedDataLog(message);
        }
        
    }
}
