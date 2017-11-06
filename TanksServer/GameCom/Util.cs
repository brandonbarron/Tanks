using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GameCom
{
    public static class Util
    {
        //private static readonly ILog Logger = LogManager.GetLogger(typeof(Util));
        //based on the provided example from SimpleUDPSocket by Dr. Clyde
        public static IPAddress ParseAddress(string hostname)
        {
            IPAddress result = null;
            IPAddress[] addressList = Dns.GetHostAddresses(hostname);
            for (int i = 0; i < addressList.Length && result == null; i++)
                if (addressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    result = addressList[i];
            return result;
        }
        public static IPEndPoint Parse(string hostnameAndPort)
        {
            IPEndPoint result = null;
            if (!string.IsNullOrWhiteSpace(hostnameAndPort))
            {
                string[] tmp = hostnameAndPort.Split(':');
                if (tmp.Length == 2 && !string.IsNullOrWhiteSpace(tmp[0]) && !string.IsNullOrWhiteSpace(tmp[1]))
                    result = new IPEndPoint(ParseAddress(tmp[0]), ParsePort(tmp[1]));
            }
            return result;
        }

        public static int ParsePort(string portStr)
        {
            int port = 0;
            if (!string.IsNullOrWhiteSpace(portStr))
                Int32.TryParse(portStr, out port);
            return port;
        }
    }
}
