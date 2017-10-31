using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    class MoveAccepted : IMessage
    {
        public short Id { get => 5; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int MoveId { get; set; }
    }
}
