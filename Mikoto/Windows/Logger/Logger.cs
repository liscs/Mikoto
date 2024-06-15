namespace Mikoto
{
    public class Logger
    {
        public static void Info(string? str)
        {
            if (str != null)
            {
                LogViewer.LogWindow.LogEntries.Add(new LogEntry() { Message = str });
            }
        }
    }
}
