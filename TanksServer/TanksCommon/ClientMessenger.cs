using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon
{
    public class ClientMessenger
    {
        bool _connectedToGame;
        bool _connectedToServer;
        bool _moveId;
        int _gameId;
        string _gameServerAddress;
        int _gameServerPort;
        public ClientMessenger()
        {

        }

        public bool ConnectToGameServer()
        {
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
    }
}
