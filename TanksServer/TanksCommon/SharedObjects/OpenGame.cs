using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    public class OpenGame
    {
        public short GameId { get; set; }
        public int MapId { get; set; }
        public int PlayerCapacity { get; set; }
        public int NumberOfPlayers { get; set; }
    }
}
