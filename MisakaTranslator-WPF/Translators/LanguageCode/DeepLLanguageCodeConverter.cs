using System.Globalization;

namespace MisakaTranslator
{
    public class DeepLLanguageCodeConverter : ILanguageCodeConverter
    {
        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            return cultureInfo.TwoLetterISOLanguageName switch
            {
                "en" => "EN-US",
                "pt" => "PT-BR",
                _ => cultureInfo.TwoLetterISOLanguageName.ToUpper(),
            };
        }
    }
}
