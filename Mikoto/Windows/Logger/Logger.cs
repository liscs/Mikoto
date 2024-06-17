namespace Mikoto
{
    public class Logger
    {
        public static void Info(object? str)
        {
            if (str?.ToString() != null)
            {
                LogViewer.LogWindow.LogEntries.Add(new LogEntry() { Message = str.ToString()! });
            }
        }
    }
}
