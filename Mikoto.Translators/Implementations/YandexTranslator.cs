using Mikoto.Translators.Interfaces;
using System.Text.Json;
using System.Web;


namespace Mikoto.Translators.Implementations
{
    public class YandexTranslator : ITranslator
    {
        public string? ApiKey;

        private string errorInfo = string.Empty;

        public string TranslatorDisplayName { get; private set; } = nameof(YandexTranslator);

        public string GetLastError()
        {
            return errorInfo;
        }

        public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            var hc = TranslateHttpClient.Instance;
            string apiurl = "https://translate.yandex.net/api/v1.5/tr.json/translate?key=" + ApiKey + "&lang=" + srcLang + "-" + desLang + "&text=";

            try
            {
                string retString = await hc.GetStringAsync(apiurl + HttpUtility.UrlEncode(sourceText));
                var doc = JsonSerializer.Deserialize<Result>(retString, TranslatorCommon.JsonSerializerOptions);
                return doc.text[0];
            }
            catch (HttpRequestException ex)
            {
                errorInfo = ex.Message;
                return null;
            }
            catch (TaskCanceledException ex)
            {
                errorInfo = ex.Message;
                return null;
            }
        }

        public YandexTranslator(string displayName, string apiKey)
        {
            TranslatorDisplayName = displayName;
            ApiKey = apiKey;
        }

        /// <summary>
        /// Yandex翻译API申请地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_API()
        {
            return "https://translate.yandex.com/developers/keys";
        }

        /// <summary>
        /// Yandex翻译API额度查询地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Bill()
        {
            return "https://translate.yandex.com/developers/stat";
        }

        /// <summary>
        /// Yandex翻译API文档地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Doc()
        {
            return "https://yandex.com/dev/translate/doc/dg/reference/translate.html";
        }

#pragma warning disable 0649
        private struct Result
        {
            public int code;
            public string[] text;
        }
    }
}
