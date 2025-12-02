using Mikoto.Translators.Interfaces;
using Mikoto.Translators.LanguageCode;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using System.Windows;

namespace Mikoto.Translators.Implementations
{
    public class GoogleCNTranslator : ITranslator
    {
        private GoogleCNTranslator() { }
        private string errorInfo = string.Empty;//错误信息

        public string TranslatorDisplayName { get; private set; }

        public string GetLastError()
        {
            return errorInfo;
        }

        public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            try
            {
                srcLang = GetLanguageCode(new CultureInfo(srcLang));
                desLang = GetLanguageCode(new CultureInfo(desLang));

                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&dt=t&sl={srcLang}&tl={desLang}&q={HttpUtility.UrlEncode(sourceText)}";

                var hc = TranslateHttpClient.Instance;
                var json = await hc.GetStringAsync(url);

                JsonNode? root = JsonSerializer.Deserialize<JsonNode?>(json, TranslatorCommon.JsonSerializerOptions);

                if (root == null || root[0] == null)
                {
                    errorInfo = "Http request result is null";
                    return null;
                }

                var resultBuilder = new StringBuilder();

                foreach (var item in root[0]?.AsArray() ?? [])
                {
                    var segment = item?[0]?.ToString();
                    if (!string.IsNullOrEmpty(segment))
                        resultBuilder.Append(segment);
                }

                return resultBuilder.ToString();
            }
            catch (HttpRequestException ex)
            {
                errorInfo = ex.Message;
            }
            catch (TaskCanceledException ex)
            {
                errorInfo = ex.Message;
            }

            return null;
        }

        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            return GoogleCNLanguageCodeConverter.GetLanguageCode(cultureInfo);
        }
        public static ITranslator TranslatorInit(params string[] param)
        {

            return new GoogleCNTranslator() { TranslatorDisplayName = param[0], };
        }
    }
}
