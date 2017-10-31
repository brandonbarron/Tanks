using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TanksCommon
{
    //based on the provided example from SimpleUDPSocket by Dr. Clyde
    public class UdpReceiver
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UdpReceiver));

        private readonly System.Net.Sockets.UdpClient _myUdpClient;
        public delegate void ReceivedDataDelegate(byte[] bytes);
        public event ReceivedDataDelegate ReceivedData;

        public UdpReceiver(UdpClient udpClient)
        {
            _myUdpClient = udpClient;
            _myUdpClient.Client.ReceiveTimeout = 1000;
        }

        public void ReceiveStuff(CancellationToken cancelationToken)
        {
            while (!cancelationToken.IsCancellationRequested)
            {
                IPEndPoint remoteEp = new IPEndPoint(IPAddress.Any, 53000);
                byte[] bytes = null;

                try
                {
                    bytes = _myUdpClient.Receive(ref remoteEp);
                }
                catch (SocketException err)
                {
                    if (err.SocketErrorCode != SocketError.TimedOut)
                        throw;
                }

                if (bytes != null)
                    ReceivedData(bytes);
            }
        }
    }
}
