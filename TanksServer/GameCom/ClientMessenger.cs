﻿using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameCom
{
    public class ClientMessenger : TheMessenger
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ClientMessenger));
        
        public delegate void SocketEvent(string socketEvent);
        public event SocketEvent SocketEventInfo;

        private readonly UdpSender _udpSender;
        public bool testConnect;
        public ClientMessenger(UdpClient udpClient, System.Threading.CancellationToken token) : base(new TcpClient(), 123, false)
        {
            _cancelToken = token;
            log4net.Config.XmlConfigurator.Configure();
            _udpSender = new UdpSender(udpClient);
            testConnect = false;
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                _clientSocket.Connect(ipAddress, port);
                SocketEventInfo?.Invoke("Connected");
                testConnect = true;
                System.Threading.Thread thread = new System.Threading.Thread(() => GetStream(_cancelToken));
                thread.Start();
                //var publicKey = encryptioinKeys.ExportPublicKey();
                //System.Threading.Thread.Sleep(100);
                //_log.Debug("Sending RSA Public Key");
                //CallReceivedDataLog("Sending RSA Public Key");
                //SendObjectToTcpClient(new TanksCommon.Encryption.RsaPublicKey() { Key = publicKey });
                return true;
            }
            catch
            {
                SocketEventInfo?.Invoke("Failed");
                testConnect = false;
                return false;
            }
        }
       
        public void AddUpdPeer(string ipAddress, int port)
        {
            _udpSender.AddPeer(ipAddress, port);
        }

        public void SendObjectToUdpPeers<T>(T theObject) where T : TanksCommon.SharedObjects.IMessage
        {
            _udpSender.SendObjectToPeers(theObject);
        }

        public void CloseConnection()
        {
            _clientSocket.Close();
            _clientSocket.Dispose();
        }

        
    }
}
