using System.Globalization;

namespace TranslatorLibrary.LanguageCode
{
    public static class YoudaoLanguageCodeConverter 
    {
        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            switch (cultureInfo.TwoLetterISOLanguageName)
            {
                case "zh":
                    return "zh_cn";
                default:
                    return cultureInfo.TwoLetterISOLanguageName;
            }
        }
    }
}
