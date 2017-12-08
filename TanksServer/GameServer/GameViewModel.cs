using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace GameServer
{
    public class GameViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(GameViewModel));
        private readonly MahApps.Metro.Controls.Dialogs.IDialogCoordinator _dialogCoordinator;
        private readonly ComLogic.PlayerLogicForGameServer _playerLogicForGameServer;

        
        public GameViewModel(MahApps.Metro.Controls.Dialogs.IDialogCoordinator instance)
        {
            this._dialogCoordinator = instance;
            _startServerCommand = new DelegateCommand<object>((p) => StartServer());
            _stopServerCommand = new DelegateCommand<object>((p) => StopServer());
            _connectToServerCommand = new DelegateCommand<object>((p) => ConnectToMainServer());
            _disconnectServerCommand = new DelegateCommand<object>((p) => DisconnectFromMainServer());
            _sendMessageCommand = new DelegateCommand<object>((p) => SendMessageToClient());
            ServerStatus = "Dead";
            GamePort = 1501;
            ServerAddress = "127.0.0.1";
            ServerPort = 1500;

            _playerLogicForGameServer = new ComLogic.PlayerLogicForGameServer(GamePort);
            Log = new System.Collections.ObjectModel.ObservableCollection<TanksCommon.SharedObjects.LogEvent>();
            GameCom.ServerMessenger.ReceivedDataLog += ServerMessenger_ReceivedDataLog;
            _playerLogicForGameServer.SocketEventInfo += _playerLogicForGameServer_SocketEventInfo;
            _playerLogicForGameServer.SocketMainServerEventInfo += _playerLogicForGameServer_SocketMainServerEventInfo;
        }

        

        private readonly DelegateCommand<object> _startServerCommand;
        public DelegateCommand<object> StartServerCommand { get => _startServerCommand; }
        private readonly DelegateCommand<object> _stopServerCommand;
        public DelegateCommand<object> StopServerCommand { get => _stopServerCommand; }

        private readonly DelegateCommand<object> _connectToServerCommand;
        public DelegateCommand<object> ConnectToServerCommand { get => _connectToServerCommand; }
        private readonly DelegateCommand<object> _disconnectServerCommand;
        public DelegateCommand<object> DisconnectServerCommand { get => _disconnectServerCommand; }
        private readonly DelegateCommand<object> _sendMessageCommand;
        public DelegateCommand<object> SendMessageCommand { get => _sendMessageCommand; }
        public int GamePort { get; set; } //TODO:does this need to change?
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

        public System.Collections.ObjectModel.ObservableCollection<TanksCommon.SharedObjects.LogEvent> Log { get; private set; }

        private void StartServer()
        {
            _playerLogicForGameServer.SetListeningPort(GamePort);
            _playerLogicForGameServer.StartServer();
        }

        private void StopServer()
        {
            _playerLogicForGameServer.StopServer();
        }

        private void ConnectToMainServer()
        {
            _log.Debug("Connecting to Main Server");
            _playerLogicForGameServer.ConnectToMainServer(ServerAddress, ServerPort);
        }

        private void DisconnectFromMainServer()
        {
            _playerLogicForGameServer.DisconnectFromMainServer();
        }

        private void SendMessageToClient()
        {
            //TODO: Working on using the Game.cs logic to send a message to the client.
            //_theGame
            _log.Debug("Hey I can send a move!");
        }

        private void ServerMessenger_ReceivedDataLog(string logString)
        {
            Application.Current.Dispatcher.Invoke(() => Log.Add(new TanksCommon.SharedObjects.LogEvent(logString)), DispatcherPriority.Background);
        }

        private void _playerLogicForGameServer_SocketEventInfo(string socketEvent)
        {
            Application.Current.Dispatcher.Invoke(() => GameServerStatus = socketEvent, DispatcherPriority.Background);
        }

        private void _playerLogicForGameServer_SocketMainServerEventInfo(string socketEvent)
        {
            Application.Current.Dispatcher.Invoke(() => ServerStatus = socketEvent, DispatcherPriority.Background);
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
