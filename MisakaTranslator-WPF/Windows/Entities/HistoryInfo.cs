namespace MisakaTranslator
{
    internal class HistoryInfo
    {
        public string Message { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string TranslatorName { get; set; } = string.Empty;
        public override string ToString()
        {
            string result = $"======================================{Environment.NewLine}{DateTime} {TranslatorName}{Environment.NewLine}{Message}";
            return result;
        }
    }
}
