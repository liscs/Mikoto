using System.Globalization;

namespace TranslatorLibrary.LanguageCode
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
