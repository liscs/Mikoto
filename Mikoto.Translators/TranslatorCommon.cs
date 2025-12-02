using Mikoto.Config;
using Mikoto.Core;
using Mikoto.Translators.Implementations;
using Mikoto.Translators.Interfaces;
using System.Text.Encodings.Web;

namespace Mikoto.Translators
{
    public static partial class TranslatorCommon
    {
        /// <summary>
        /// 根据翻译器名称自动返回翻译器类实例(包括初始化)
        /// </summary>
        /// <param name="translatorName"></param>
        /// <returns></returns>
        public static ITranslator? GetTranslator(string translatorName, IAppSettings appSettings, string translatorDisplayName)
        {
            return translatorName switch
            {
                nameof(BaiduTranslator) => BaiduTranslator.TranslatorInit(translatorDisplayName, appSettings.BDappID, appSettings.BDsecretKey),
                nameof(TencentOldTranslator) => TencentOldTranslator.TranslatorInit(translatorDisplayName, appSettings.TXOSecretId, appSettings.TXOSecretKey),
                nameof(CaiyunTranslator) => CaiyunTranslator.TranslatorInit(translatorDisplayName, appSettings.CaiyunToken),
                nameof(XiaoniuTranslator) => XiaoniuTranslator.TranslatorInit(translatorDisplayName, appSettings.XiaoniuApiKey),
                nameof(IBMTranslator) => IBMTranslator.TranslatorInit(translatorDisplayName, appSettings.IBMApiKey, appSettings.IBMURL),
                nameof(YandexTranslator) => YandexTranslator.TranslatorInit(translatorDisplayName, appSettings.YandexApiKey),
                nameof(YoudaoZhiyun) => YoudaoZhiyun.TranslatorInit(translatorDisplayName, appSettings.YDZYAppId, appSettings.YDZYAppSecret),
                nameof(GoogleCNTranslator) => GoogleCNTranslator.TranslatorInit(translatorDisplayName),
                nameof(JBeijingTranslator) => JBeijingTranslator.TranslatorInit(translatorDisplayName, appSettings.JBJCTDllPath),
                nameof(KingsoftFastAITTranslator) => KingsoftFastAITTranslator.TranslatorInit(translatorDisplayName, appSettings.KingsoftFastAITPath),
                nameof(DreyeTranslator) => DreyeTranslator.TranslatorInit(translatorDisplayName, appSettings.DreyePath),
                nameof(DeepLTranslator) => DeepLTranslator.TranslatorInit(translatorDisplayName, appSettings.DeepLsecretKey),
                nameof(ChatGPTTranslator) => ChatGPTTranslator.TranslatorInit(translatorDisplayName, appSettings.ChatGPTapiKey, appSettings.ChatGPTapiUrl, appSettings.ChatGPTapiModel),
                nameof(AzureTranslator) => AzureTranslator.TranslatorInit(translatorDisplayName, appSettings.AzureSecretKey, appSettings.AzureLocation),
                nameof(ArtificialTranslator) => ArtificialTranslator.TranslatorInit(translatorDisplayName, appSettings.ArtificialPatchPath),
                nameof(VolcanoTranslator) => VolcanoTranslator.TranslatorInit(translatorDisplayName, appSettings.VolcanoId, appSettings.VolcanoKey),
                nameof(AwsTranslator) => AwsTranslator.TranslatorInit(translatorDisplayName, appSettings.AwsAccessKey, appSettings.AwsSecretKey),
                nameof(NoTranslator) => NoTranslator.TranslatorInit(translatorDisplayName),
                _ => null,
            };
        }

        /// <summary>
        /// 刷新/初始化翻译器
        /// </summary>
        public static void Refresh(IResourceService resourceService)
        {
            DisplayNameTranslatorNameDict.Clear();

            // 遍历由 Source Generator 在编译时生成的列表
            foreach (string className in AllTranslatorClassNames)
            {
                // className 就是 "BaiduTranslator", "TencentOldTranslator" 等
                string displayName = resourceService.Get(className);

                if (!string.IsNullOrEmpty(displayName))
                {
                    DisplayNameTranslatorNameDict[displayName]= className;
                    TranslatorNameDisplayNameDict[className] = displayName;
                }
            }
        }

        // 默认使用cultureinfo的语言代码
        public static Dictionary<string, string> LanguageDict { get; } = new Dictionary<string, string>()
        {
            { "简体中文" , "zh" },
            { "繁體中文" , "zh-Hant" },
            { "English" , "en" },
            { "日本語" ,  "ja" },
            { "한국어" , "ko" },
            { "Русскийязык" , "ru" },
            { "Français" , "fr" },
            { "Español", "es" },
            { "Português", "pt" },
            { "Deutsch", "de" },
            { "Italiano", "it" }
        };

        public static Dictionary<string, string> DisplayNameTranslatorNameDict { get; } = new();

        /// <summary>
        /// 获取所有可用的翻译API列表
        /// </summary>
        public static List<string> GetTranslatorDisplayNameList()
        {
            return DisplayNameTranslatorNameDict.Keys.ToList();
        }


        public static System.Text.Json.JsonSerializerOptions JsonSerializerOptions { get; } = new()
        {
            IncludeFields = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        public static Dictionary<string, string> TranslatorNameDisplayNameDict { get; } = new();
    }
}
