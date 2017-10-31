using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    public class JoinGame : IMessage
    {
        public short Id { get => 3; }
        public int GameId { get; set; }
        public string PlayerName { get; set; }
    }
}
