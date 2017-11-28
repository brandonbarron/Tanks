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
        private int _mesageId;
        private System.Collections.Concurrent.ConcurrentQueue<MessageInfo<TanksCommon.SharedObjects.IMessage>> messageQueue;
        protected TheMessenger(TcpClient tcpSocket, int clientId)
        {
            _clientId = clientId;
            _clientSocket = tcpSocket;
            _mesageId = 0;
            messageQueue = new System.Collections.Concurrent.ConcurrentQueue<MessageInfo<TanksCommon.SharedObjects.IMessage>>();
            //TODO: start new thread to periodically check the queue and resend messages if nessisary
        }

        protected bool ReceiveDataFromClient(NetworkStream stream)
        {
            try
            {
                stream.ReadTimeout = 500;
                var message = stream.ReadStreamMessage();
                if (message != null && message.Length > 0)
                {
                    if (!Hash.HashAndCompare(message.Skip(32).ToArray(), message.Take(32).ToArray()))
                    {
                        _log.Error("hash not equal");
                        HandleHashFailed(message.Skip(32).ToArray());
                    }
                    AcknowledgeMessage((byte[])message.Skip(32).ToArray().Clone());
                    HandleRecievedMessage(message.Skip(32).ToArray());
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
            while (keepGoing && !token.IsCancellationRequested)
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
            //TODO: add message to message queeue
            SendObjectToTcpClient_Imp(theObject, sendingFrom);
        }

        private void SendObjectToTcpClient_Imp<T>(T theObject, string sendingFrom ) where T : TanksCommon.SharedObjects.IMessage
        {
            if (theObject.Id != 99)
            {
                theObject.MessageId = _mesageId++;
            }
            using (var stream = new System.IO.MemoryStream())
            {
                _log.Debug($"Sending object from: {sendingFrom}");
                var messageStream = TanksCommon.MessageEncoder.EncodeMessage(stream, theObject);
                messageStream.Seek(0, System.IO.SeekOrigin.Begin);
                var messageBytes = messageStream.ToArray();
                var hash = Hash.HashData(messageBytes);
                List<byte> byteList = new List<byte>(hash);
                byteList.AddRange(messageBytes);

                this.SendDataToClient(byteList.ToArray());
            }
        }

        public void CallReceivedDataLog(string message)
        {
            ReceivedDataLog(message);
        }
        
        private void AcknowledgeMessage(byte[] messageBytes)
        {
            //TODO: remove message from queue, marking it complete
            var stream = new System.IO.MemoryStream(messageBytes);
            var message = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.DataReceived>(stream);
            if (message.Id != 99)
            {
                SendObjectToTcpClient(new TanksCommon.SharedObjects.DataReceived() { MessageId = message.MessageId, Id = 99 });
            }
        }

        private void HandleHashFailed(byte[] messageBytes)
        {
            //TODO: retry message sending message when hashing failed
        }

        private void CheckQueueForUnAcknowledMessages()
        {
            //TODO: this will be running in a different thread checking the queue periodically
        }
    }
}
