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
        public event SocketEvent SocketMainServerEventInfo;
        public delegate void ReceivedDataDelegateForLog(string logString);
        public event ReceivedDataDelegateForLog ReceivedDataLog;

        public PlayerLogicForGameServer(int listenPort)
        {
            AddGames();//TODO: for debug
            var localEp = new System.Net.IPEndPoint(System.Net.IPAddress.Any, listenPort);
            var myUdpClient = new System.Net.Sockets.UdpClient(localEp);
            myUdpClient.Client.ReceiveTimeout = 1000;
            this._serverComManager = new ComManager.ServerComManager(myUdpClient);
            this._serverComManager.SocketEventInfo += _serverComManager_SocketEventInfo; ;
            _cancellationTokenSource = new System.Threading.CancellationTokenSource();
            _clientMessenger = new GameCom.ClientMessenger(myUdpClient, _cancellationTokenSource.Token);
            _clientMessenger.SocketEventInfo += _serverComManager_SocketEventInfo;
            _clientMessenger.HandleRecievedMessage += HandleRecievedMessage;
            _serverComManager.ReceivedDataLog += _serverComManager_ReceivedDataLog;
            _serverComManager.SocketEventInfo += _serverComManager_SocketEventInfo1;
        }

        private void _serverComManager_SocketEventInfo1(string socketEvent)
        {
            SocketMainServerEventInfo?.Invoke(socketEvent);
        }

        private void _serverComManager_ReceivedDataLog(string logString)
        {
            ReceivedDataLog?.Invoke(logString);
        }

        private void _serverComManager_SocketEventInfo(string socketEvent)
        {
            SocketEventInfo?.Invoke(socketEvent);
        }

        private void AddGames()
        {
            var games = new TanksCommon.SharedObjects.ListOfOpenGames()
            {
                OpenGames = new List<TanksCommon.SharedObjects.OpenGame>()
                {
                    new TanksCommon.SharedObjects.OpenGame()
                    {
                        GameId = 1, MapId = 1, NumberOfPlayers = 5, PlayerCapacity = 3
                    }
                }
            };
            Game.GameLedger.Instance.ListOfOpenGames.Add(games);
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
            tcpThread = new System.Threading.Thread(() => this.ConnectToMainServerAndRegister(serverAddress, serverPort, gameReg));
            tcpThread.Start();
        }

        public void DisconnectFromMainServer()
        {
            this._clientMessenger.CloseConnection();
            tcpThread.Abort();
        }

        public void ConnectToMainServerAndRegister(string ipAddress, int port, TanksCommon.SharedObjects.GameServerRegister gameServerRegister)
        {
            _clientMessenger.AddUpdPeer(ipAddress, port);
            _log.Debug($"Sending GameReg: {gameServerRegister}");
            _clientMessenger.SendObjectToUdpPeers(gameServerRegister);
        }

        private void SendListOfGames()
        {
            _clientMessenger.SendObjectToTcpClient(new TanksCommon.SharedObjects.ListOfOpenGames());
        }

        private void HandleRecievedMessage(byte[] messageBytes)
        {
            var stream = new System.IO.MemoryStream(messageBytes);
            short messageType = TanksCommon.MessageDecoder.DecodeMessageType(stream);
            switch (messageType)
            {
                case 0:
                    var ping = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.Ping>(stream);
                    _log.Debug($"Received Ping: {ping}");
                    ReceivedDataLog($"Received ping: {ping}");
                    break;
                case 1:
                    var gameStatus = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.GameStatus>(stream);
                    _log.Debug($"Received game status: {gameStatus}");
                    ReceivedDataLog($"Received game status: {gameStatus}");
                    break;
                case 2:
                    var invalidMove = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.InvalidMove>(stream);
                    _log.Debug($"Received invalidMove: {invalidMove}");
                    ReceivedDataLog($"Received invalidMove: {invalidMove}");
                    break;
                case 3:
                    var joinGame = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.JoinGame>(stream);
                    _log.Debug($"Received joinGame: {joinGame}");
                    ReceivedDataLog($"Received joinGame: {joinGame}");
                    break;
                case 4:
                    var joinGameAccepted = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.JoinGameAccepted>(stream);
                    _log.Debug($"Received joinGameAccepted: {joinGameAccepted}");
                    ReceivedDataLog($"Received joinGameAccepted: {joinGameAccepted}");
                    break;
                case 5:
                    var moveAccepted = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.MoveAccepted>(stream);
                    _log.Debug($"Received moveAccepted: {moveAccepted}");
                    ReceivedDataLog($"Received moveAccepted: {moveAccepted}");
                    break;
                case 6:
                    var requestMove = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.RequestMove>(stream);
                    _log.Debug($"Received requestMove: {requestMove}");
                    ReceivedDataLog($"Received requestMove: {requestMove}");
                    break;
                case 7:
                    var gameMove = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.GameMove>(stream);
                    _log.Debug($"Received gameMove {gameMove.MessageId}: {gameMove}");
                    ReceivedDataLog($"Received gameMove {gameMove.MessageId}: {gameMove}");
                    break;
                case 8:
                    var listOfOpenGames = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.ListOfOpenGames>(stream);
                    _log.Debug($"Received listOfOpenGames{listOfOpenGames.MessageId}: {listOfOpenGames}");
                    ReceivedDataLog($"Received listOfOpenGames{listOfOpenGames.MessageId}: {listOfOpenGames}");
                    break;
                case 9:
                    var RequestGames = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.RequestGames>(stream);
                    _log.Debug($"Received RequestGames{RequestGames.MessageId}: {RequestGames}");
                    ReceivedDataLog($"Received RequestGames{RequestGames.MessageId}: {RequestGames}");
                    SendListOfGames();
                    break;
                case 99:
                    var ack = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.DataReceived>(stream);
                    _log.Debug($"Received DataReceived: {ack.MessageId}");
                    ReceivedDataLog($"Received DataReceived: {ack.MessageId}");
                    break;
            }
        }
    }
}
