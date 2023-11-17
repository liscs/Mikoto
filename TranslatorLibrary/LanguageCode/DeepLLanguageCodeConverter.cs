using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
