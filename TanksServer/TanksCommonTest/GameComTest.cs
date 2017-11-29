﻿using System;
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

        [TestMethod]
        public void TestConnection()
        {
            var localEp = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 1502);
            var localEp2 = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 1501);
            var myUdpClient = new System.Net.Sockets.UdpClient(localEp);
            var myUdpClient2 = new System.Net.Sockets.UdpClient(localEp2);
            var cancellationToken = new System.Threading.CancellationToken();
            GameCom.ClientMessenger clientMessenger = new ClientMessenger(myUdpClient, cancellationToken);
            ComManager.ServerComManager serverCom = new ServerComManager(myUdpClient2);

            Task.Run(() => serverCom.Start(localEp2.Port, cancellationToken));
            clientMessenger.Connect(localEp2.Address.ToString(), localEp2.Port);

            //Assert.IsTrue(clientMessenger.testConnect);

        }
    }
}
