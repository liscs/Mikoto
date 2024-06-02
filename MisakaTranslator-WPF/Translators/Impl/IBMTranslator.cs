using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace MisakaTranslator.Translators
{
    public class IBMTranslator : ITranslator
    {
        public string? ApiKey;
        public string? URL;

        private string errorInfo = string.Empty;

        public string TranslatorDisplayName { get { return Application.Current.Resources["IBMTranslator"].ToString()!; } }

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
            var hc = TranslatorCommon.GetHttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, URL);
            string jsonParam = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                {"text", new string[] {sourceText}},
                {"model_id", srcLang + "-" + desLang}
            });
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
            var result = JsonSerializer.Deserialize<Result>(retString, TranslatorCommon.JsonOP);

            if (!resp.IsSuccessStatusCode)
            {
                errorInfo = result.error;
                return null;
            }

            return result.translations[0].translation;
        }

        public static ITranslator TranslatorInit(params string[] param)
        {
            IBMTranslator iBMTranslator = new();
            iBMTranslator.ApiKey = "apikey:" + param.First();
            iBMTranslator.URL = param.Last() + "/v3/translate?version=2018-05-01";
            return iBMTranslator;
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
        private struct Result
        {
            public Translations[] translations;
            public string error;
        }

        private struct Translations
        {
            public string translation;
        }
    }
}
