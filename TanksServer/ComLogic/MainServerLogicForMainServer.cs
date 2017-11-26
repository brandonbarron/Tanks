using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComLogic
{
    public class MainServerLogicForMainServer
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(MainServerLogicForMainServer));
        private readonly ComManager.ServerComManager _serverComManager;
        private readonly System.Threading.CancellationTokenSource _cancellationTokenSource;
        private System.Threading.Thread _comManagerTcpThread;

        private int _listeningPort;


        public delegate void SocketEvent(string socketEvent);
        public event SocketEvent SocketEventInfo;
        public delegate void ReceivedDataNewGameSever(TanksCommon.SharedObjects.GameServerRegister gameServer);
        public event ReceivedDataNewGameSever NewGameServerConnected;
        public delegate void ReceivedDataDelegateForLog(string logString);
        public event ReceivedDataDelegateForLog ReceivedDataLog;

        public MainServerLogicForMainServer()
        {
            var localEp = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 1500);
            var myUdpClient = new System.Net.Sockets.UdpClient(localEp);
            myUdpClient.Client.ReceiveTimeout = 1000;
            this._serverComManager = new ComManager.ServerComManager(myUdpClient);
            
            _cancellationTokenSource = new System.Threading.CancellationTokenSource();

            GameCom.ServerMessenger.ReceivedDataLog += ServerMessenger_ReceivedDataLog;
            _serverComManager.SocketEventInfo += _serverComManager_SocketEventInfo; ;
            _serverComManager.NewGameServerConnected += _serverComManager_NewGameServerConnected;
            _serverComManager.ReceivedDataLog += ServerMessenger_ReceivedDataLog;
            
        }
        
        public void SetListeningPort(int port)
        {
            _listeningPort = port;
        }

        public void StartServer()
        {
            _log.Debug("starting server");
            _comManagerTcpThread = new System.Threading.Thread(() => this._serverComManager.Start(_listeningPort, _cancellationTokenSource.Token));
            _comManagerTcpThread.Start();
        }

        public void StopServer()
        {
            _log.Debug("Stopping server");
            _cancellationTokenSource.Cancel();
            _comManagerTcpThread.Abort();
        }

        private void ServerMessenger_ReceivedDataLog(string logString)
        {
            ReceivedDataLog?.Invoke(logString);
        }

        private void _serverComManager_NewGameServerConnected(TanksCommon.SharedObjects.GameServerRegister gameServer)
        {
            NewGameServerConnected?.Invoke(gameServer);
        }

        private void _serverComManager_SocketEventInfo(string socketEvent)
        {
            SocketEventInfo?.Invoke(socketEvent);
        }
        
    }
}
