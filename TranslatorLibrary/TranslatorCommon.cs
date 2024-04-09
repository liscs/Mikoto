using System.Net;
using System.Security.Cryptography;
using System.Text;
using TranslatorLibrary.Translator;

namespace TranslatorLibrary
{
    public static class TranslatorCommon
    {
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
                    object? obj = Activator.CreateInstance(item);
                    string? displayName = item.GetProperty("TranslatorDisplayName")?.GetValue(obj)?.ToString();
                    if (!string.IsNullOrEmpty(displayName))
                    {
                        TranslatorDict.Add(displayName, item.Name);
                    }
                }
            });
        }

        // 默认使用cultureinfo的语言代码
        public static Dictionary<string, string> LanguageDict { get; } = new()
        {
            { "中文" , "zh" },
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

        public static Dictionary<string, string> TranslatorDict { get; } = new();
        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EncryptString(string str)
        {
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = MD5.HashData(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }

        /// <summary>
        /// 计算时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }

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

        private static HttpClient? HC;
        /// <summary>
        /// 获得HttpClient单例，第一次调用自动初始化
        /// </summary>
        public static HttpClient GetHttpClient()
        {
            if (HC == null)
                lock (typeof(TranslatorCommon))
                    if (HC == null)
                    {
                        HC = new HttpClient() { Timeout = TimeSpan.FromSeconds(8) };
                        HC.DefaultRequestHeaders.UserAgent.ParseAdd("MisakaTranslator");
                        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12; // For FX4.7
                    }
            return HC;
        }
        public static void SetHttpProxiedClient(string addr)
        {
            if (HC == null)
            {
                var px = new WebProxy() { Address = new Uri(addr), UseDefaultCredentials = true };
                var ph = new HttpClientHandler() { Proxy = px };
                HC = new HttpClient(ph) { Timeout = TimeSpan.FromSeconds(8) };
                HC.DefaultRequestHeaders.UserAgent.ParseAdd("MisakaTranslator");
            }
        }

        public static Random RD { get; } = new Random();

        public static System.Text.Json.JsonSerializerOptions JsonOP { get; } = new()
        {
            IncludeFields = true
        };
    }
}
