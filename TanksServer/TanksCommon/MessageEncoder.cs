﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TanksCommon
{
    public class MessageEncoder
    {
        public static MemoryStream EncodeMessage<T>(MemoryStream memoryStream, T theMessage) where T : SharedObjects.IMessage
        {
            Write(memoryStream, theMessage.Id);
            XmlSerializer cereal = new XmlSerializer(typeof(T));
            cereal.Serialize(memoryStream, theMessage);
            
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        public static void Write(Stream memoryStream, short value)
        {
            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            memoryStream.Write(bytes, 0, bytes.Length);
        }
    }
}
