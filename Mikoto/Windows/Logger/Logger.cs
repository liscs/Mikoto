using System.Windows.Media;

namespace Mikoto
{
    public class Logger
    {
        public static void Info(object? str)
        {
            if (str?.ToString() != null)
            {
                LogViewer.LogWindow.LogEntries.Add(new LogEntry()
                {
                    Message = str.ToString()!,
                    Color = Brushes.Green,
                });
            }
        }

        public static void Warn(object? str)
        {
            if (str?.ToString() != null)
            {
                LogViewer.LogWindow.LogEntries.Add(new LogEntry()
                {
                    Message = str.ToString()!,
                    Color = Brushes.Yellow,
                });
            }
        }

        public static void Error(object? str)
        {
            if (str?.ToString() != null)
            {
                LogViewer.LogWindow.LogEntries.Add(new LogEntry()
                {
                    Message = str.ToString()!,
                    Color = Brushes.Red,
                });
            }
        }
    }
}
