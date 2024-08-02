using System.Windows;
using System.Windows.Media;

namespace Mikoto
{
    public class Logger
    {
        public static void Info(object? str)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (str?.ToString() != null)
                {
                    LogViewer.LogWindow.LogEntries.Add(new LogEntry()
                    {
                        Message = str.ToString()!,
                        Color = Brushes.Green,
                    });
                }
            });
        }

        public static void Warn(object? str)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (str?.ToString() != null)
                {
                    LogViewer.LogWindow.LogEntries.Add(new LogEntry()
                    {
                        Message = str.ToString()!,
                        Color = Brushes.Yellow,
                    });
                }
            });
        }

        public static void Error(object? str)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (str?.ToString() != null)
                {
                    LogViewer.LogWindow.LogEntries.Add(new LogEntry()
                    {
                        Message = str.ToString()!,
                        Color = Brushes.Red,
                    });
                }
            });
        }
    }
}
