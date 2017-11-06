using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ClientTester
{
    public class ClientViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ClientViewModel));
        private readonly MahApps.Metro.Controls.Dialogs.IDialogCoordinator dialogCoordinator;
        private readonly GameCom.ClientMessenger _mainServerMessenger;
        private readonly GameCom.ClientMessenger _gameServerMessenger;
        private Thread gameServerTcpThread;
        private Thread mainServerTcpThread;
        public ClientViewModel(MahApps.Metro.Controls.Dialogs.IDialogCoordinator instance)
        {
            this.dialogCoordinator = instance;
            _startGameCommand = new DelegateCommand<object>((p) => StartGame());
            _stopGameCommand = new DelegateCommand<object>((p) => StopGame());
            _sendMoveCommand = new DelegateCommand<object>((p) => SendMove());
            _connectToMainServerCommand = new DelegateCommand<object>((p) => ConnectToMainServer());
            _disconnectMainServerCommand = new DelegateCommand<object>((p) => DisconnectMainServer());
            ServerStatus = "Dead";
            ServerIp = "127.0.0.1";
            ServerPort = 1500;
            GameServerIp = "127.0.0.1";
            GameServerPort = 1501;
            var localEp = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 1502);
            var myUdpClient = new System.Net.Sockets.UdpClient(localEp);
            myUdpClient.Client.ReceiveTimeout = 1000;
            _mainServerMessenger = new GameCom.ClientMessenger(myUdpClient);
            _mainServerMessenger.SocketEventInfo += _clientMessenger_SocketEventInfo;
            _gameServerMessenger = new GameCom.ClientMessenger(myUdpClient);
            _gameServerMessenger.SocketEventInfo += _clientMessenger_GameServerSocketEventInfo;
        }

        private readonly DelegateCommand<object> _startGameCommand;
        public DelegateCommand<object> StartGameCommand { get => _startGameCommand; }
        private readonly DelegateCommand<object> _stopGameCommand;
        public DelegateCommand<object> StopServerCommand { get => _stopGameCommand; }

        private readonly DelegateCommand<object> _connectToMainServerCommand;
        public DelegateCommand<object> ConnectToMainServerCommand { get => _connectToMainServerCommand; }
        private readonly DelegateCommand<object> _disconnectMainServerCommand;
        public DelegateCommand<object> DisconnectMainServerCommand { get => _disconnectMainServerCommand; }

        private readonly DelegateCommand<object> _sendMoveCommand;
        public DelegateCommand<object> SendMoveCommand { get => _sendMoveCommand; }

        private string _serverStatus;
        public string ServerStatus
        {
            get => _serverStatus;
            private set
            {
                _serverStatus = value;
                OnPropertyChanged();
            }
        }

        private string _gameServerStatus;
        public string GameServerStatus
        {
            get => _gameServerStatus;
            private set
            {
                _gameServerStatus = value;
                OnPropertyChanged();
            }
        }

        public string ServerIp { get; set; }
        public int ServerPort { get; set; }

        public string GameServerIp { get; set; }
        public int GameServerPort { get; set; }

        private void StartGame()
        {
            _log.Debug("Starting game");
            gameServerTcpThread = new Thread(() => this._gameServerMessenger.ConnectToGameServer(GameServerIp, GameServerPort));
            gameServerTcpThread.Start();
        }

        private void StopGame()
        {
            this._gameServerMessenger.StopGame();
            gameServerTcpThread.Abort();
        }

        private void ConnectToMainServer()
        {
            _log.Debug("Connecting To Main Server");
            mainServerTcpThread = new Thread(() => this._mainServerMessenger.ConnectToGameServer(ServerIp, ServerPort));
            mainServerTcpThread.Start();
        }

        private void DisconnectMainServer()
        {
            this._mainServerMessenger.StopGame();
            mainServerTcpThread.Abort();
        }

        private void SendMove()
        {
            _log.Debug("SendMove");
            this._gameServerMessenger.SendMove(new TanksCommon.SharedObjects.GameMove() { GameId = 0, GunType = 1, LocationX = 2, LocationY = 3 });
        }

        private void GetOpenGames()
        {
            _log.Debug("Asking for open games");
            this._mainServerMessenger.GetGames();
        }

        private void _clientMessenger_SocketEventInfo(string socketEvent)
        {
            Application.Current.Dispatcher.Invoke(() => ServerStatus = socketEvent, DispatcherPriority.Background);
        }

        private void _clientMessenger_GameServerSocketEventInfo(string socketEvent)
        {
            Application.Current.Dispatcher.Invoke(() => GameServerStatus = socketEvent, DispatcherPriority.Background);
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
