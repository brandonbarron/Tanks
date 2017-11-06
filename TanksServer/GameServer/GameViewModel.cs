using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace GameServer
{
    public class GameViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(GameViewModel));
        private readonly MahApps.Metro.Controls.Dialogs.IDialogCoordinator _dialogCoordinator;
        private readonly GameCom.ServerComManager _serverComManager;
        private readonly GameCom.ClientMessenger _clientMessenger;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Thread tcpThread;
        public GameViewModel(MahApps.Metro.Controls.Dialogs.IDialogCoordinator instance)
        {
            this._dialogCoordinator = instance;
            _startServerCommand = new DelegateCommand<object>((p) => StartServer());
            _stopServerCommand = new DelegateCommand<object>((p) => StopServer());
            _connectToServerCommand = new DelegateCommand<object>((p) => ConnectToMainServer());
            _disconnectServerCommand = new DelegateCommand<object>((p) => DisconnectFromMainServer());
            ServerStatus = "Dead";
            GamePort = 1501;
            ServerAddress = "127.0.0.1";
            ServerPort = 1500;
            var localEp = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 1501);
            var myUdpClient = new System.Net.Sockets.UdpClient(localEp);
            myUdpClient.Client.ReceiveTimeout = 1000;
            this._serverComManager = new GameCom.ServerComManager(myUdpClient);
            this._serverComManager.SocketEventInfo += _serverComManager_SocketEventInfo;
            
            Log = new System.Collections.ObjectModel.ObservableCollection<LogEvent>();
            GameCom.ServerMessenger.ReceivedDataLog += ServerMessenger_ReceivedDataLog;
            _cancellationTokenSource = new CancellationTokenSource();
            _clientMessenger = new GameCom.ClientMessenger(myUdpClient);
            _clientMessenger.SocketEventInfo += _clientMessenger_SocketEventInfo;
        }

        private readonly DelegateCommand<object> _startServerCommand;
        public DelegateCommand<object> StartServerCommand { get => _startServerCommand; }
        private readonly DelegateCommand<object> _stopServerCommand;
        public DelegateCommand<object> StopServerCommand { get => _stopServerCommand; }

        private readonly DelegateCommand<object> _connectToServerCommand;
        public DelegateCommand<object> ConnectToServerCommand { get => _connectToServerCommand; }
        private readonly DelegateCommand<object> _disconnectServerCommand;
        public DelegateCommand<object> DisconnectServerCommand { get => _disconnectServerCommand; }
        public int GamePort { get; set; }
        public string ServerAddress { get; set; }
        public int ServerPort { get; set; }

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

        public System.Collections.ObjectModel.ObservableCollection<LogEvent> Log { get; private set; }

        private void StartServer()
        {
            _log.Debug("starting game server");
            Thread t = new Thread(() => this._serverComManager.Start(GamePort, _cancellationTokenSource.Token));
            t.Start();
        }

        private void StopServer()
        {
            _cancellationTokenSource.Cancel();
        }

        private void ConnectToMainServer()
        {
            _log.Debug("Connecting to Main Server");
            var gameReg = new TanksCommon.SharedObjects.GameServerRegister()
            {
                OpenGames = new System.Collections.Generic.List<TanksCommon.SharedObjects.OpenGame>()
                {
                    new TanksCommon.SharedObjects.OpenGame()
                    {
                        GameId = 1, MapId = 1, NumberOfPlayers = 5, PlayerCapacity = 3
                    }
                }
            };
            tcpThread = new Thread(() => this._clientMessenger.ConnectToMainServerAndRegister(ServerAddress, ServerPort, gameReg));
            tcpThread.Start();
        }

        private void DisconnectFromMainServer()
        {
            this._clientMessenger.StopGame();
            tcpThread.Abort();
        }

        private void ServerMessenger_ReceivedDataLog(string logString)
        {
            Application.Current.Dispatcher.Invoke(() => Log.Add(new LogEvent(logString)), DispatcherPriority.Background);
        }

        private void _serverComManager_SocketEventInfo(string socketEvent)
        {
            Application.Current.Dispatcher.Invoke(() => ServerStatus = socketEvent, DispatcherPriority.Background);
        }

        private void _clientMessenger_SocketEventInfo(string socketEvent)
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
