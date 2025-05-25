using Mikoto.Translators.Interfaces;
using System.Windows;

namespace Mikoto.Translators.Implementations
{
    public class NoTranslator : ITranslator
    {
        private NoTranslator() { }
        public string TranslatorDisplayName { get { return Application.Current.Resources["NoTranslator"].ToString()!; } }

        public string GetLastError()
        {
            return string.Empty;
        }

        public Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            return Task.FromResult<string?>(string.Empty);
        }

        public static ITranslator TranslatorInit(params string[] param)
        {
            return new NoTranslator();
        }
    }
}
