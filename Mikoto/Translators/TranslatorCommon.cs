using Mikoto.Translators;
using System.Net;
using System.Net.Http;

namespace Mikoto
{
    public static class TranslatorCommon
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
                nameof(BaiduTranslator) => BaiduTranslator.TranslatorInit(Common.AppSettings.BDappID, Common.AppSettings.BDsecretKey),
                nameof(TencentOldTranslator) => TencentOldTranslator.TranslatorInit(Common.AppSettings.TXOSecretId, Common.AppSettings.TXOSecretKey),
                nameof(CaiyunTranslator) => CaiyunTranslator.TranslatorInit(Common.AppSettings.CaiyunToken),
                nameof(XiaoniuTranslator) => XiaoniuTranslator.TranslatorInit(Common.AppSettings.XiaoniuApiKey),
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

        /// <summary>
        /// 刷新/初始化翻译器
        /// </summary>
        public static void Refresh()
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
                    string? displayName = item.GetProperty(nameof(ITranslator.TranslatorDisplayName))?.GetValue(obj)?.ToString();
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

        private static HttpClient? _httpClient;
        private static readonly object _httpClientLock = new object();
        /// <summary>
        /// 获得HttpClient单例，第一次调用自动初始化
        /// </summary>
        public static HttpClient HttpClientInstance
        {
            get
            {
                if (_httpClient == null)
                {
                    lock (_httpClientLock)
                    {
                        if (_httpClient == null)
                        {
                            _httpClient = new HttpClient()
                            {
                                Timeout = TimeSpan.FromSeconds(8)
                            };
                            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mikoto");
                        }
                    }
                }

                return _httpClient;
            }
        }

        public static void SetHttpProxiedClient(string uriString)
        {
            if (_httpClient == null)
            {
                HttpClientHandler handler = new()
                {
                    Proxy = new WebProxy()
                    {
                        Address = new Uri(uriString),
                        UseDefaultCredentials = true
                    }
                };
                _httpClient = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(8),
                };
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mikoto");
            }
        }


        public static System.Text.Json.JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
        {
            IncludeFields = true
        };
    }
}
