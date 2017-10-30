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
        bool _connectedToGame;
        bool _connectedToServer;
        bool _moveId;
        int _gameId;
        string _gameServerAddress;
        int _gameServerPort;
        public ClientMessenger() : base(new TcpClient())
        {

        }

        private bool Connect()
        {
            _clientSocket.Connect("127.0.0.1", 1500);
            return true;
        }

        public bool ConnectToGameServer()
        {
            this.Connect();
            return false;
        }

        private void HandleAckMessage()
        {

        }

        public bool SendMove(object move)
        {
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

                    break;
                case 2:
                    var invalidMove = MessageDecoder.DecodeMessage<SharedObjects.InvalidMove>(stream);

                    break;
                case 3:
                    var joinGame = MessageDecoder.DecodeMessage<SharedObjects.JoinGame>(stream);

                    break;
                case 4:
                    var joinGameAccepted = MessageDecoder.DecodeMessage<SharedObjects.JoinGameAccepted>(stream);

                    break;
                case 5:
                    var moveAccepted = MessageDecoder.DecodeMessage<SharedObjects.MoveAccepted>(stream);
                    break;
                case 6:
                    var requestMove = MessageDecoder.DecodeMessage<SharedObjects.RequestMove>(stream);
                    break;
                case 7:
                    var gameMove = MessageDecoder.DecodeMessage<SharedObjects.GameMove>(stream);

                    break;
                case 8:
                    var listOfOpenGames = MessageDecoder.DecodeMessage<SharedObjects.ListOfOpenGames>(stream);

                    break;
            }
        }
    }
}
