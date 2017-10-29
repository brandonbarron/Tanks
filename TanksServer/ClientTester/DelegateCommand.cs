namespace ClientTester
{
    //https://social.technet.microsoft.com/wiki/contents/articles/18199.event-handling-in-an-mvvm-wpf-application.aspx
    public class DelegateCommand<T> : System.Windows.Input.ICommand where T : class
    {
        private readonly System.Predicate<T> _canExecute;
        private readonly System.Action<T> _execute;

        public DelegateCommand(System.Action<T> execute)
            : this(execute, null)
        {
        }

        public DelegateCommand(System.Action<T> execute, System.Predicate<T> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            return _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event System.EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, System.EventArgs.Empty);
            var a = CanExecute(null);
        }
    }
}
