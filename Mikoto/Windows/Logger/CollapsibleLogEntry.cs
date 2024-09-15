namespace Mikoto.Windows.Logger
{
    internal class CollapsibleLogEntry : LogEntry
    {
        internal List<LogEntry> Contents { get; set; } = new();
    }
}
