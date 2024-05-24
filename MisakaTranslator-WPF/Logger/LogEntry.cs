namespace MisakaTranslator
{
    public class LogEntry : ViewModelBase
    {
        public DateTime DateTime { get; set; } = DateTime.Now;

        public int Index { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
