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
        public delegate void ClientNotResponding();
        public event ClientNotResponding ClientNotRespondingEvent;
        protected TcpClient _clientSocket;
        protected System.Threading.CancellationToken _cancelToken;
        private int _mesageId;
        private int _nextReceivedMessageId;
        private System.Collections.Concurrent.ConcurrentQueue<MessageInfo<TanksCommon.SharedObjects.IMessage>> _messageQueue;
        private System.Collections.Concurrent.ConcurrentDictionary<int, TanksCommon.SharedObjects.IMessage> _messageHistory;
        private System.Collections.Concurrent.ConcurrentDictionary<int, byte[]> _incommingMessages;
        protected TheMessenger(TcpClient tcpSocket, int clientId)
        {
            _clientId = clientId;
            _clientSocket = tcpSocket;
            _mesageId = 0;
            _nextReceivedMessageId = 0;
            _messageQueue = new System.Collections.Concurrent.ConcurrentQueue<MessageInfo<TanksCommon.SharedObjects.IMessage>>();
            _messageHistory = new System.Collections.Concurrent.ConcurrentDictionary<int, TanksCommon.SharedObjects.IMessage>();
            _incommingMessages = new System.Collections.Concurrent.ConcurrentDictionary<int, byte[]>();
            //start new thread to periodically check the queue and resend messages if nessisary
            System.Threading.Thread t = new System.Threading.Thread(() => CheckQueueForUnAcknowledMessages());
            t.Start();
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
                        return true;
                    }
                    AcknowledgeMessage((byte[])message.Skip(32).ToArray().Clone());
                    //check if right order
                    EnsureProperMessageOrder(message.Skip(32).ToArray());
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
            this._cancelToken = token;
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
            if(theObject == null)
            {
                _log.Error("Object to send is null");
                throw new ArgumentException("Object to send cannot be null", "theObject");
            }
            if (theObject.Id != 99) //ignore keeping track of the ack 
            {
                theObject.MessageId = _mesageId++;
                _messageHistory.TryAdd(theObject.MessageId, theObject);
                _messageQueue.Enqueue(new MessageInfo<TanksCommon.SharedObjects.IMessage>(theObject));
            }
            SendObjectToTcpClient_Imp(theObject, sendingFrom);
        }

        private void SendObjectToTcpClient_Imp<T>(T theObject, string sendingFrom ) where T : TanksCommon.SharedObjects.IMessage
        {
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
            var stream = new System.IO.MemoryStream(messageBytes);
            var message = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.DataReceived>(stream);
            if (message.Id == 900)
            {
                //resend message
                SendObjectToTcpClient(_messageHistory[message.MessageId]);
            }
            else if (message.Id != 99)
            {
                SendObjectToTcpClient(new TanksCommon.SharedObjects.DataReceived() { MessageId = message.MessageId, Id = 99 });
                
            }
            else 
            {
                //remove message from queue, marking it complete
                var top = new MessageInfo<TanksCommon.SharedObjects.IMessage>();
                if (_messageQueue.TryPeek(out top))
                {
                    if (top.OriginalMessage.MessageId == message.MessageId)
                    {
                        //success remove from queue
                        _messageQueue.TryDequeue(out top);
                    }
                }
                else
                {
                    _log.Error("Message not found in queue");
                }
            }
        }

        private void HandleHashFailed(byte[] messageBytes)
        {
            //retry message sending message when hashing failed
            var stream = new System.IO.MemoryStream(messageBytes);
            var message = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.DataReceived>(stream);
            SendObjectToTcpClient_Imp(new TanksCommon.SharedObjects.MessageResend() { MessageId = message.MessageId }, "HandleHashFailed");//bypass message id and queue logic
        }

        private void CheckQueueForUnAcknowledMessages()
        {
            //this will be running in a different thread checking the queue periodically
            while (!this._cancelToken.IsCancellationRequested)
            {
                System.Threading.Thread.Sleep(1500);
                if(!_messageQueue.IsEmpty)
                {
                    var top = new MessageInfo<TanksCommon.SharedObjects.IMessage>();
                    if (_messageQueue.TryPeek(out top))
                    {
                        //resend message
                        if (!top.RertyExceeded())
                        {
                            top.RetryCount++;
                            SendObjectToTcpClient(_messageHistory[top.OriginalMessage.MessageId]);
                        } else
                        {
                            _log.Error("Retry count exceeded for message: " + top.OriginalMessage.MessageId);
                            _messageQueue.TryDequeue(out top);//ignore for now
                            ClientNotRespondingEvent();
                        }
                    }
                    else
                    {
                        _log.Error("Message could not be read from queue");
                    }
                }
            }
        }

        private void EnsureProperMessageOrder(byte[] messageBytes)
        {
            var stream = new System.IO.MemoryStream(messageBytes);
            var message = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.DataReceived>(stream);
            _incommingMessages.TryAdd(message.MessageId, messageBytes);
            var justRecievedId = message.MessageId;
            if(!_incommingMessages.IsEmpty)
            {
                while(_incommingMessages.ContainsKey(_nextReceivedMessageId)) {
                    HandleRecievedMessage(_incommingMessages[_nextReceivedMessageId]);
                    _nextReceivedMessageId++;
                }
                if(_nextReceivedMessageId < justRecievedId)
                {
                    SendObjectToTcpClient_Imp(new TanksCommon.SharedObjects.MessageResend() { MessageId = _nextReceivedMessageId }, "EnsureProperMessageOrder");//bypass message id and queue logic
                }
            }

        }
    }
}
