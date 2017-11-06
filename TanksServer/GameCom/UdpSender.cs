using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameCom
{
    //based on the provided example from SimpleUDPSocket by Dr. Clyde
    public class UdpSender
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(UdpSender));

        private readonly UdpClient _myUdpClient;

        public List<IPEndPoint> Peers { get; set; }

        public UdpSender(UdpClient udpClient)
        {
            _myUdpClient = udpClient;
            Peers = new List<IPEndPoint>();
        }

        public void AddPeer(string address, int port)
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                IPEndPoint peerAddress = Util.Parse(address + ':' + port.ToString());
                if (peerAddress != null)
                {
                    Peers.Add(peerAddress);
                    _log.DebugFormat("Add {0} as a peer", peerAddress);
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
                    _log.InfoFormat("Send to {0} was {1}", ep, (bytesSent == bytes.Length) ? "Successful" : "Not Successful");
                }
            }
        }

        public void SendObjectToPeers<T>(T theObject, [System.Runtime.CompilerServices.CallerMemberName] string sendingFrom = "") where T : TanksCommon.SharedObjects.IMessage
        {
            using (var stream = new System.IO.MemoryStream())
            {
                _log.Debug($"Sending udp object from: {sendingFrom}");
                var messageStream = TanksCommon.MessageEncoder.EncodeMessage(stream, theObject);
                messageStream.Seek(0, System.IO.SeekOrigin.Begin);
                this.SendToPeers(messageStream.ToArray());
            }
        }
    }
}
