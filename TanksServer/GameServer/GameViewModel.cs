using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace GameServer
{
    public class GameViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(GameViewModel));
        private readonly MahApps.Metro.Controls.Dialogs.IDialogCoordinator _dialogCoordinator;
        private readonly TanksCommon.ServerComManager _serverComManager;
        private readonly CancellationTokenSource _cancellationTokenSource;

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
            this._serverComManager = new TanksCommon.ServerComManager();
            this._serverComManager.SocketEventInfo += _serverComManager_SocketEventInfo;
            
            Log = new System.Collections.ObjectModel.ObservableCollection<LogEvent>();
            TanksCommon.ServerMessenger.ReceivedDataLog += ServerMessenger_ReceivedDataLog;
            _cancellationTokenSource = new CancellationTokenSource();
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

        }

        private void DisconnectFromMainServer()
        {

        }

        private void ServerMessenger_ReceivedDataLog(string logString)
        {
            Application.Current.Dispatcher.Invoke(() => Log.Add(new LogEvent(logString)), DispatcherPriority.Background);
        }

        private void _serverComManager_SocketEventInfo(string socketEvent)
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
