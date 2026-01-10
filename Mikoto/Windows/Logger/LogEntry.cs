using Serilog.Events;
namespace Mikoto.Windows.Logger
{
    public class LogEntry
    {
        public LogEntry() { }
        public LogEntry(int index, DateTime timestamp, string message, LogEventLevel level)
        {
            Index = index;
            Timestamp = timestamp;
            Message = message;
            Level = level;
        }
        public DateTime Timestamp { get; init; } = DateTime.Now;
        public int Index { get; init; }
        public string Message { get; init; } = string.Empty;
        public LogEventLevel Level { get; init; }
    }
}
