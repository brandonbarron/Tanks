using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    [System.Xml.Serialization.XmlRoot(ElementName = "Message", Namespace = "http://Message.io")]
    public class Ping : IMessage
    {
        public short Id { get => 0; }
        public int MessageId { get; set; }
        public int PlayerId { get; set; }
    }
}
