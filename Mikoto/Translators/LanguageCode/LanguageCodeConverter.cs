using System.Globalization;

namespace Mikoto
{
    public interface ILanguageCodeConverter
    {
        public static abstract string GetLanguageCode(CultureInfo cultureInfo);
    }
}