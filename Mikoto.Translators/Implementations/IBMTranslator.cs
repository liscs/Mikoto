using Mikoto.Translators.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mikoto.Translators.Implementations
{
    public class IBMTranslator : ITranslator
    {
        public string? ApiKey;
        public string? URL;

        private string errorInfo = string.Empty;

        public string DisplayName { get; private set; }

        public string GetLastError()
        {
            return errorInfo;
        }

        public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            if (desLang != "en" && srcLang != "en")
            {
                sourceText = await TranslateAsync(sourceText, "en", srcLang) ?? string.Empty;
                if (string.IsNullOrEmpty(sourceText))
                {
                    return null;
                }

                srcLang = "en";
            }

            HttpResponseMessage resp;
            var hc = TranslateHttpClient.Instance;
            var req = new HttpRequestMessage(HttpMethod.Post, URL);
            var root = new JsonObject
            {
                ["text"] = new JsonArray(sourceText), // JsonNode 体系在 AOT 下是安全的
                ["model_id"] = $"{srcLang}-{desLang}"
            };

            string jsonParam = root.ToJsonString(TranslatorJsonContext.AotSafeContext.Options);
            req.Content = new StringContent(jsonParam, null, "application/json");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(ApiKey ?? string.Empty)));

            try
            {
                resp = await hc.SendAsync(req);
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
            finally
            {
                req.Dispose();
            }

            string retString = await resp.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Result>(retString, TranslatorJsonContext.AotSafeContext.IBMResult);

            if (!resp.IsSuccessStatusCode)
            {
                errorInfo = result.error;
                return null;
            }

            return result.translations[0].translation;
        }

        public IBMTranslator(string displayName, string key, string url)
        {
            DisplayName = displayName;
            ApiKey = "apikey:" + key;
            URL = url + "/v3/translate?version=2018-05-01";
        }

        /// <summary>
        /// IBM翻译API申请地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_API()
        {
            return "https://cloud.ibm.com/catalog/services/language-translator";
        }

        /// <summary>
        /// IBM翻译API额度查询地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Bill()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// IBM翻译API文档地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Doc()
        {
            return "https://cloud.ibm.com/apidocs/language-translator#translate";
        }

#pragma warning disable 0649
        public struct Result
        {
            public Translations[] translations;
            public string error;
        }

        public struct Translations
        {
            public string translation;
        }
    }
}
