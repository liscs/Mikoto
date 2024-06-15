using System.Globalization;

namespace Mikoto
{
    public class TencentOldLanguageCodeConverter : ILanguageCodeConverter
    {
        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            return cultureInfo.Name switch
            {
                "zh-Hant" => "zh-TW",
                _ => cultureInfo.Name,
            };
        }
    }
}
