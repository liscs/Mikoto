using Mikoto.Config;
using Mikoto.Core;
using Mikoto.Translators.Implementations;
using Mikoto.Translators.Interfaces;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Mikoto.Translators
{
    public static partial class TranslatorCommon
    {
        public static ITranslatorFactory TranslatorFactory { get; } = new TranslatorFactory();

        /// <summary>
        /// 根据翻译器名称自动返回翻译器类实例(包括初始化)
        /// </summary>
        /// <param name="translatorName"></param>
        /// <returns></returns>
        public static ITranslator? GetTranslator(string translatorName, IAppSettings appSettings, string translatorDisplayName)
        {
            return TranslatorFactory.GetTranslator(translatorName, appSettings, translatorDisplayName);
        }

        /// <summary>
        /// 刷新/初始化翻译器
        /// </summary>
        public static void Refresh(IResourceService resourceService)
        {
            DisplayNameTranslatorNameDict.Clear();
            TranslatorNameDisplayNameDict.Clear();

            // 遍历由 Source Generator 在编译时生成的列表
            foreach (string className in AllTranslatorClassNames)
            {
                // className 就是 "BaiduTranslator", "TencentOldTranslator" 等
                string displayName = resourceService.Get(className);
                if (string.IsNullOrEmpty(displayName)) displayName= className;


                DisplayNameTranslatorNameDict[displayName]= className;
                TranslatorNameDisplayNameDict[className] = displayName;

            }
            // 默认值兼容性处理
            TranslatorNameDisplayNameDict["NoTranslate"] = resourceService.Get(nameof(NoTranslator));
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
            // 核心：告诉 Options 使用 Source Generator 生成的类型信息，而不是反射
            TypeInfoResolver = TranslatorJsonContext.Default
        };
        public static Dictionary<string, string> TranslatorNameDisplayNameDict { get; } = new();
    }

    [JsonSourceGenerationOptions(IncludeFields = true,WriteIndented = false)]
    [JsonSerializable(typeof(JsonNode))]
    [JsonSerializable(typeof(JsonObject))]
    [JsonSerializable(typeof(JsonArray))]
    [JsonSerializable(typeof(XiaoniuTransOutInfo))]
    [JsonSerializable(typeof(List<AzureTransOutInfo>))]
    [JsonSerializable(typeof(BaiduTransOutInfo))]
    [JsonSerializable(typeof(CaiyunTransResult))]
    [JsonSerializable(typeof(ChatResponse))]
    [JsonSerializable(typeof(ChatResErr))]
    [JsonSerializable(typeof(DeepLTranslateResult))]
    [JsonSerializable(typeof(GoogleApiErrorResponse))]
    [JsonSerializable(typeof(GoogleTranslateResponse))]
    // 为 IBM 的 Result 指定一个唯一的属性名
    [JsonSerializable(typeof(IBMTranslator.Result), TypeInfoPropertyName = "IBMResult")]
    // 为 Yandex 的 Result 指定一个唯一的属性名
    [JsonSerializable(typeof(YandexTranslator.Result), TypeInfoPropertyName = "YandexResult")]
    [JsonSerializable(typeof(YoudaoZhiyunResult))]
    [JsonSerializable(typeof(GoogleTranslateRequest))]
    [JsonSerializable(typeof(VolcanoRequest))]
    internal partial class TranslatorJsonContext : JsonSerializerContext
    {
        // 定义一个私有静态变量来缓存实例
        private static TranslatorJsonContext? _aotSafeContext;

        public static TranslatorJsonContext AotSafeContext => _aotSafeContext ??= new TranslatorJsonContext(new JsonSerializerOptions
        {
            IncludeFields = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            ReadCommentHandling = JsonCommentHandling.Skip
        });
    }
}
