namespace GameServer
{
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        GameViewModel vm;
        public MainWindow()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
            vm = new GameViewModel(MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance);
            this.DataContext = vm;
        }
    }
}
