using System.Globalization;

namespace Mikoto.Translators.LanguageCode
{
    public class DeepLLanguageCodeConverter : ILanguageCodeConverter
    {
        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            return cultureInfo.Name switch
            {
                "en" => "EN-US",
                "pt" => "PT-BR",
                _ => cultureInfo.Name.ToUpper(),
            };
        }
    }
}
