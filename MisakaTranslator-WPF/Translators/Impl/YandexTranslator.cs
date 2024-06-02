using System.Net.Http;
using System.Text.Json;
using System.Web;
using System.Windows;


namespace MisakaTranslator.Translators
{
    public class YandexTranslator : ITranslator
    {
        public string? ApiKey;

        private string errorInfo = string.Empty;

        public string TranslatorDisplayName { get { return Application.Current.Resources["YandexTranslator"].ToString()!; } }

        public string GetLastError()
        {
            return errorInfo;
        }

        public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            var hc = TranslatorCommon.GetHttpClient();
            string apiurl = "https://translate.yandex.net/api/v1.5/tr.json/translate?key=" + ApiKey + "&lang=" + srcLang + "-" + desLang + "&text=";

            try
            {
                string retString = await hc.GetStringAsync(apiurl + HttpUtility.UrlEncode(sourceText));
                var doc = JsonSerializer.Deserialize<Result>(retString, TranslatorCommon.JsonOP);
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

        public static ITranslator TranslatorInit(params string[] param)
        {
            YandexTranslator yandexTranslator = new();
            yandexTranslator.ApiKey = param.First();
            return yandexTranslator;
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
