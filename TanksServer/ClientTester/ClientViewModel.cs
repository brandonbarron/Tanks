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
        private readonly ComLogic.PlayerLogicForMainServer _clientLogic;
        private readonly ComLogic.GameServerLogicForPlayer _gameServerLogic;
        public ClientViewModel(MahApps.Metro.Controls.Dialogs.IDialogCoordinator instance)
        {
            this.dialogCoordinator = instance;
            _connectToMainServerCommand = new DelegateCommand<object>((p) => ConnectToMainServer());
            _disconnectMainServerCommand = new DelegateCommand<object>((p) => DisconnectMainServer());
            _askOpenGameServerCommand = new DelegateCommand<object>((p) => GetOpenGameServers());

            _connectToGameServerCommand = new DelegateCommand<object>((p) => ConnectToGameServer());
            _disconnectFromGameServerCommand = new DelegateCommand<object>((p) => DisconnectFromGameServer());
            _askOpenGamesCommand = new DelegateCommand<object>((p) => AskOpenGames());
            _joinGameCommand = new DelegateCommand<object>((p) => JoinGame());
            _sendMoveCommand = new DelegateCommand<object>((p) => SendMove());
            _leaveGameCommand = new DelegateCommand<object>((p) => LeaveGame());

            GameServers = new System.Collections.ObjectModel.ObservableCollection<TanksCommon.SharedObjects.ListOfOpenGames>();
            Log = new System.Collections.ObjectModel.ObservableCollection<TanksCommon.SharedObjects.LogEvent>();
            ServerStatus = "Dead";
            ServerIp = "127.0.0.1";
            ServerPort = 1500;
            GameServerIp = "127.0.0.1";
            GameServerPort = 1501;
            var localEp = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 1502);
            var myUdpClient = new System.Net.Sockets.UdpClient(localEp);
            myUdpClient.Client.ReceiveTimeout = 1000;
            _clientLogic = new ComLogic.PlayerLogicForMainServer(myUdpClient);
            _gameServerLogic = new ComLogic.GameServerLogicForPlayer(myUdpClient);

            _clientLogic.RecievedOpenGamesEvent += _clientLogic_RecievedOpenGamesEvent;
            _clientLogic.SocketEventInfo += _clientLogic_SocketEventInfo;
            _clientLogic.ReceivedDataLog += _clientLogic_ReceivedDataLog;

            _gameServerLogic.SocketEventInfo += _gameServerLogic_SocketEventInfo;
            _gameServerLogic.ReceivedDataLog += _clientLogic_ReceivedDataLog;
            _gameServerLogic.RecievedOpenGamesEvent += _gameServerLogic_RecievedOpenGamesEvent;
        }

        

        //Main Server commands
        private readonly DelegateCommand<object> _connectToMainServerCommand;
        public DelegateCommand<object> ConnectToMainServerCommand { get => _connectToMainServerCommand; }
        private readonly DelegateCommand<object> _disconnectMainServerCommand;
        public DelegateCommand<object> DisconnectMainServerCommand { get => _disconnectMainServerCommand; }
        private readonly DelegateCommand<object> _askOpenGameServerCommand;
        public DelegateCommand<object> AskOpenGameServerCommand { get => _askOpenGameServerCommand; }
        
        //game server commands
        private readonly DelegateCommand<object> _connectToGameServerCommand;
        public DelegateCommand<object> ConnectToGameServerCommand { get => _connectToGameServerCommand; }
        private readonly DelegateCommand<object> _disconnectFromGameServerCommand;
        public DelegateCommand<object> DisconnectFromGameServerCommand { get => _disconnectFromGameServerCommand; }
        private readonly DelegateCommand<object> _askOpenGamesCommand;
        public DelegateCommand<object> AskOpenGamesCommand { get => _askOpenGamesCommand; }
        private readonly DelegateCommand<object> _joinGameCommand;
        public DelegateCommand<object> JoinGameCommand { get => _joinGameCommand; }
        private readonly DelegateCommand<object> _sendMoveCommand;
        public DelegateCommand<object> SendMoveCommand { get => _sendMoveCommand; }
        private readonly DelegateCommand<object> _leaveGameCommand;
        public DelegateCommand<object> LeaveGameCommand { get => _leaveGameCommand; }
        public System.Collections.ObjectModel.ObservableCollection<TanksCommon.SharedObjects.ListOfOpenGames> GameServers { get; private set; }
        public System.Collections.ObjectModel.ObservableCollection<TanksCommon.SharedObjects.LogEvent> Log { get; private set; }

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

        //Main Server Functions
        private void ConnectToMainServer()
        {
            _log.Debug("Connecting To Main Server");
            _clientLogic.SetServer(ServerIp, ServerPort);
            _clientLogic.ConnectToMainServer();
        }

        private void DisconnectMainServer()
        {
            _log.Debug("Dis-connecting From Main Server");
            _clientLogic.DisconnectMainServer();
        }
        private void GetOpenGameServers()
        {
            _log.Debug("Asking for open game servers");
            _clientLogic.GetOpenGameServers();
        }
        //Game Server Functions
        private void ConnectToGameServer()
        {
            _log.Debug("Connecting to Game Server");
            _gameServerLogic.SetServer(GameServerIp, GameServerPort);
            _gameServerLogic.ConnectToGameServer();
        }

        private void DisconnectFromGameServer()
        {
            _log.Debug("Dis-connecting from game server");
            _gameServerLogic.DisconnectFromGameServer();
        }
        private void AskOpenGames()
        {
            _log.Debug("Asking for open games");
            _gameServerLogic.AskOpenGames();
        }

        private void JoinGame()
        {
            //TODO:
        }

        private void SendMove()
        {
            _gameServerLogic.SendMove(new TanksCommon.SharedObjects.GameMove() { GunType = 3 });
        }

        private void LeaveGame()
        {
            //TODO:
        }

        private void _gameServerLogic_SocketEventInfo(string socketEvent)
        {
            Application.Current.Dispatcher.Invoke(() => GameServerStatus = socketEvent, DispatcherPriority.Background);
        }

        private void _clientLogic_SocketEventInfo(string socketEvent)
        {
            Application.Current.Dispatcher.Invoke(() => ServerStatus = socketEvent, DispatcherPriority.Background);
        }

        private void _clientLogic_RecievedOpenGamesEvent(TanksCommon.SharedObjects.ListOfOpenGames games)
        {
            Application.Current.Dispatcher.Invoke(() => GameServers.Add(games), DispatcherPriority.Background);
        }

        private void _gameServerLogic_RecievedOpenGamesEvent(TanksCommon.SharedObjects.ListOfOpenGames games)
        {
            Application.Current.Dispatcher.Invoke(() => GameServers.Add(games), DispatcherPriority.Background);
        }

        private void _clientLogic_ReceivedDataLog(string logString)
        {
            Application.Current.Dispatcher.Invoke(() => Log.Add(new TanksCommon.SharedObjects.LogEvent(logString)), DispatcherPriority.Background);
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
