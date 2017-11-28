using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComLogic
{
    public class PlayerLogicForMainServer
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(PlayerLogicForMainServer));
        private readonly GameCom.ClientMessenger _mainServerMessenger;
        private System.Threading.Thread mainServerTcpThread;
        private readonly System.Net.Sockets.UdpClient _myUdpClient;
        private readonly System.Threading.CancellationTokenSource _cancellationTokenSource;


        private string _serverIp;
        private int _serverPort;

        public delegate void SocketEvent(string socketEvent);
        public event SocketEvent SocketEventInfo;
        public delegate void RecievedOpenGames(TanksCommon.SharedObjects.ListOfOpenGames games);
        public event RecievedOpenGames RecievedOpenGamesEvent;
        public delegate void ReceivedDataDelegateForLog(string logString);
        public event ReceivedDataDelegateForLog ReceivedDataLog;

        public PlayerLogicForMainServer(System.Net.Sockets.UdpClient udpClient)
        {
            _cancellationTokenSource = new System.Threading.CancellationTokenSource();
            _myUdpClient = udpClient;
            _mainServerMessenger = new GameCom.ClientMessenger(_myUdpClient, _cancellationTokenSource.Token);
            _mainServerMessenger.SocketEventInfo += _mainServerMessenger_SocketEventInfo;

            GameCom.ClientMessenger.ReceivedDataLog += ClientMessenger_ReceivedDataLog;
            _mainServerMessenger.HandleRecievedMessage += HandleRecievedMessage;
        }
        private void _mainServerMessenger_SocketEventInfo(string socketEvent)
        {
            SocketEventInfo?.Invoke(socketEvent);
        }

        private void ClientMessenger_ReceivedDataLog(string logString)
        {
            ReceivedDataLog?.Invoke(logString);
        }

        public void SetServer(string ip, int port)
        {
            _serverIp = ip;
            _serverPort = port;
        }

        public void ConnectToMainServer()
        {
            _log.Debug("Connecting To Main Server");
            mainServerTcpThread = new System.Threading.Thread(() => this.ConnectToMainServer(_serverIp, _serverPort));
            mainServerTcpThread.Start();
        }

        public void ConnectToMainServer(string ipAddress, int port)
        {
            _mainServerMessenger.Connect(ipAddress, port);
            this.SendPing();
        }

        public void DisconnectMainServer()
        {
            _log.Debug("Dis-Connecting from Main server");
            _cancellationTokenSource.Cancel();
            this._mainServerMessenger.CloseConnection();
            mainServerTcpThread.Abort();
        }

        public void GetOpenGameServers()
        {
            _log.Debug("Asking Main Server for games.");
            _mainServerMessenger.SendObjectToTcpClient(new TanksCommon.SharedObjects.RequestGames());
        }

        public void ConnectToMainServerAndRegister(string ipAddress, int port, TanksCommon.SharedObjects.GameServerRegister gameServerRegister)
        {
            _mainServerMessenger.AddUpdPeer(ipAddress, port);
            _log.Debug($"Sending GameReg: {gameServerRegister}");
            this._mainServerMessenger.SendObjectToUdpPeers(gameServerRegister);
        }

        private void SendPing()
        {
            _log.Debug("Sending Ping");
            _mainServerMessenger.SendObjectToTcpClient(new TanksCommon.SharedObjects.Ping() { PlayerId = 0 });
        }
        
        public int GetGameStatus()
        {
            return 0;
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
                    RecievedOpenGamesEvent(listOfOpenGames);
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
