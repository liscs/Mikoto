using System.Globalization;

namespace TranslatorLibrary.LanguageCode
{
    public class GoogleCNLanguageCodeConverter : ILanguageCodeConverter
    {
        public string GetLanguageCode(CultureInfo cultureInfo)
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
