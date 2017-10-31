using log4net;
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
        private static readonly ILog _log = LogManager.GetLogger(typeof(ServerComManager));
        public delegate void SocketEvent(string socketEvent);
        public event SocketEvent SocketEventInfo;
        public ServerComManager()
        {
            log4net.Config.XmlConfigurator.Configure();
        }
        public void Start(int port, CancellationToken token) {
            TcpListener serverSocket = new TcpListener(System.Net.IPAddress.Any, port);
            SocketEventInfo("Listening");
            System.Net.Sockets.TcpClient clientSocket = default(System.Net.Sockets.TcpClient);
            int clientId = 0;

            serverSocket.Start();

            clientId = 0;

            while (!token.IsCancellationRequested) {
                clientId += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                _log.Debug($"New TCP Client connected");
                Thread thread = new Thread(() => StartServerMessenger(clientSocket, clientId, token));
                thread.Start();
            }
            clientSocket.Close();
            serverSocket.Stop();
            SocketEventInfo("Closed");
        }

        public void StartServerMessenger(System.Net.Sockets.TcpClient clientSocket, int clientId, CancellationToken token) {
            var client = new ServerMessenger(clientSocket, clientId, token);//TODO: this would be the game manager instead
        }
    }
}
