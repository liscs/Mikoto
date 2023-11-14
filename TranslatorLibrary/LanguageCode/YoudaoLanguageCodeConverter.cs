using System.Globalization;

namespace TranslatorLibrary.LanguageCode
{
    public class YoudaoLanguageCodeConverter : ILanguageCodeConverter
    {
        public string GetLanguageCode(CultureInfo cultureInfo)
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
