using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCom
{
    public class MessageInfo<T> where T : TanksCommon.SharedObjects.IMessage
    {
        public DateTime OriginalSendTime { get; set; }
        public T OriginalMessage { get; }
        public short RetryCount { get; set; }

        public MessageInfo(T theMessenge)  {
            this.OriginalMessage = theMessenge;
            this.OriginalSendTime = DateTime.Now;
        }
    }
}
