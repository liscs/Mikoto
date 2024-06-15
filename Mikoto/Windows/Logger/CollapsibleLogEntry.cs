namespace Mikoto
{
    public class CollapsibleLogEntry : LogEntry
    {
        public List<LogEntry> Contents { get; set; } = new();
    }
}
