using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon
{
    //based on the provided example from SimpleUDPSocket by Dr. Clyde
    public class UdpSender
    {
        //private static readonly ILog Logger = LogManager.GetLogger(typeof(Sender));

        private readonly UdpClient _myUdpClient;

        public List<IPEndPoint> Peers { get; set; }

        public UdpSender(UdpClient udpClient)
        {
            _myUdpClient = udpClient;
            Peers = new List<IPEndPoint>();
        }

        public void AddPeer(object setupParams)
        {
            //var peer = setupParams.ServerAddress + ":" + setupParams.ServerPort.ToString();
            var peer = "";
            if (!string.IsNullOrWhiteSpace(peer))
            {
                IPEndPoint peerAddress = Util.Parse(peer);
                if (peerAddress != null)
                {
                    Peers.Add(peerAddress);
                    //Logger.DebugFormat("Add {0} as a peer", peerAddress);
                }
            }
        }

        public void SendToPeers(byte[] bytes)
        {
            if (Peers != null && Peers.Count > 0)
            {
                foreach (IPEndPoint ep in Peers)
                {
                    int bytesSent = _myUdpClient.Send(bytes, bytes.Length, ep);
                    //Logger.InfoFormat("Send to {0} was {1}", ep, (bytesSent == bytes.Length) ? "Successful" : "Not Successful");
                }
            }
        }

    }
}
