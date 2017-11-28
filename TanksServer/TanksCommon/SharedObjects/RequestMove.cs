using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    [System.Xml.Serialization.XmlRoot(ElementName = "Message", Namespace = "http://Message.io")]
    public class RequestMove : IMessage
    {
        public short Id { get => 6; }
        public int MessageId { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int MoveId { get; set; }
    }
}
