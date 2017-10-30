using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    class RequestMove : IMessage
    {
        public int Id { get => 6; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int MoveId { get; set; }
    }
}
