using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class TheGame
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(TheGame));
        private readonly GameCom.ServerMessenger _serverMessenger;
        public TheGame(System.Net.Sockets.TcpClient clientSocket, int clientId, System.Threading.CancellationToken token)
        {
            _serverMessenger = new GameCom.ServerMessenger(clientSocket, clientId, token);//TODO: this would be the game manager instead
            _serverMessenger.HandleRecievedMessage += HandleRecievedMessage;
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
                    _serverMessenger.CallReceivedDataLog($"Received ping: {ping}");
                    _serverMessenger.SendObjectToTcpClient(new TanksCommon.SharedObjects.Ping() { PlayerId = 0 });
                    break;
                case 1:
                    var gameStatus = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.GameStatus>(stream);
                    _log.Debug($"Received game status: {gameStatus}");
                    _serverMessenger.CallReceivedDataLog($"Received game status: {gameStatus}");
                    break;
                case 2:
                    var invalidMove = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.InvalidMove>(stream);
                    _log.Debug($"Received invalidMove: {invalidMove}");
                    _serverMessenger.CallReceivedDataLog($"Received invalidMove: {invalidMove}");
                    break;
                case 3:
                    var joinGame = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.JoinGame>(stream);
                    _log.Debug($"Received joinGame: {joinGame}");
                    _serverMessenger.CallReceivedDataLog($"Received joinGame: {joinGame}");
                    break;
                case 4:
                    var joinGameAccepted = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.JoinGameAccepted>(stream);
                    _log.Debug($"Received joinGameAccepted: {joinGameAccepted}");
                    _serverMessenger.CallReceivedDataLog($"Received joinGameAccepted: {joinGameAccepted}");
                    break;
                case 5:
                    var moveAccepted = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.MoveAccepted>(stream);
                    _log.Debug($"Received moveAccepted: {moveAccepted}");
                    _serverMessenger.CallReceivedDataLog($"Received moveAccepted: {moveAccepted}");
                    break;
                case 6:
                    var requestMove = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.RequestMove>(stream);
                    _log.Debug($"Received requestMove: {requestMove}");
                    _serverMessenger.CallReceivedDataLog($"Received requestMove: {requestMove}");
                    break;
                case 7:
                    var gameMove = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.GameMove>(stream);
                    _log.Debug($"Received gameMove: {gameMove}");
                    _serverMessenger.CallReceivedDataLog($"Received gameMove: {gameMove}");
                    break;
                case 8:
                    var listOfOpenGames = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.ListOfOpenGames>(stream);
                    _log.Debug($"Received listOfOpenGames: {listOfOpenGames}");
                    _serverMessenger.CallReceivedDataLog($"Received listOfOpenGames: {listOfOpenGames}");
                    AddGamesToLedger(listOfOpenGames);
                    break;
                case 9:
                    var requestOpenGames = TanksCommon.MessageDecoder.DecodeMessage<TanksCommon.SharedObjects.RequestGames>(stream);
                    _log.Debug($"Received requestOpenGames: {requestOpenGames}");
                    _serverMessenger.CallReceivedDataLog($"Received requestOpenGames: {requestOpenGames}");
                    SendOpenGamesToPlayer();
                    break;
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

        private void AddGamesToLedger(TanksCommon.SharedObjects.ListOfOpenGames games)
        {
            Game.GameLedger.Instance.ListOfOpenGames.Add(games);
        }

        private void SendOpenGamesToPlayer()
        {
            _log.Debug("Sending open games to player");
            var allGames = new TanksCommon.SharedObjects.ListOfOpenGames();
            foreach (var gameList in Game.GameLedger.Instance.ListOfOpenGames)
            {
                foreach (var game in gameList.OpenGames)
                {
                    allGames.OpenGames.Add(game);
                }
            }
            allGames.OpenGames = Game.GameLedger.Instance.ListOfOpenGames[0].OpenGames;
            _serverMessenger.SendObjectToTcpClient(allGames);
        }
    }
}
