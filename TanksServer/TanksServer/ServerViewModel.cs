using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace TanksServer
{
    public class ServerViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ServerViewModel));
        private readonly MahApps.Metro.Controls.Dialogs.IDialogCoordinator _dialogCoordinator;
        private readonly TanksCommon.ServerComManager _serverComManager;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ServerViewModel(MahApps.Metro.Controls.Dialogs.IDialogCoordinator instance)
        {
            this._dialogCoordinator = instance;
            _startServerCommand = new DelegateCommand<object>((p) => StartServer());
            _stopServerCommand = new DelegateCommand<object>((p) => StopServer());
            ServerStatus = "Dead";
            ServerPort = 1500;
            this._serverComManager = new TanksCommon.ServerComManager();
            this._serverComManager.SocketEventInfo += _serverComManager_SocketEventInfo;
            CurrentGames = new System.Collections.ObjectModel.ObservableCollection<object>()
            {
                new
                {
                    Id = 1,
                    Players = 3,
                    TotalTime = "1:03",
                    CurrentWinner = "Player1"
                },
                new
                {
                    Id = 2,
                    Players = 2,
                    TotalTime = "4:13",
                    CurrentWinner = "Player1"
                },
                new
                {
                    Id = 3,
                    Players = 17,
                    TotalTime = "30:03",
                    CurrentWinner = "Player1"
                }
            };
            Log = new System.Collections.ObjectModel.ObservableCollection<LogEvent>();
            TanksCommon.ServerMessenger.ReceivedDataLog += ServerMessenger_ReceivedDataLog;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private readonly DelegateCommand<object> _startServerCommand;
        public DelegateCommand<object> StartServerCommand { get => _startServerCommand; }
        private readonly DelegateCommand<object> _stopServerCommand;
        public DelegateCommand<object> StopServerCommand { get => _stopServerCommand; }

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

        public System.Collections.ObjectModel.ObservableCollection<object> CurrentGames { get; private set; }
        public System.Collections.ObjectModel.ObservableCollection<LogEvent> Log { get; private set; }

        private void StartServer()
        {
            _log.Debug("starting server");
            Thread t = new Thread(() => this._serverComManager.Start(ServerPort, _cancellationTokenSource.Token));
            t.Start();
        }

        private void StopServer()
        {
            _cancellationTokenSource.Cancel();
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
