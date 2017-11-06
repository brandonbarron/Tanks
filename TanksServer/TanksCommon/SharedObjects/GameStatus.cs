using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    public class GameStatus : IMessage
    {
        public short Id { get => 1; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int Status { get; set; }
        public int PlayerTurn { get; set; }
    }
}
