using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameCom
{
    /// <summary>
    /// This class will be the abstraction to handle multiple game communications
    /// </summary>
    public class ServerComManager
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ServerComManager));
        public delegate void SocketEvent(string socketEvent);
        public event SocketEvent SocketEventInfo;
        private readonly UdpReceiver _udpReceiver;
        public delegate void ReceivedDataNewGameSever(TanksCommon.SharedObjects.GameServerRegister gameServer);
        public event ReceivedDataNewGameSever NewGameServerConnected;
        public delegate void ReceivedDataDelegateForLog(string logString);
        public event ReceivedDataDelegateForLog ReceivedDataLog;
        Thread _udpThread;
        public ServerComManager(UdpClient udpClient)
        {
            
            log4net.Config.XmlConfigurator.Configure();
            _udpReceiver = new UdpReceiver(udpClient);
            _udpReceiver.ReceivedData += _udpReceiver_ReceivedData;
        }

        public void Start(int port, CancellationToken token) {
            _udpThread = new Thread(() => _udpReceiver.ReceiveStuff(token));
            _udpThread.Start();

            TcpListener serverSocket = new TcpListener(System.Net.IPAddress.Any, port);
            SocketEventInfo("Listening");
            System.Net.Sockets.TcpClient clientSocket = default(System.Net.Sockets.TcpClient);
            int clientId = 0;

            serverSocket.Start();

            clientId = 0;

            while (!token.IsCancellationRequested) {
                clientId += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                _log.Debug($"New TCP Client connected");
                Thread thread = new Thread(() => StartServerMessenger(clientSocket, clientId, token));
                thread.Start();
            }
            clientSocket.Close();
            serverSocket.Stop();
            SocketEventInfo("Closed");
        }

        public void StartServerMessenger(System.Net.Sockets.TcpClient clientSocket, int clientId, CancellationToken token) {
            var client = new ServerMessenger(clientSocket, clientId, token);//TODO: this would be the game manager instead
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
