using System.Globalization;

namespace MisakaTranslator
{
    public class TencentOldLanguageCodeConverter : ILanguageCodeConverter
    {
        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            return cultureInfo.TwoLetterISOLanguageName switch
            {
                "zh-Hant" => "zh-TW",
                _ => cultureInfo.TwoLetterISOLanguageName,
            };
        }
    }
}
