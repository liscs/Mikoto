using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageCodeLibrary
{
    internal class DefaultLanguageCodeConverter : ILanguageCodeConverter
    {
        public string GetLanguageCode(Language language)
        {
            switch (language)
            {
                case Language.Simplified_Chinese:
                    return "zh";
                case Language.Traditional_Chinese:
                    return "cht";
                case Language.English:
                    return "en";
                case Language.Japanese:
                    return "jp";
                case Language.Korean:
                    return "kor";
                case Language.Spanish:
                    return "spa";
                case Language.French:
                    return "fra";
                case Language.Portuguese:
                    return "pt";
                case Language.Russian:
                    return "ru";
                case Language.German:
                    return "de";
                case Language.Italian:
                    return "it";
                default:
                    return "zh";
            }
        }
    }
}
