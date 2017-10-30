using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    class JoinGameAccepted : IMessage
    {
        public int Id { get => 4; }
        public int GameId { get; set; }
        public int NewPlayerId { get; set; }
        public object GameSettings { get; set; }
    }
}
