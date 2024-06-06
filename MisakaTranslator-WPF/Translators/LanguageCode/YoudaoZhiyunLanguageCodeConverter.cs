using System.Globalization;

namespace MisakaTranslator
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
