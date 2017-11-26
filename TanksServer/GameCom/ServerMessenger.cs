using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameCom
{
    public class ServerMessenger : TheMessenger
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ServerMessenger));

        public ServerMessenger(TcpClient clientSocket, int clientId, CancellationToken token) : base(clientSocket, clientId)
        {
            //this._clientId = clientId;
            Thread thread = new Thread(() => GetStream(token));
            thread.Start();
        }
    }
}
