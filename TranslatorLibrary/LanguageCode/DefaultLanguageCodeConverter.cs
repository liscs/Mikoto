using System.Globalization;

namespace TranslatorLibrary
{
    internal class DefaultLanguageCodeConverter : ILanguageCodeConverter
    {
        public string GetLanguageCode(CultureInfo cultureInfo)
        {
            switch (cultureInfo.Name)
            {
                case "zh-CN":
                    return "zh";
                case "zh-TW":
                    return "cht";
                case "en":
                    return "en";
                case "ja":
                    return "jp";
                case "ko":
                    return "kor";
                case "es":
                    return "spa";
                case "fr":
                    return "fra";
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
