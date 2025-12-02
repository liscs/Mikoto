using Mikoto.Config;
using Mikoto.Translators.Interfaces;

namespace Mikoto.Translators
{
    public interface ITranslatorFactory
    {
        public ITranslator? GetTranslator(string translatorName, IAppSettings appSettings, string translatorDisplayName);

    }
}