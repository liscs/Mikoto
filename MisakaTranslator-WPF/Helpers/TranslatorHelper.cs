using TranslatorLibrary.Translator;

namespace MisakaTranslator.Helpers
{
    internal static class TranslatorHelper
    {

        /// <summary>
        /// 根据翻译器名称自动返回翻译器类实例(包括初始化)
        /// </summary>
        /// <param name="translatorName"></param>
        /// <returns></returns>
        public static ITranslator? GetTranslator(string translatorName)
        {
            return translatorName switch
            {
                "BaiduTranslator" => BaiduTranslator.TranslatorInit(Common.AppSettings.BDappID, Common.AppSettings.BDsecretKey),
                "TencentOldTranslator" => TencentOldTranslator.TranslatorInit(Common.AppSettings.TXOSecretId, Common.AppSettings.TXOSecretKey),
                "CaiyunTranslator" => CaiyunTranslator.TranslatorInit(Common.AppSettings.CaiyunToken),
                "XiaoniuTranslator" => XiaoniuTranslator.TranslatorInit(Common.AppSettings.xiaoniuApiKey),
                "IBMTranslator" => IBMTranslator.TranslatorInit(Common.AppSettings.IBMApiKey, Common.AppSettings.IBMURL),
                "YandexTranslator" => YandexTranslator.TranslatorInit(Common.AppSettings.YandexApiKey),
                "YoudaoZhiyun" => YoudaoZhiyun.TranslatorInit(Common.AppSettings.YDZYAppId, Common.AppSettings.YDZYAppSecret),
                "GoogleCNTranslator" => GoogleCNTranslator.TranslatorInit(),
                "JBeijingTranslator" => JBeijingTranslator.TranslatorInit(Common.AppSettings.JBJCTDllPath),
                "KingsoftFastAITTranslator" => KingsoftFastAITTranslator.TranslatorInit(Common.AppSettings.KingsoftFastAITPath),
                "DreyeTranslator" => DreyeTranslator.TranslatorInit(Common.AppSettings.DreyePath),
                "DeepLTranslator" => DeepLTranslator.TranslatorInit(Common.AppSettings.DeepLsecretKey, Common.AppSettings.DeepLsecretKey),
                "ChatGPTTranslator" => ChatGPTTranslator.TranslatorInit(Common.AppSettings.ChatGPTapiKey, Common.AppSettings.ChatGPTapiUrl),
                "AzureTranslator" => AzureTranslator.TranslatorInit(Common.AppSettings.AzureSecretKey, Common.AppSettings.AzureLocation),
                "ArtificialTranslator" => ArtificialTranslator.TranslatorInit(Common.AppSettings.ArtificialPatchPath),
                "VolcanoTranslator" => VolcanoTranslator.TranslatorInit(Common.AppSettings.VolcanoId, Common.AppSettings.VolcanoKey),
                _ => null,
            };
        }
    }
}