using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTester
{
    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private readonly MahApps.Metro.Controls.Dialogs.IDialogCoordinator dialogCoordinator;

        public MainViewModel(MahApps.Metro.Controls.Dialogs.IDialogCoordinator instance)
        {
            this.dialogCoordinator = instance;
            _startGameCommand = new DelegateCommand<object>((p) => StartGame());
            _stopGameCommand = new DelegateCommand<object>((p) => StopGame());
            ServerStatus = "Dead";
           
        }

        private readonly DelegateCommand<object> _startGameCommand;
        public DelegateCommand<object> StartGameCommand { get => _startGameCommand; }
        private readonly DelegateCommand<object> _stopGameCommand;
        public DelegateCommand<object> StopServerCommand { get => _stopGameCommand; }

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


        private void StartGame()
        {

        }

        private void StopGame()
        {

        }


        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
