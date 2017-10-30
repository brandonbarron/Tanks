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
        private readonly int _clientId;

        public ServerMessenger(TcpClient clientSocket, int clientId) : base(clientSocket)
        {
            this._clientId = clientId;
            Thread thread = new Thread(() => GetStream(clientSocket));
            thread.Start();
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
                    var ameMove = MessageDecoder.DecodeMessage<SharedObjects.GameMove>(stream);

                    break;
                case 8:
                    var listOfOpenGames = MessageDecoder.DecodeMessage<SharedObjects.ListOfOpenGames>(stream);

                    break;
            }
        }
        
    }
}
