using System.Globalization;

namespace TranslatorLibrary.LanguageCode
{
    public static class GoogleCNLanguageCodeConverter 
    {
        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            switch (cultureInfo.TwoLetterISOLanguageName)
            {
                case "zh":
                    return "zh-cn";
                default:
                    return cultureInfo.TwoLetterISOLanguageName;
            }
        }
    }
}
