namespace MisakaTranslator
{
    public class Logger
    {
        public static void WriteLine(string? str)
        {
            if (str != null)
            {
                LogViewer.LogWindow.LogEntries.Add(new LogEntry() { Message = str });
            }
        }
    }
}
