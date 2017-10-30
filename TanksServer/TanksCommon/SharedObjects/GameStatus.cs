using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    class GameStatus : IMessage
    {
        public int Id { get => 1; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int Status { get; set; }
        public int PlayerTurn { get; set; }
    }
}
