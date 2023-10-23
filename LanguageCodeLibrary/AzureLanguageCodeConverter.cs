using System;

namespace LanguageCodeLibrary
{
    public class AzureLanguageCodeConverter : ILanguageCodeConverter
    {
        public string GetLanguageCode(Language language)
        {
            switch (language)
            {
                case Language.Simplified_Chinese:
                    return "zh-Hans";
                case Language.Traditional_Chinese:
                    return "zh-Hant";
                case Language.English:
                    return "en";
                case Language.Japanese:
                    return "ja";
                case Language.Korean:
                    return "ko";
                case Language.Spanish:
                    return "es";
                case Language.French:
                    return "fr";
                case Language.Portuguese:
                    return "pt";
                case Language.Russian:
                    return "ru";
                case Language.German:
                    return "de";
                case Language.Italian:
                    return "it";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
