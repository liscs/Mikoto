using System.Globalization;

namespace TranslatorLibrary.LanguageCode
{
    public class YoudaoZhiyunLanguageCodeConverter : ILanguageCodeConverter
    {
        public string GetLanguageCode(CultureInfo cultureInfo)
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
