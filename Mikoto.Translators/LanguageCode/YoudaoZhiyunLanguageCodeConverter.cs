using System.Globalization;

namespace Mikoto.Translators.LanguageCode
{
    public class YoudaoZhiyunLanguageCodeConverter : ILanguageCodeConverter
    {
        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            return cultureInfo.Name switch
            {
                "zh" => "zh-CHS",
                _ => cultureInfo.Name,
            };
        }
    }
}
