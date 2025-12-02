using Mikoto.Config;
using Mikoto.Translators.Implementations;
using Mikoto.Translators.Interfaces;

namespace Mikoto.Translators
{
    public class TranslatorFactory : ITranslatorFactory
    {
        private readonly Dictionary<string, Func<IAppSettings, string, ITranslator>> _creatorMap = new();

        public TranslatorFactory()
        {
            RegisterAllTranslators();
        }


        private void RegisterAllTranslators()
        {
            // Lambda 表达式负责从 IAppSettings 中 "提取" 最小必需参数，然后调用翻译器的构造函数。

            _creatorMap.Add(nameof(BaiduTranslator), (settings, displayName) =>
                new BaiduTranslator(displayName, settings.BDappID, settings.BDsecretKey)
            );

            _creatorMap.Add(nameof(TencentOldTranslator), (settings, displayName) =>
                new TencentOldTranslator(displayName, settings.TXOSecretId, settings.TXOSecretKey)
            );

            _creatorMap.Add(nameof(CaiyunTranslator), (settings, displayName) =>
                new CaiyunTranslator(displayName, settings.CaiyunToken)
            );

            _creatorMap.Add(nameof(XiaoniuTranslator), (settings, displayName) =>
                new XiaoniuTranslator(displayName, settings.XiaoniuApiKey)
            );

            _creatorMap.Add(nameof(IBMTranslator), (settings, displayName) =>
                new IBMTranslator(displayName, settings.IBMApiKey, settings.IBMURL)
            );

            _creatorMap.Add(nameof(YandexTranslator), (settings, displayName) =>
                new YandexTranslator(displayName, settings.YandexApiKey)
            );

            _creatorMap.Add(nameof(YoudaoZhiyun), (settings, displayName) =>
                new YoudaoZhiyun(displayName, settings.YDZYAppId, settings.YDZYAppSecret)
            );

            _creatorMap.Add(nameof(GoogleCNTranslator), (settings, displayName) =>
                new GoogleCNTranslator(displayName)
            );

            _creatorMap.Add(nameof(JBeijingTranslator), (settings, displayName) =>
                new JBeijingTranslator(displayName, settings.JBJCTDllPath)
            );

            _creatorMap.Add(nameof(KingsoftFastAITTranslator), (settings, displayName) =>
                new KingsoftFastAITTranslator(displayName, settings.KingsoftFastAITPath)
            );

            _creatorMap.Add(nameof(DreyeTranslator), (settings, displayName) =>
                new DreyeTranslator(displayName, settings.DreyePath)
            );

            _creatorMap.Add(nameof(DeepLTranslator), (settings, displayName) =>
                new DeepLTranslator(displayName, settings.DeepLsecretKey)
            );

            _creatorMap.Add(nameof(ChatGPTTranslator), (settings, displayName) =>
                new ChatGPTTranslator(displayName, settings.ChatGPTapiKey, settings.ChatGPTapiUrl, settings.ChatGPTapiModel)
            );

            _creatorMap.Add(nameof(AzureTranslator), (settings, displayName) =>
                new AzureTranslator(displayName, settings.AzureSecretKey, settings.AzureLocation)
            );

            _creatorMap.Add(nameof(ArtificialTranslator), (settings, displayName) =>
                new ArtificialTranslator(displayName, settings.ArtificialPatchPath)
            );

            _creatorMap.Add(nameof(VolcanoTranslator), (settings, displayName) =>
                new VolcanoTranslator(displayName, settings.VolcanoId, settings.VolcanoKey)
            );

            _creatorMap.Add(nameof(AwsTranslator), (settings, displayName) =>
                new AwsTranslator(displayName, settings.AwsAccessKey, settings.AwsSecretKey)
            );

            _creatorMap.Add(nameof(NoTranslator), (settings, displayName) =>
                new NoTranslator(displayName)
            );
        }

        public ITranslator? GetTranslator(string translatorName,
                                          IAppSettings appSettings,
                                          string translatorDisplayName)
        {
            if (_creatorMap.TryGetValue(translatorName, out var factoryDelegate))
            {
                // 调用委托，传入最新的 appSettings
                return factoryDelegate(appSettings, translatorDisplayName);
            }
            return null;
        }

    }
}
