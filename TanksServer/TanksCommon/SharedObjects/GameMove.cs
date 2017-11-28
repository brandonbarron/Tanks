using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    [System.Xml.Serialization.XmlRoot(ElementName = "Message", Namespace = "http://Message.io")]
    public class GameMove : IMessage
    {
        public short Id { get => 7; }
        public int MessageId { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int MoveId { get; set; }
        public int LocationY { get; set; }
        public int LocationX { get; set; }
        public int GunType { get; set; }
        public int ShotAngle { get; set; }
        public int ShotPower { get; set; }
    }
}
