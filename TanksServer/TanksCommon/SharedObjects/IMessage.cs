using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DataReceived))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(GameMove))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(GameServerRegister))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(GameStatus))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InvalidMove))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(JoinGame))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(JoinGameAccepted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ListOfOpenGames))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MoveAccepted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(OpenGame))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Ping))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestGames))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestMove))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MessageResend))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Encryption.RsaPublicKey))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Encryption.AesPublicKey))]
    [System.Xml.Serialization.XmlRoot(ElementName = "Message", Namespace = "http://Message.io")]
    public interface IMessage
    {
        short Id { get; }
        int MessageId { get; set; }
    }
}
