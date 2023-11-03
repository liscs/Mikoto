using System.Globalization;

namespace TranslatorLibrary
{
    public class AzureLanguageCodeConverter : ILanguageCodeConverter
    {
        public string GetLanguageCode(CultureInfo cultureInfo)
        {
            switch (cultureInfo.Name)
            {
                case "zh-CN":
                    return "zh-Hans";
                case "zh-TW":
                    return "zh-Hant";
                case "en":
                    return "en";
                case "ja":
                    return "ja";
                case "ko":
                    return "ko";
                case "es":
                    return "es";
                case "fr":
                    return "fr";
                case "pt":
                    return "pt";
                case "ru":
                    return "ru";
                case "de":
                    return "de";
                case "it":
                    return "it";
                default:
                    return "zh";
            }
        }
    }
}
