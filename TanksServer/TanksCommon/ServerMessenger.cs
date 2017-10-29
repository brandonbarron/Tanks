using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon
{
    public class ServerMessenger
    {
        public ServerMessenger()
        {

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
    }
}
