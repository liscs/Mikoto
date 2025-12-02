using Mikoto.Config;
using Mikoto.Core;
using Mikoto.Translators.Implementations;
using Mikoto.Translators.Interfaces;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Mikoto.Translators
{
    public static class TranslatorCommon
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
                nameof(BaiduTranslator) => BaiduTranslator.TranslatorInit(translatorDisplayName,appSettings.BDappID, appSettings.BDsecretKey),
                nameof(TencentOldTranslator) => TencentOldTranslator.TranslatorInit(translatorDisplayName,appSettings.TXOSecretId, appSettings.TXOSecretKey),
                nameof(CaiyunTranslator) => CaiyunTranslator.TranslatorInit(translatorDisplayName,appSettings.CaiyunToken),
                nameof(XiaoniuTranslator) => XiaoniuTranslator.TranslatorInit(translatorDisplayName,appSettings.XiaoniuApiKey),
                nameof(IBMTranslator) => IBMTranslator.TranslatorInit(translatorDisplayName,appSettings.IBMApiKey, appSettings.IBMURL),
                nameof(YandexTranslator) => YandexTranslator.TranslatorInit(translatorDisplayName,appSettings.YandexApiKey),
                nameof(YoudaoZhiyun) => YoudaoZhiyun.TranslatorInit(translatorDisplayName,appSettings.YDZYAppId, appSettings.YDZYAppSecret),
                nameof(GoogleCNTranslator) => GoogleCNTranslator.TranslatorInit(translatorDisplayName),
                nameof(JBeijingTranslator) => JBeijingTranslator.TranslatorInit(translatorDisplayName,appSettings.JBJCTDllPath),
                nameof(KingsoftFastAITTranslator) => KingsoftFastAITTranslator.TranslatorInit(translatorDisplayName,appSettings.KingsoftFastAITPath),
                nameof(DreyeTranslator) => DreyeTranslator.TranslatorInit(translatorDisplayName,appSettings.DreyePath),
                nameof(DeepLTranslator) => DeepLTranslator.TranslatorInit(translatorDisplayName,appSettings.DeepLsecretKey),
                nameof(ChatGPTTranslator) => ChatGPTTranslator.TranslatorInit(translatorDisplayName,appSettings.ChatGPTapiKey, appSettings.ChatGPTapiUrl, appSettings.ChatGPTapiModel),
                nameof(AzureTranslator) => AzureTranslator.TranslatorInit(translatorDisplayName,appSettings.AzureSecretKey, appSettings.AzureLocation),
                nameof(ArtificialTranslator) => ArtificialTranslator.TranslatorInit(translatorDisplayName,appSettings.ArtificialPatchPath),
                nameof(VolcanoTranslator) => VolcanoTranslator.TranslatorInit(translatorDisplayName,appSettings.VolcanoId, appSettings.VolcanoKey),
                nameof(AwsTranslator) => AwsTranslator.TranslatorInit(translatorDisplayName,appSettings.AwsAccessKey, appSettings.AwsSecretKey),
                nameof(NoTranslator) => NoTranslator.TranslatorInit(translatorDisplayName),
                _ => null,
            };
        }

        /// <summary>
        /// 刷新/初始化翻译器
        /// </summary>
        public static void Refresh(IResourceService resourceService)
        {
            //反射获取所有的翻译器（即所有实现了ITranslator的类），放入字典
            Task.Run(() =>
            {
                TranslatorDict.Clear();
                Type type = typeof(ITranslator);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                                                   .SelectMany(s => s.GetTypes())
                                                   .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);
                foreach (Type item in types)
                {
                    object? obj = Activator.CreateInstance(item, true);
                    string displayName = resourceService.Get(item.Name);
                    if (!string.IsNullOrEmpty(displayName))
                    {
                        TranslatorDict.Add(displayName, item.Name);
                    }
                }
            });
        }

        // 默认使用cultureinfo的语言代码
        public static Dictionary<string, string> LanguageDict = new Dictionary<string, string>()
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

        public static Dictionary<string, string> TranslatorDict { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 获取所有可用的翻译API列表
        /// </summary>
        /// <returns></returns>
        public static List<string> GetTranslatorList()
        {
            return TranslatorDict.Keys.ToList();
        }

        /// <summary>
        /// 返回翻译API的值（用于存储的值）的索引
        /// </summary>
        /// <param name="TranslatorValue"></param>
        /// <returns></returns>
        public static int GetTranslatorIndex(string TranslatorValue)
        {
            for (int i = 0; i < TranslatorDict.Count; i++)
            {
                var kvp = TranslatorDict.ElementAt(i);
                if (kvp.Value == TranslatorValue)
                {
                    return i;
                }
            }
            return -1;
        }


        public static System.Text.Json.JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
        {
            IncludeFields = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),

        };
    }
}
