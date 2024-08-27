using System.Globalization;

namespace Mikoto.Translators.LanguageCode
{
    public interface ILanguageCodeConverter
    {
        public static abstract string GetLanguageCode(CultureInfo cultureInfo);
    }
}