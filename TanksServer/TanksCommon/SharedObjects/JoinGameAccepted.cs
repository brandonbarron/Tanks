﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    [System.Xml.Serialization.XmlRoot(ElementName = "Message", Namespace = "http://Message.io")]
    public class JoinGameAccepted : IMessage
    {
        public short Id { get => 4; }
        public int MessageId { get; set; }
        public int GameId { get; set; }
        public int NewPlayerId { get; set; }
        public object GameSettings { get; set; }
    }
}
