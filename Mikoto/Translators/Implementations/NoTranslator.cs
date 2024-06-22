using System.Windows;

namespace Mikoto.Translators
{
    public class NoTranslator : ITranslator
    {
        private NoTranslator() { }
        public string TranslatorDisplayName { get { return Application.Current.Resources["NoTranslator"].ToString()!; } }

        public string GetLastError()
        {
            return "";
        }

        public Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            return Task.FromResult<string?>(null);
        }

        public static ITranslator TranslatorInit(params string[] param)
        {
            return new NoTranslator();
        }
    }
}
