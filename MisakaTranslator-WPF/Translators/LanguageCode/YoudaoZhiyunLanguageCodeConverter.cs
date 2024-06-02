using System.Globalization;

namespace MisakaTranslator.LanguageCode
{
    public static class YoudaoZhiyunLanguageCodeConverter
    {
        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            switch (cultureInfo.TwoLetterISOLanguageName)
            {
                case "zh":
                    return "zh-CHS";
                default:
                    return cultureInfo.TwoLetterISOLanguageName;
            }
        }
    }
}
