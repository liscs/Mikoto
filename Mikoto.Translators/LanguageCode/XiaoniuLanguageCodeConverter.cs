using System.Globalization;

namespace Mikoto.Translators.LanguageCode
{
    public class XiaoniuLanguageCodeConverter : ILanguageCodeConverter
    {
        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            return cultureInfo.Name switch
            {
                "zh-Hant" => "cht",
                _ => cultureInfo.Name,
            };
        }
    }
}
