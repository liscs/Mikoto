using System.Globalization;

namespace TranslatorLibrary.LanguageCode
{
    public static class DeepLLanguageCodeConverter
    {
        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            switch (cultureInfo.TwoLetterISOLanguageName)
            {
                case "en":
                    return "EN-US";
                case "pt":
                    return "PT-BR";
                default:
                    return cultureInfo.TwoLetterISOLanguageName.ToUpper();
            }
        }
    }
}
