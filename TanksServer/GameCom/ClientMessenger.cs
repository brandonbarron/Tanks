using log4net;
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
        public ClientMessenger(UdpClient udpClient) : base(new TcpClient())
        {
            log4net.Config.XmlConfigurator.Configure();
            _udpSender = new UdpSender(udpClient);
            testConnect = false;
        }

        private bool Connect(string ipAddress, int port)
        {
            try
            {
                _clientSocket.Connect(ipAddress, port);
                SocketEventInfo("Connected");
                testConnect = true;
                return true;
            }
            catch
            {
                SocketEventInfo("Failed");
                testConnect = false;
                return false;
            }
        }

        public void ConnectToGameServer(string ipAddress, int port)
        {
            this.Connect(ipAddress, port);
        }

        public void ConnectToMainServerAndRegister(string ipAddress, int port, TanksCommon.SharedObjects.GameServerRegister gameServerRegister)
        {
            _udpSender.AddPeer(ipAddress, port);
            _log.Debug($"Sending GameReg: {gameServerRegister}");
            this._udpSender.SendObjectToPeers(gameServerRegister);
        }

        public void StopGame()
        {
            _clientSocket.Close();
            _clientSocket.Dispose();
        }

        private void HandleAckMessage()
        {

        }

        public bool SendMove(TanksCommon.SharedObjects.GameMove move)
        {
            _log.Debug($"Sending Move: {move}");
            this.SendObjectToTcpClient(move);
            return false;
        }

        public bool JoinGame(int gameId)
        {
            return false;
        }

        public void GetGames()
        {
            _log.Debug("Asking Main Server for games.");
            this.SendObjectToTcpClient(new TanksCommon.SharedObjects.RequestGames());
        }
        public int GetGameStatus()
        {
            return 0;
        }

        protected override void HandleRecievedMessage(byte[] messageBytes)
        {
            var stream = new MemoryStream(messageBytes);
            short messageType = TanksCommon.MessageDecoder.DecodeMessageType(stream);
            switch (messageType)
            {
                case 1:
                    var gameStatus = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.GameStatus>(stream);
                    _log.Debug($"Received game status: {gameStatus}");

                    break;
                case 2:
                    var invalidMove = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.InvalidMove>(stream);
                    _log.Debug($"Received invalidMove: {invalidMove}");

                    break;
                case 3:
                    var joinGame = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.JoinGame>(stream);
                    _log.Debug($"Received joinGame: {joinGame}");

                    break;
                case 4:
                    var joinGameAccepted = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.JoinGameAccepted>(stream);
                    _log.Debug($"Received joinGameAccepted: {joinGameAccepted}");

                    break;
                case 5:
                    var moveAccepted = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.MoveAccepted>(stream);
                    _log.Debug($"Received moveAccepted: {moveAccepted}");

                    break;
                case 6:
                    var requestMove = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.RequestMove>(stream);
                    _log.Debug($"Received requestMove: {requestMove}");

                    break;
                case 7:
                    var gameMove = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.GameMove>(stream);
                    _log.Debug($"Received gameMove: {gameMove}");

                    break;
                case 8:
                    var listOfOpenGames = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.ListOfOpenGames>(stream);
                    _log.Debug($"Received listOfOpenGames: {listOfOpenGames}");

                    break;
            }
        }
    }
}
