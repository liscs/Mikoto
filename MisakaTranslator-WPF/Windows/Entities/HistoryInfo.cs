using System;

namespace MisakaTranslator
{
    internal class HistoryInfo
    {
        public string Message { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string TransolatorName { get; set; } = string.Empty;
        public override string ToString()
        {
            string result = $"======================================{Environment.NewLine}{DateTime} {TransolatorName}{Environment.NewLine}{Message}";
            return result;
        }
    }
}
