using System.Globalization;

namespace MisakaTranslator
{
    public interface ILanguageCodeConverter
    {
        public static abstract string GetLanguageCode(CultureInfo cultureInfo);
    }
}