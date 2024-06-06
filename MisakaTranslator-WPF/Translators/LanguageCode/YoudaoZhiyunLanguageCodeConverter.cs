using System.Globalization;

namespace MisakaTranslator
{
    public class YoudaoZhiyunLanguageCodeConverter : ILanguageCodeConverter
    {
        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            return cultureInfo.TwoLetterISOLanguageName switch
            {
                "zh" => "zh-CHS",
                _ => cultureInfo.TwoLetterISOLanguageName,
            };
        }
    }
}
