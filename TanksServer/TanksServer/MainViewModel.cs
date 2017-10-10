namespace TanksServer
{
    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private readonly MahApps.Metro.Controls.Dialogs.IDialogCoordinator dialogCoordinator;

        public MainViewModel(MahApps.Metro.Controls.Dialogs.IDialogCoordinator instance)
        {
            this.dialogCoordinator = instance;
            _startServerCommand = new DelegateCommand<object>((p) => StartServer());
            _stopServerCommand = new DelegateCommand<object>((p) => StopServer());
            ServerStatus = "Dead";
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
        }

        private readonly DelegateCommand<object> _startServerCommand;
        public DelegateCommand<object> StartServerCommand { get => _startServerCommand; }
        private readonly DelegateCommand<object> _stopServerCommand;
        public DelegateCommand<object> StopServerCommand { get => _stopServerCommand; }

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


        private void StartServer()
        {

        }

        private void StopServer()
        {

        }


        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
