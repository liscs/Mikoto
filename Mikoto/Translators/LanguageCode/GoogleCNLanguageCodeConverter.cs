using System.Globalization;

namespace Mikoto
{
    public class GoogleCNLanguageCodeConverter : ILanguageCodeConverter
    {
        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            return cultureInfo.Name switch
            {
                "zh" => "zh-CN",
                "zh-Hant" => "zh-TW",
                _ => cultureInfo.Name,
            };
        }
    }
}
