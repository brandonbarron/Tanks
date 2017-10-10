namespace TanksServer
{
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        MainViewModel vm;
        public MainWindow()
        {
            InitializeComponent();
            vm = new MainViewModel(MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance);
            this.DataContext = vm;
        }
    }
}
