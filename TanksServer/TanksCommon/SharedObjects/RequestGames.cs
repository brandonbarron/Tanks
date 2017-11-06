using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    public class RequestGames : IMessage
    {
        public short Id { get => 9; }

    }
}
