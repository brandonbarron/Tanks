using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    class InvalidMove : IMessage
    {
        public short Id { get => 2; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int MoveId { get; set; }
        public string ErrorType { get; set; }
    }
}
