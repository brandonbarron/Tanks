using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameCom;
using ComManager;

namespace TanksCommonTest
{
    [TestClass]
    public class GameComTest
    {
        static GameCom.ClientMessenger _clientMessenger;
        static ComManager.ServerComManager _serverCom;
        static GameCom.ServerMessenger _serverMessenger;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            ServerComManager.ServerClientForTest getClient;

            var localEp = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 1502);
            var localEp2 = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 1501);
            var myUdpClient = new System.Net.Sockets.UdpClient(localEp);
            var myUdpClient2 = new System.Net.Sockets.UdpClient(localEp2);
            var cancellationToken = new System.Threading.CancellationToken();
            _clientMessenger = new ClientMessenger(myUdpClient, cancellationToken);
            _serverCom = new ServerComManager(myUdpClient2);

            _serverCom.ReceivedServerClientForTest += getClient = (theClient) =>
            {
                _serverMessenger = theClient;
            };

            Task.Run(() => _serverCom.StartForTestOnly(localEp2.Port, cancellationToken));
            _clientMessenger.Connect("127.0.0.1", localEp2.Port);
            Thread.Sleep(500);
            _serverCom.ReceivedServerClientForTest -= getClient;
        }

        [TestMethod]
        public void TestConnection()
        {
            Assert.IsTrue(_clientMessenger.testConnect);
        }      

        [TestMethod]
        public void TestClassInit()
        {
            Assert.IsNotNull(_serverMessenger);
        }

        [TestMethod]
        public void MessageOrder()
        {
            TheMessenger.RecievedMessage theHandler;

            _clientMessenger.HandleRecievedMessage += theHandler = (messageBytes) =>
            {
                var stream = new System.IO.MemoryStream(messageBytes);
                short messageType = TanksCommon.MessageDecoder.DecodeMessageType(stream);
                var gameMove = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.GameMove>(stream);
                //should be getting 0 first if 1 was delivered first
                Assert.IsTrue(gameMove.MessageId == 0);
            };

            //will be expecting 0 but sending 1 first
            _serverMessenger.SendObjectToTcpClient_Imp(new TanksCommon.SharedObjects.GameMove() { MessageId = 1 }, "TestMethod");
            _serverMessenger.SendObjectToTcpClient_Imp(new TanksCommon.SharedObjects.GameMove() { MessageId = 0 }, "TestMethod");

            Thread.Sleep(200);
            _clientMessenger.HandleRecievedMessage -= theHandler;
        }

        [TestMethod]
        public void MessageRequestResend()
        {
            TheMessenger.ReceivedDataDelegateForLog theHandler;
            //bool eventHit = false;
            TheMessenger.ReceivedDataLog += theHandler = (messageText) =>
            {
                //should be asking to resend a message 
                if (messageText.Contains("Request"))
                {
                    Assert.IsTrue(messageText.Contains("Request to resend Message: "));
                }
                //eventHit = true;
            };

            //will be expecting 0 but sending 1 first
            _serverMessenger.SendObjectToTcpClient_Imp(new TanksCommon.SharedObjects.GameMove() { MessageId = 10 }, "TestMethod");
            Thread.Sleep(200);
            TheMessenger.ReceivedDataLog -= theHandler;
            //Assert.IsTrue(eventHit); -- This must be run individualy
        }

        [TestMethod]
        public void MessageHashFailed()
        {
            TheMessenger.ReceivedDataDelegateForLog theHandler;

            TheMessenger.ReceivedDataLog += theHandler = (messageText) =>
            {
                //should be asking to send message 8
                //this also works if we recieve out of order, see how we send 8
                Assert.IsTrue(messageText.Contains("Hash failed. Asking to resend Message: 8"));
            };

            using (var stream = new System.IO.MemoryStream())
            {
                var theObject = new TanksCommon.SharedObjects.GameMove() { MessageId = 8 };
                var messageStream = TanksCommon.MessageEncoder.EncodeMessage(stream, theObject);
                messageStream.Seek(0, System.IO.SeekOrigin.Begin);
                var messageBytes = messageStream.ToArray();
                var hash = new byte[32];//send all 0s (incorrect hash)
                System.Collections.Generic.List<byte> byteList = new System.Collections.Generic.List<byte>(hash);
                byteList.AddRange(messageBytes);

                _serverMessenger.SendDataToClient(byteList.ToArray());
            }
            Thread.Sleep(200);
            TheMessenger.ReceivedDataLog -= theHandler;
        }

        [TestMethod]
        public void Hash()
        {
            using (var stream = new System.IO.MemoryStream())
            {
                var theObject = new TanksCommon.SharedObjects.GameMove() { MessageId = 8 };
                var messageStream = TanksCommon.MessageEncoder.EncodeMessage(stream, theObject);
                messageStream.Seek(0, System.IO.SeekOrigin.Begin);
                var messageBytes = messageStream.ToArray();
                var hash = GameCom.Hash.HashData(messageBytes);

                Assert.IsNotNull(hash);
                Assert.IsTrue(hash.Length == 32);
            }
        }

        [TestMethod]
        public void HashAndCompare()
        {
            using (var stream = new System.IO.MemoryStream())
            using (var stream2 = new System.IO.MemoryStream())
            {
                var theObject = new TanksCommon.SharedObjects.GameMove() { MessageId = 8 };
                var messageStream = TanksCommon.MessageEncoder.EncodeMessage(stream, theObject);
                messageStream.Seek(0, System.IO.SeekOrigin.Begin);
                var messageBytes = messageStream.ToArray();
                var hash = GameCom.Hash.HashData(messageBytes);

                //slightly different object
                var theObject2 = new TanksCommon.SharedObjects.GameMove() { MessageId = 1 };
                var messageStream2 = TanksCommon.MessageEncoder.EncodeMessage(stream2, theObject2);
                messageStream2.Seek(0, System.IO.SeekOrigin.Begin);
                var messageBytes2 = messageStream2.ToArray();


                Assert.IsFalse(GameCom.Hash.HashAndCompare(messageBytes2, hash));
            }
        }
    }
}
