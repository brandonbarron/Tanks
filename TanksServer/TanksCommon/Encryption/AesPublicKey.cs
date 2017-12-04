using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.Encryption
{
    [Serializable]
    [System.Xml.Serialization.XmlRoot(ElementName = "Message", Namespace = "http://Message.io")]
    public class AesPublicKey : SharedObjects.IMessage
    {
        public short Id { get => 501; }
        public int MessageId { get; set; }
        public byte[] Iv { get; set; }
        public byte[] SessionKey { get; set; }
    }
}
