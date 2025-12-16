//来自 https://stackoverflow.com/questions/16743804/implementing-a-log-viewer-with-wpf

using Mikoto.Windows.Logger;
using Serilog.Events;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Mikoto
{
    /// <summary>
    /// LogViewer.xaml 的交互逻辑
    /// </summary>
    public partial class LogViewer
    {
        private static LogViewer? logWindow;

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
        internal void AppendLog(LogEntry logEntry)
        {
            var paragraph = new Paragraph { Margin = new Thickness(0) };

            // 时间
            paragraph.Inlines.Add(new Run($"[{logEntry.DateTime:HH:mm:ss}] ") { Foreground = Brushes.Gray });

            // 索引
            paragraph.Inlines.Add(new Run($"# {logEntry.Index}: ") { Foreground = Brushes.LightGray });

            // 日志消息
            paragraph.Inlines.Add(new Run(logEntry.Message) { Foreground = logEntry.Color });

            LogRichTextBox.Document.Blocks.Add(paragraph);

            // 自动滚动到底部
            LogRichTextBox.ScrollToEnd();

            // 更新日志数量显示
            LogCountTextBlock.Text = $"{LogRichTextBox.Document.Blocks.Count} Items";
        }

        private LogViewer()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }


        internal static void Sink(LogEvent logEvent)
        {
            var brush = GetBrush(logEvent.Level);
            var message = logEvent.RenderMessage();

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                var entry = new LogEntry
                {
                    Message = message,
                    Color = brush
                };
                LogWindow.AppendLog(entry);
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
                _ => Brushes.Black
            };
        }
    }
}
