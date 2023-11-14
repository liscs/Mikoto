using System.Globalization;

namespace TranslatorLibrary
{
    public class BaiduLanguageCodeConverter : ILanguageCodeConverter
    {
        public string GetLanguageCode(CultureInfo cultureInfo)
        {
            switch (cultureInfo.TwoLetterISOLanguageName)
            {
                case "ja":
                    return "jp";
                case "ko":
                    return "kor";
                case "es":
                    return "spa";
                case "fr":
                    return "fra";
                default:
                    return cultureInfo.TwoLetterISOLanguageName;
            }
        }
    }
}
