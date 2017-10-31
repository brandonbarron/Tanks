using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TanksCommon
{
    public class ServerMessenger : TheMessenger
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ServerMessenger));
        private readonly int _clientId;

        public ServerMessenger(TcpClient clientSocket, int clientId) : base(clientSocket)
        {
            this._clientId = clientId;
            Thread thread = new Thread(() => GetStream(clientSocket));
            thread.Start();
            //GetStream(clientSocket);
        }

        public void GetStream(TcpClient clientSocket)
        {
            var keepGoing = true;
            while (true) { 
                NetworkStream stream = clientSocket.GetStream();
                keepGoing = ReceiveDataFromClient(stream);
            }
        }

        public bool SendOpenGames(int clientId, object[] games)
        {
            return false;
        }
        public bool HandleHeartbeat(int clientId)
        {
            return false;
        }

        public bool AcceptMove(int clientId)
        {
            return false;
        }

        public bool AcceptJoinGame(int clientId)
        {
            return false;
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
