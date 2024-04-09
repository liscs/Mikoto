using TranslatorLibrary.Translator;

namespace MisakaTranslator.Helpers
{
    internal static class TranslatorHelper
    {

        /// <summary>
        /// 根据翻译器名称自动返回翻译器类实例(包括初始化)
        /// </summary>
        public static ITranslator? GetTranslator(string translatorName)
        {
            return translatorName switch
            {
                nameof(BaiduTranslator) => BaiduTranslator.TranslatorInit(Common.AppSettings.BDappID, Common.AppSettings.BDsecretKey),
                nameof(TencentOldTranslator) => TencentOldTranslator.TranslatorInit(Common.AppSettings.TXOSecretId, Common.AppSettings.TXOSecretKey),
                nameof(CaiyunTranslator) => CaiyunTranslator.TranslatorInit(Common.AppSettings.CaiyunToken),
                nameof(XiaoniuTranslator) => XiaoniuTranslator.TranslatorInit(Common.AppSettings.xiaoniuApiKey),
                nameof(IBMTranslator) => IBMTranslator.TranslatorInit(Common.AppSettings.IBMApiKey, Common.AppSettings.IBMURL),
                nameof(YandexTranslator) => YandexTranslator.TranslatorInit(Common.AppSettings.YandexApiKey),
                nameof(YoudaoZhiyun) => YoudaoZhiyun.TranslatorInit(Common.AppSettings.YDZYAppId, Common.AppSettings.YDZYAppSecret),
                nameof(GoogleCNTranslator) => GoogleCNTranslator.TranslatorInit(),
                nameof(JBeijingTranslator) => JBeijingTranslator.TranslatorInit(Common.AppSettings.JBJCTDllPath),
                nameof(KingsoftFastAITTranslator) => KingsoftFastAITTranslator.TranslatorInit(Common.AppSettings.KingsoftFastAITPath),
                nameof(DreyeTranslator) => DreyeTranslator.TranslatorInit(Common.AppSettings.DreyePath),
                nameof(DeepLTranslator) => DeepLTranslator.TranslatorInit(Common.AppSettings.DeepLsecretKey, Common.AppSettings.DeepLsecretKey),
                nameof(ChatGPTTranslator) => ChatGPTTranslator.TranslatorInit(Common.AppSettings.ChatGPTapiKey, Common.AppSettings.ChatGPTapiUrl),
                nameof(AzureTranslator) => AzureTranslator.TranslatorInit(Common.AppSettings.AzureSecretKey, Common.AppSettings.AzureLocation),
                nameof(ArtificialTranslator) => ArtificialTranslator.TranslatorInit(Common.AppSettings.ArtificialPatchPath),
                nameof(VolcanoTranslator) => VolcanoTranslator.TranslatorInit(Common.AppSettings.VolcanoId, Common.AppSettings.VolcanoKey),
                nameof(AwsTranslator) => AwsTranslator.TranslatorInit(Common.AppSettings.AwsAccessKey, Common.AppSettings.AwsSecretKey),
                _ => null,
            };
        }
    }
}