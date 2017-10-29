using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon
{
    class Client
    {
        public static void Sample(string[] args)
        {
            var messageTosend = (args.Length > 0) ? args[0] : "Nothing specific to say";
            var repeatCount = 1;
            if (args.Length > 1)
                int.TryParse(args[1], out repeatCount);

            // Create a TcpClient
            var client = new TcpClient();

            // Connect the client to the server -- remember that TCP is connection orient
            client.Connect("127.0.0.1", 15000);

            // Note that previous two statement can be combined using one of the
            // TcpClient constructs:
            //
            //  TcpClient client = new TcpClient("127.0.0.1", 15000);
            //
            // IMPORTANT: This constructor is NOT the same as one the takes an IPEndPoint
            // as a parameter.  That constructor binds the TcpClient to a local end point;
            // it does not connect the client to a remote end point.

            NetworkStream stream = client.GetStream();
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(messageTosend);
            for (var i = 0; i < repeatCount; i++)
                stream.Write(bytes, 0, bytes.Length);
        }
    }
}
