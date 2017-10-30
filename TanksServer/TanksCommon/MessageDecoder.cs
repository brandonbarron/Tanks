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
    class MessageDecoder
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
    }
}
