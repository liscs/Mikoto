using System.Globalization;

namespace TranslatorLibrary;
public interface ILanguageCodeConverter
{
    string GetLanguageCode(CultureInfo cultureInfo);
}
