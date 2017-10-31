using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon
{
    public class ClientMessenger : TheMessenger
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ClientMessenger));
        bool _connectedToGame;
        bool _connectedToServer;
        bool _moveId;
        int _gameId;
        string _gameServerAddress;
        int _gameServerPort;
        public delegate void SocketEvent(string socketEvent);
        public event SocketEvent SocketEventInfo;
        public ClientMessenger() : base(new TcpClient())
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        private bool Connect(string ipAddress, int port)
        {
            try
            {
                _clientSocket.Connect(ipAddress, port);
                SocketEventInfo("Connected");
                return true;
            }
            catch
            {
                SocketEventInfo("Failed");
                return false;
            }
        }

        public void ConnectToGameServer(string ipAddress, int port)
        {
            this.Connect(ipAddress, port);
        }

        public void StopGame()
        {
            _clientSocket.Close();
            _clientSocket.Dispose();
        }

        private void HandleAckMessage()
        {

        }

        public bool SendMove(SharedObjects.GameMove move)
        {
            using(var stream = new MemoryStream())
            {
                _log.Debug($"Sending Move: {move}");
                var messageStream = MessageEncoder.EncodeMessage(stream, move);
                messageStream.Seek(0, SeekOrigin.Begin);
                this.SendDataToClient(messageStream.ToArray());
            }
            return false;
        }

        public bool JoinGame(int gameId)
        {
            return false;
        }

        public object[] GetGames()
        {
            return null;
        }
        public int GetGameStatus()
        {
            return 0;
        }

        protected override void HandleRecievedMessage(byte[] messageBytes)
        {
            var stream = new MemoryStream(messageBytes);
            short messageType = MessageDecoder.DecodeMessageType(stream);
            switch (messageType)
            {
                case 1:
                    var gameStatus = MessageDecoder.DecodeMessage<SharedObjects.GameStatus>(stream);
                    _log.Debug($"Received game status: {gameStatus}");

                    break;
                case 2:
                    var invalidMove = MessageDecoder.DecodeMessage<SharedObjects.InvalidMove>(stream);
                    _log.Debug($"Received invalidMove: {invalidMove}");

                    break;
                case 3:
                    var joinGame = MessageDecoder.DecodeMessage<SharedObjects.JoinGame>(stream);
                    _log.Debug($"Received joinGame: {joinGame}");

                    break;
                case 4:
                    var joinGameAccepted = MessageDecoder.DecodeMessage<SharedObjects.JoinGameAccepted>(stream);
                    _log.Debug($"Received joinGameAccepted: {joinGameAccepted}");

                    break;
                case 5:
                    var moveAccepted = MessageDecoder.DecodeMessage<SharedObjects.MoveAccepted>(stream);
                    _log.Debug($"Received moveAccepted: {moveAccepted}");

                    break;
                case 6:
                    var requestMove = MessageDecoder.DecodeMessage<SharedObjects.RequestMove>(stream);
                    _log.Debug($"Received requestMove: {requestMove}");

                    break;
                case 7:
                    var gameMove = MessageDecoder.DecodeMessage<SharedObjects.GameMove>(stream);
                    _log.Debug($"Received gameMove: {gameMove}");

                    break;
                case 8:
                    var listOfOpenGames = MessageDecoder.DecodeMessage<SharedObjects.ListOfOpenGames>(stream);
                    _log.Debug($"Received listOfOpenGames: {listOfOpenGames}");

                    break;
            }
        }
    }
}
