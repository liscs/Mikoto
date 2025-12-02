using System.Globalization;

namespace Mikoto.Translators.LanguageCode
{
    public interface ILanguageCodeConverter
    {
        static abstract string GetLanguageCode(CultureInfo cultureInfo);
    }
}