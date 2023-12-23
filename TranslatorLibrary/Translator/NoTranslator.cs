using System.Threading.Tasks;
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

        public void TranslatorInit(string? param1 = null, string? param2 = null)
        {
            return;
        }
    }
}
