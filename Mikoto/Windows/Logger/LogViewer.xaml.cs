using Serilog.Events;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Mikoto.Windows.Logger
{
    public partial class LogViewer : Window
    {
        private static LogViewer? logWindow;

        // 绑定 ViewModel
        internal LogViewerViewModel ViewModel { get; } = new();

        public static LogViewer LogWindow
        {
            get
            {
                if (logWindow is null)
                {
                    logWindow = new LogViewer();
                    logWindow.LogRichTextBox.Document.Blocks.Clear();
                }
                return logWindow;
            }
        }

        private LogViewer()
        {
            InitializeComponent();
            // 设置 DataContext 以便 XAML 绑定 LogCountText
            this.DataContext = ViewModel;

            // 订阅 ViewModel 的集合变化，实现自动渲染到 RichTextBox
            ((System.Collections.Specialized.INotifyCollectionChanged)ViewModel.Logs).CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (LogEntry entry in e.NewItems)
                    {
                        AppendLogInternal(entry);
                    }
                }
            };
        }

        // 核心渲染逻辑：将数据转换为 UI 元素
        private void AppendLogInternal(LogEntry logEntry)
        {
            var blocks = LogRichTextBox.Document.Blocks;

            // 批量删除旧块逻辑
            if (blocks.Count >= 1000)
            {
                for (int i = 0; i < 100; i++)
                {
                    if (blocks.FirstBlock != null) blocks.Remove(blocks.FirstBlock);
                }
            }

            var paragraph = new Paragraph { Margin = new Thickness(0) };

            paragraph.Inlines.Add(new Run($"[{logEntry.Timestamp:HH:mm:ss}] ") { Foreground = Brushes.Gray });

            string levelText = $"[{logEntry.Level.ToString().Substring(0, 1).ToUpper()}] ";
            paragraph.Inlines.Add(new Run(levelText)
            {
                Foreground = GetBrush(logEntry.Level),
            });

            paragraph.Inlines.Add(new Run(logEntry.Message)
            {
                Foreground = GetBrush(logEntry.Level)
            });

            blocks.Add(paragraph);

            LogRichTextBox.ScrollToEnd();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        internal static void Sink(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage();
            var level = logEvent.Level;

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                LogWindow.ViewModel.AddLog(message, level);
            });
        }

        private static SolidColorBrush GetBrush(LogEventLevel level)
        {
            return level switch
            {
                LogEventLevel.Verbose => Brushes.Gray,
                LogEventLevel.Debug => Brushes.LightGray,
                LogEventLevel.Information => Brushes.LimeGreen,
                LogEventLevel.Warning => Brushes.Yellow,
                LogEventLevel.Error => Brushes.Red,
                LogEventLevel.Fatal => Brushes.DarkRed,
                _ => Brushes.AliceBlue
            };
        }
    }
}