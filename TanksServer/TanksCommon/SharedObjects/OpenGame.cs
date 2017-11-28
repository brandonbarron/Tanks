using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    [System.Xml.Serialization.XmlRoot(ElementName = "Message", Namespace = "http://Message.io")]
    public class OpenGame : IMessage
    {
        public short Id { get => 10; }
        public int MessageId { get; set; }
        public short GameId { get; set; }
        public int MapId { get; set; }
        public int PlayerCapacity { get; set; }
        public int NumberOfPlayers { get; set; }
    }
}
