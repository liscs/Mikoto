using Mikoto.Config;
using Mikoto.Core;
using Mikoto.Translators.Interfaces;
using System.Text.Encodings.Web;

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
