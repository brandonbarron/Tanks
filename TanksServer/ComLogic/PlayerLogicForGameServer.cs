using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComLogic
{
    
    public class PlayerLogicForGameServer
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(PlayerLogicForGameServer));
        private readonly ComManager.ServerComManager _serverComManager;
        private readonly GameCom.ClientMessenger _clientMessenger;
        private readonly System.Threading.CancellationTokenSource _cancellationTokenSource;
        private System.Threading.Thread tcpThread;
        private int _listeningPort;

        public delegate void SocketEvent(string socketEvent);
        public event SocketEvent SocketEventInfo;
        public delegate void ReceivedDataDelegateForLog(string logString);
        public event ReceivedDataDelegateForLog ReceivedDataLog;

        public PlayerLogicForGameServer(int listenPort)
        {
            var localEp = new System.Net.IPEndPoint(System.Net.IPAddress.Any, listenPort);
            var myUdpClient = new System.Net.Sockets.UdpClient(localEp);
            myUdpClient.Client.ReceiveTimeout = 1000;
            this._serverComManager = new ComManager.ServerComManager(myUdpClient);
            this._serverComManager.SocketEventInfo += _serverComManager_SocketEventInfo; ;
            _cancellationTokenSource = new System.Threading.CancellationTokenSource();
            _clientMessenger = new GameCom.ClientMessenger(myUdpClient, _cancellationTokenSource.Token);
            _clientMessenger.SocketEventInfo += _serverComManager_SocketEventInfo;
            _serverComManager.ReceivedDataLog += _serverComManager_ReceivedDataLog;
        }

        private void _serverComManager_ReceivedDataLog(string logString)
        {
            ReceivedDataLog?.Invoke(logString);
        }

        private void _serverComManager_SocketEventInfo(string socketEvent)
        {
            SocketEventInfo?.Invoke(socketEvent);
        }

        public void SetListeningPort(int port)
        {
            _listeningPort = port;
        }

        public void StartServer()
        {
            _log.Debug("starting game server");
            System.Threading.Thread t = new System.Threading.Thread(() => this._serverComManager.Start(_listeningPort, _cancellationTokenSource.Token));
            t.Start();
        }

        public void StopServer()
        {
            _cancellationTokenSource.Cancel();
        }

        public void ConnectToMainServer(string serverAddress, int serverPort)
        {
            _log.Debug("Connecting to Main Server");
            var gameReg = new TanksCommon.SharedObjects.GameServerRegister()
            {
                OpenGames = new System.Collections.Generic.List<TanksCommon.SharedObjects.OpenGame>()
                {
                    new TanksCommon.SharedObjects.OpenGame()
                    {
                        GameId = 1, MapId = 1, NumberOfPlayers = 5, PlayerCapacity = 3
                    }
                }
            };
            tcpThread = new System.Threading.Thread(() => this._clientMessenger.ConnectToMainServerAndRegister(serverAddress, serverPort, gameReg));
            tcpThread.Start();
        }

        public void DisconnectFromMainServer()
        {
            this._clientMessenger.CloseConnection();
            tcpThread.Abort();
        }



    }
}
