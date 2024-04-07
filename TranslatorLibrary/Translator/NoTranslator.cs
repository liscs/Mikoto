using TranslatorLibrary.lang;

namespace TranslatorLibrary.Translator
{
    public class NoTranslator : ITranslator
    {
        public string TranslatorDisplayName { get { return Strings.NoTranslator; } }

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
