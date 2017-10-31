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
        private readonly TanksCommon.ClientMessenger _clientMessenger;
        private Thread tcpThread;
        public ClientViewModel(MahApps.Metro.Controls.Dialogs.IDialogCoordinator instance)
        {
            this.dialogCoordinator = instance;
            _startGameCommand = new DelegateCommand<object>((p) => StartGame());
            _stopGameCommand = new DelegateCommand<object>((p) => StopGame());
            _sendMoveCommand = new DelegateCommand<object>((p) => SendMove());
            ServerStatus = "Dead";
            ServerIp = "127.0.0.1";
            ServerPort = 1500;
            _clientMessenger = new TanksCommon.ClientMessenger();
            _clientMessenger.SocketEventInfo += _clientMessenger_SocketEventInfo;
        }

        private readonly DelegateCommand<object> _startGameCommand;
        public DelegateCommand<object> StartGameCommand { get => _startGameCommand; }
        private readonly DelegateCommand<object> _stopGameCommand;
        public DelegateCommand<object> StopServerCommand { get => _stopGameCommand; }

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

        public string ServerIp { get; set; }
        public int ServerPort { get; set; }


        private void StartGame()
        {
            _log.Debug("Starting game");
            tcpThread = new Thread(() => this._clientMessenger.ConnectToGameServer(ServerIp, ServerPort));
            tcpThread.Start();
        }

        private void StopGame()
        {
            this._clientMessenger.StopGame();
            tcpThread.Abort();
        }

        private void SendMove()
        {
            _log.Debug("SendMove");
            this._clientMessenger.SendMove(new TanksCommon.SharedObjects.GameMove() { GameId = 0, GunType = 1, LocationX = 2, LocationY = 3 });
        }

        private void _clientMessenger_SocketEventInfo(string socketEvent)
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
