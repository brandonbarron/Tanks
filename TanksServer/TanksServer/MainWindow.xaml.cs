namespace TanksServer
{
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        ServerViewModel vm;
        public MainWindow()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
            vm = new ServerViewModel(MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance);
            this.DataContext = vm;
        }
    }
}
