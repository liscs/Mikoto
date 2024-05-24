//来自 https://stackoverflow.com/questions/16743804/implementing-a-log-viewer-with-wpf

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace MisakaTranslator
{
    /// <summary>
    /// LogViewer.xaml 的交互逻辑
    /// </summary>
    public partial class LogViewer : Window
    {
        private static LogViewer? logWindow;

        public static LogViewer LogWindow
        {
            get
            {
                logWindow ??= new LogViewer();
                return logWindow;
            }
        }

        public ObservableCollection<LogEntry> LogEntries { get; set; }

        private LogViewer()
        {
            InitializeComponent();
            DataContext = LogEntries = new ObservableCollection<LogEntry>();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
