using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TanksCommon
{
    public class MessageDecoder
    {
        public static short DecodeMessageType(Stream memoryStream)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            return ReadShort(memoryStream);
        }

        public static T DecodeMessage<T>(Stream stream)
        {
            stream.Seek(2, SeekOrigin.Begin);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            T message = ((T)xmlSerializer.Deserialize(stream));
            return message;
        }

        public static short ReadShort(Stream memoryStream)
        {
            byte[] bytes = new byte[2];
            int bytesRead = memoryStream.Read(bytes, 0, bytes.Length);
            if (bytesRead != bytes.Length)
                throw new ApplicationException("Cannot decode a short integer from message");

            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 0));
        }

        public static Type GetTypeFromStream(MemoryStream stream)
        {
            short messageType = DecodeMessageType(stream);
            return GetType(messageType);
        }
        public static Type GetType(short messageType)
        {
            switch (messageType)
            {
                case 0:
                    return typeof(TanksCommon.SharedObjects.Ping);
                case 1:
                    return typeof(TanksCommon.SharedObjects.GameStatus);
                case 2:
                    return typeof(TanksCommon.SharedObjects.InvalidMove);
                case 3:
                    return typeof(TanksCommon.SharedObjects.JoinGame);
                case 4:
                    return typeof(TanksCommon.SharedObjects.JoinGameAccepted);
                case 5:
                    return typeof(TanksCommon.SharedObjects.MoveAccepted);
                case 6:
                    return typeof(TanksCommon.SharedObjects.RequestMove);
                case 7:
                    return typeof(TanksCommon.SharedObjects.GameMove);
                case 8:
                    return typeof(TanksCommon.SharedObjects.ListOfOpenGames);
                case 9:
                    return typeof(TanksCommon.SharedObjects.RequestGames);
                case 99:
                    return typeof(TanksCommon.SharedObjects.DataReceived);
            }
            return typeof(SharedObjects.IMessage);
        }

        //public static T ConvertType<T>(SharedObjects.IMessage message) where T : SharedObjects.IMessage
        //{
        //    switch (message.Id)
        //    {
        //        case 0:
        //            return message as SharedObjects.Ping;
        //        case 1:
        //            return typeof(TanksCommon.SharedObjects.GameStatus);
        //        case 2:
        //            return typeof(TanksCommon.SharedObjects.InvalidMove);
        //        case 3:
        //            return typeof(TanksCommon.SharedObjects.JoinGame);
        //        case 4:
        //            return typeof(TanksCommon.SharedObjects.JoinGameAccepted);
        //        case 5:
        //            return typeof(TanksCommon.SharedObjects.MoveAccepted);
        //        case 6:
        //            return typeof(TanksCommon.SharedObjects.RequestMove);
        //        case 7:
        //            return typeof(TanksCommon.SharedObjects.GameMove);
        //        case 8:
        //            return typeof(TanksCommon.SharedObjects.ListOfOpenGames);
        //        case 9:
        //            return typeof(TanksCommon.SharedObjects.RequestGames);
        //        case 99:
        //            return typeof(TanksCommon.SharedObjects.DataReceived);
        //    }
        //    return typeof(SharedObjects.IMessage);
        //}
    }
}
