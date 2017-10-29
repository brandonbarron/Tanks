using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon
{
    class Receiver
    {
        public static void Sample()
        {
            IPEndPoint myEp = new IPEndPoint(IPAddress.Any, 15000);
            TcpListener server = new TcpListener(myEp);
            server.Start();

            bool keepGoing = true;
            while (keepGoing)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                keepGoing = ReceiveDataFromClient(stream);
            }

        }

        private static bool ReceiveDataFromClient(NetworkStream stream)
        {
            var buffer = new byte[256];

            var keepGoing = true;
            var stayConnected = true;

            while (stayConnected)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string data = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Received {0} bytes: {1}", bytesRead, data);

                        if (data == "$" || data == "*")
                            stayConnected = false;

                        if (data == "$")
                            keepGoing = false;
                    }
                }
                catch
                {
                    stayConnected = false;
                }
            }
            return keepGoing;
        }
    }
}
