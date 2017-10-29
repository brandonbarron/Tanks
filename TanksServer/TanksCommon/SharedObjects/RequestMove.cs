using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    class RequestMove
    {
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int MoveId { get; set; }
    }
}
