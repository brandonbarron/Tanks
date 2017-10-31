using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksServer
{
    public struct LogEvent
    {
        public LogEvent(string logEvent)
        {
            TheEvent = logEvent;
        }
        public DateTime TheTime { get => DateTime.Now; }
        public string TheEvent { get; set; }
    }
}
