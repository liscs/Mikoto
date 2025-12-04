using Mikoto.Translators.Interfaces;

namespace Mikoto.Translators.Implementations
{
    public class NoTranslator : ITranslator
    {
        public string DisplayName { get; private set; }

        public string GetLastError()
        {
            return string.Empty;
        }

        public Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            return Task.FromResult<string?>(string.Empty);
        }

        public NoTranslator(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
