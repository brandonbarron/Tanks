using GameCom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComManager
{
    public class ServerComManager
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ServerComManager));
        public delegate void SocketEvent(string socketEvent);
        public event SocketEvent SocketEventInfo;
        private readonly UdpReceiver _udpReceiver;
        public delegate void ReceivedDataNewGameSever(TanksCommon.SharedObjects.GameServerRegister gameServer);
        public event ReceivedDataNewGameSever NewGameServerConnected;
        public delegate void ReceivedDataDelegateForLog(string logString);
        public event ReceivedDataDelegateForLog ReceivedDataLog;
        System.Threading.Thread _udpThread;
        public ServerComManager(System.Net.Sockets.UdpClient udpClient)
        {

            log4net.Config.XmlConfigurator.Configure();
            _udpReceiver = new UdpReceiver(udpClient);
            _udpReceiver.ReceivedData += _udpReceiver_ReceivedData;
        }

        public void Start(int port, System.Threading.CancellationToken token)
        {
            _udpThread = new System.Threading.Thread(() => _udpReceiver.ReceiveStuff(token));
            _udpThread.Start();

            System.Net.Sockets.TcpListener serverSocket = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, port);
            SocketEventInfo?.Invoke("Listening");
            System.Net.Sockets.TcpClient clientSocket = default(System.Net.Sockets.TcpClient);
            int clientId = 0;

            serverSocket.Start();

            clientId = 0;

            while (!token.IsCancellationRequested)
            {
                clientId += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                _log.Debug($"New TCP Client connected");
                System.Threading.Thread thread = new System.Threading.Thread(() => StartServerMessenger(clientSocket, clientId, token));
                thread.Start();
            }
            clientSocket.Close();
            serverSocket.Stop();
            SocketEventInfo?.Invoke("Closed");
        }

        public void StartServerMessenger(System.Net.Sockets.TcpClient clientSocket, int clientId, System.Threading.CancellationToken token)
        {
            //var client = new ServerMessenger(clientSocket, clientId, token);//TODO: this would be the game manager instead
            var Game = new Game.TheGame(clientSocket, clientId, token);
        }

        private void _udpReceiver_ReceivedData(byte[] messageBytes)
        {
            var stream = new System.IO.MemoryStream(messageBytes);
            short messageType = TanksCommon.MessageDecoder.DecodeMessageType(stream);
            switch (messageType)
            {
                case 100:
                    var gameRegisterInfo = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.GameServerRegister>(stream);
                    _log.Debug($"Received gameRegisterInfo: {gameRegisterInfo}");
                    ReceivedDataLog($"Received gameRegisterInfo: {gameRegisterInfo}");
                    NewGameServerConnected(gameRegisterInfo);
                    break;
            }
        }
    }
}
