using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace MisakaTranslator
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<Border> GamePanelCollection { get; set; } = new();

        private System.Windows.Visibility _LEEnabled;
        public System.Windows.Visibility LEEnabled
        {
            get
            {
                return _LEEnabled;
            }

            set
            {
                SetProperty(ref _LEEnabled, value);
            }
        }
        private ICommand? openLog;
        public ICommand OpenLog
        {
            get
            {
                return openLog ??= new ActionCommand(LogViewer.LogWindow.Show);
            }
        }

    }

    public class ActionCommand : ICommand
    {
        private readonly Action _action;

        public ActionCommand(Action action)
        {
            _action = action;
        }

        public void Execute(object? parameter)
        {
            _action();
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public event EventHandler? CanExecuteChanged;
    }
}