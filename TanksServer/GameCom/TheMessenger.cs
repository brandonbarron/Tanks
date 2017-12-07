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
        protected readonly bool _isServer;

        protected TanksCommon.Encryption.EncryptioinKeys encryptioinKeys = new TanksCommon.Encryption.EncryptioinKeys();
        private Encrypt aesEncrypter;

        public delegate void ReceivedDataDelegateForLog(string logString);
        public static event ReceivedDataDelegateForLog ReceivedDataLog;
        public delegate void RecievedMessage(byte[] messageBytes);
        public event RecievedMessage HandleRecievedMessage;
        public event RecievedMessage HandleDebugRecievedMessage;
        public delegate void ClientNotResponding();
        public event ClientNotResponding ClientNotRespondingEvent;
        protected TcpClient _clientSocket;
        protected System.Threading.CancellationToken _cancelToken;
        private int _mesageId;
        private int _nextReceivedMessageId;
        private System.Collections.Concurrent.ConcurrentQueue<MessageInfo<TanksCommon.SharedObjects.IMessage>> _messageQueue;
        private System.Collections.Concurrent.ConcurrentDictionary<int, TanksCommon.SharedObjects.IMessage> _messageHistory;
        private System.Collections.Concurrent.ConcurrentDictionary<int, byte[]> _incommingMessages;
        protected TheMessenger(TcpClient tcpSocket, int clientId, bool isServer)
        {
            _clientId = clientId;
            _clientSocket = tcpSocket;
            _isServer = isServer;
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
                    var decodedBytes = CheckEncryption(message.Skip(32).ToArray());
                    //var decodedBytes = message.Skip(33).ToArray();
                    if (!Hash.HashAndCompare(decodedBytes, message.Take(32).ToArray()))
                    {
                        _log.Error("hash not equal");
                        HandleHashFailed(decodedBytes.ToArray());
                        return true;
                    }
                    AcknowledgeMessage((byte[])decodedBytes.Clone());
                    //check if right order
                    EnsureProperMessageOrder((byte[])decodedBytes.Clone());
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error while reading data -- {0}", ex.Message);
            }
            return false;
        }

        private byte[] CheckEncryption(byte[] message)
        {
            if (System.Convert.ToBoolean(message[0]))
            {
                return Encrypt.DecryptBytes(encryptioinKeys, message.Skip(1).ToArray());
            }
            return message.Skip(1).ToArray();
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

        public void SendDataToClient(byte[] messageBytes)
        {
            _log.Debug("Sending data to client");
            NetworkStream networkStream = _clientSocket.GetStream();
            networkStream.WriteStreamMessage(messageBytes);
        }

        public void SendObjectToTcpClient<T>(T theObject, [System.Runtime.CompilerServices.CallerMemberName] string sendingFrom = "") where T : TanksCommon.SharedObjects.IMessage
        {
            if (theObject == null)
            {
                _log.Error("Object to send is null");
                throw new ArgumentException("Object to send cannot be null", "theObject");
            }
            //SendObjectToTcpClient_Imp(theObject, sendingFrom);
            if (theObject.Id != 99) //ignore keeping track of the ack 
            {
                theObject.MessageId = _mesageId++;
                _messageHistory.TryAdd(theObject.MessageId, theObject);
                _messageQueue.Enqueue(new MessageInfo<TanksCommon.SharedObjects.IMessage>(theObject));
            }
            SendObjectToTcpClient_Imp(theObject, sendingFrom);
        }

        public void SendObjectToTcpClient_Imp<T>(T theObject, string sendingFrom) where T : TanksCommon.SharedObjects.IMessage
        {
            using (var stream = new System.IO.MemoryStream())
            {
                _log.Debug($"Sending object from: {sendingFrom}");
                var messageStream = TanksCommon.MessageEncoder.EncodeMessage(stream, theObject);
                messageStream.Seek(0, System.IO.SeekOrigin.Begin);
                var messageBytes = messageStream.ToArray();
                var hash = Hash.HashData(messageBytes);
                List<byte> byteList = new List<byte>(hash);
                var didEncrypt = EncryptMessage(ref messageBytes, theObject.Id);
                byteList.Add(System.Convert.ToByte(didEncrypt));
                byteList.AddRange(messageBytes);

                this.SendDataToClient(byteList.ToArray());
            }
        }

        private bool EncryptMessage(ref byte[] messageBytes, int messageType)
        {
            if (!_isServer && messageType != 501 && aesEncrypter != null) {
                messageBytes = aesEncrypter.EncryptBytes(messageBytes);
                return true;
            }
            return false;
        }

        public void CallReceivedDataLog(string message)
        {
            ReceivedDataLog?.Invoke(message);
        }

        private void AcknowledgeMessage(byte[] messageBytes)
        {
            var stream = new System.IO.MemoryStream(messageBytes);
            var message = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.DataReceived>(stream);
            if (message.Id == 500)
            {
                SendObjectToTcpClient(new TanksCommon.SharedObjects.DataReceived() { MessageId = message.MessageId, Id = 99 });

                var rsaPublicKeys = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.Encryption.RsaPublicKey>(stream);
                encryptioinKeys.ImportPublicKey(rsaPublicKeys.Key);

                aesEncrypter = new Encrypt(encryptioinKeys);
                SendObjectToTcpClient(new TanksCommon.Encryption.AesPublicKey() { Iv = encryptioinKeys.Iv, SessionKey = encryptioinKeys.SessionKey });
                
            }
            else if (message.Id == 501)
            {
                SendObjectToTcpClient(new TanksCommon.SharedObjects.DataReceived() { MessageId = message.MessageId, Id = 99 });

                var aesPublicKeys = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.Encryption.AesPublicKey>(stream);
                encryptioinKeys.SetIvAndSessionKey(aesPublicKeys.Iv, aesPublicKeys.SessionKey);
            }
            else if (message.Id == 900)
            {
                //resend message
                HandleDebugRecievedMessage?.Invoke(messageBytes);
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
            ReceivedDataLog?.Invoke("Hash failed. Asking to resend Message: " + message.MessageId);
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
                            top.RetryCount++;//TODO: need to change the type from IMessage to actuall message type
                            //SendObjectToTcpClient(_messageHistory[top.OriginalMessage.MessageId]);
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
                    HandleRecievedMessage?.Invoke(_incommingMessages[_nextReceivedMessageId]);
                    _nextReceivedMessageId++;
                }
                if(_nextReceivedMessageId < justRecievedId)
                {
                    ReceivedDataLog?.Invoke("Request to resend Message: " + _nextReceivedMessageId);
                    _log.Debug("Request to resend Message: " + _nextReceivedMessageId);
                    SendObjectToTcpClient_Imp(new TanksCommon.SharedObjects.MessageResend() { MessageId = _nextReceivedMessageId }, "EnsureProperMessageOrder");//bypass message id and queue logic
                }
            }

        }



        /* TODO:
         * Background: 
         * The way this works is that server creates an encryption method that only server can decrypt.
         * Server sends "public" keys that can ONLY be used to encrypt.
         * There needs to be an exchange of two sets of keys: RSA and AES.
         * The server will send RSA public keys to client.
         * Client will then encrypt AES keys using the provided RSA public keys.
         * 
         * 
         * For encryption to be enabled, we must do the following:
         * Server creates TanksCommon.Encryption.EncryptioinKeys --creates RSA keys
         * RSA public keys are obtained by calling ExportPublicKeys()
         * Server sends client RSA public Keys -- we can use Encryption.RsaPublicKey as the message which has the id of 500
         * client Creates instatnce of TanksCommon.Encryption.EncryptioinKeys and imports server RSA Public keys with ImportPublicKey(..)
         * client makes instance of GameCom.Encrypt using local RSA encryption keys
         * client sends AES public keys to server -- we can use Encryption.AesPublicKey which has the id of 501
         * Server now sets the received public AES keys using SetIvAndSessionKey(...)
         * Server can now decrypt anything that the client sends using GameCom.Encrypt.DecryptBytes(...)
         * 
         * 
         * Implementation Ideas:
         * To keep things simlpe we can do communication is encrypted one way, client to server.
         * To keep things generic we can set a byte in the message stream to contain a boolean of if the message is encrypted
         *     and only decrypt if so
         * 
         * */
    }
}
