using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCom
{
    public static class NetworkStreamExtensions
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(NetworkStreamExtensions));

        public static bool WriteStreamMessage(this System.Net.Sockets.NetworkStream stream, System.IO.MemoryStream message)
        {
            _log.DebugFormat("In WriteStreamMessage, message={0}", (message == null) ? "null" : message.GetType().Name);
            message.Seek(0, System.IO.SeekOrigin.Begin);
            byte[] messageBytes = message.ToArray();
            return stream.WriteStreamMessage(messageBytes);
        }

        public static bool WriteStreamMessage(this System.Net.Sockets.NetworkStream stream, byte[] messageBytes)
        {
            bool result = false;
            _log.DebugFormat("In WriteStreamMessage, message={0}", (messageBytes == null) ? "null" : messageBytes.GetType().Name);
            if (stream != null && messageBytes != null)
            {
                
                byte[] lengthBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(messageBytes.Length));
                if (stream.CanWrite)
                {
                    try
                    {
                        stream.Write(lengthBytes, 0, lengthBytes.Length);
                        stream.Write(messageBytes, 0, messageBytes.Length);
                        result = true;
                        _log.Debug("Write complete");
                    }
                    catch (Exception err)
                    {
                        _log.Error(err.Message);
                    }
                }
                else
                    _log.Warn("Stream is not writable");
            }
            return result;
        }

        public static byte[] ReadStreamMessage(this System.Net.Sockets.NetworkStream stream)
        {
            //_log.DebugFormat("In ReadStreamMessage, with stream.ReadTimeout={0}", (stream == null) ? "null" : stream.ReadTimeout.ToString());

            byte[] result = null;

            int bytesRead = 4;
            byte[] bytes = new byte[4];
            bytes = ReadBytes(stream, bytesRead);

            _log.DebugFormat("Length bytes read = {0}", bytes.Length);

            if (bytesRead == bytes.Length)
            {
                int messageLength = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 0));
                _log.DebugFormat("Incoming message will be {0} bytes", messageLength);

                bytes = ReadBytes(stream, messageLength);
                _log.DebugFormat("Message bytes read = {0}", bytes.Length);

                if (messageLength == bytes.Length)
                    result = bytes;
            }
            return result;
        }

        private static byte[] ReadBytes(System.Net.Sockets.NetworkStream stream, int bytesToRead)
        {
            byte[] bytes = new byte[bytesToRead];
            int bytesRead = 0;

            _log.DebugFormat("Try to read {0} length bytes, with stream.CanRead={1} and stream.ReadTimeout={2}", bytesToRead, stream.CanRead, stream.ReadTimeout);

            int remainingTime = stream.ReadTimeout;
            while (stream.CanRead && bytesRead < bytesToRead && remainingTime > 0)
            {
                DateTime ts = DateTime.Now;
                try
                {
                    bytesRead += stream.Read(bytes, bytesRead, bytes.Length - bytesRead);
                }
                catch (System.IO.IOException) { }
                catch (Exception err)
                {
                    _log.Warn(err.GetType());
                    _log.Warn(err.Message);
                }
                remainingTime -= Convert.ToInt32(DateTime.Now.Subtract(ts).TotalMilliseconds);
            }

            if (bytesToRead != bytesRead)
            {
                if (bytesRead > 0)
                    throw new ApplicationException(string.Format("Expected {0} bytes of messsage data, but only {1} of them arrived within {2} ms", bytesToRead, bytesRead, stream.ReadTimeout));
                bytes = new byte[0];
            }
            return bytes;
        }
    }
}
