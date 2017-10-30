using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TanksCommon
{
    /// <summary>
    /// This class will be the abstraction to handle multiple game communications
    /// </summary>
    public class ServerComManager
    {
        public static void Start() {
            TcpListener serverSocket = new TcpListener(System.Net.IPAddress.Any, 1500);
            System.Net.Sockets.TcpClient clientSocket = default(System.Net.Sockets.TcpClient);
            int clientId = 0;

            serverSocket.Start();

            clientId = 0;

            while (true) {
                clientId += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                Thread thread = new Thread(() => StartServerMessenger(clientSocket, clientId));
                thread.Start();
            }
            //TODO: provide cancellation token to close
            clientSocket.Close();
            serverSocket.Stop();
        }

        public static void StartServerMessenger(System.Net.Sockets.TcpClient clientSocket, int clientId) {
            var client = new ServerMessenger(clientSocket, clientId);//TODO: this would be the game manager instead
        }
    }
}
