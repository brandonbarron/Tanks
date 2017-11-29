using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    public class MessageResend : IMessage
    {
        public short Id { get => 900; }
        public int MessageId { get; set; }
    }
}
