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
        public short RetryMax { get; set; }

        public MessageInfo(T theMessenge)  {
            this.OriginalMessage = theMessenge;
            this.OriginalSendTime = DateTime.Now;
            this.RetryCount = 0;
            this.RetryMax = 3;
        }

        public bool RertyExceeded()
        {
            return RetryCount > RetryMax;
        }

        public MessageInfo()
        {
            this.OriginalSendTime = DateTime.Now;
            this.RetryCount = 0;
            this.RetryMax = 3;
        }
    }
}
