using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TranslatorLibrary.lang;

/*
 * DeepL translator integration
 * Author: kjstart
 * API version: v2
 */
namespace TranslatorLibrary.Translator
{
    public class DeepLTranslator : ITranslator
    {
        public static readonly string SIGN_UP_URL = "https://www.deepl.com/pro?cta=menu-login-signup";
        public static readonly string BILL_URL = "https://www.deepl.com/pro-account/usage";
        public static readonly string DOCUMENT_URL = "https://www.deepl.com/docs-api/accessing-the-api/error-handling/";

        private static readonly string TRANSLATE_API_URL = "https://api-free.deepl.com/v2/translate";

        private string secretKey; //DeepL翻译API的秘钥
        private string errorInfo; //错误信息

        public string TranslatorDisplayName { get { return Strings.DeepLTranslator; } }

        public string GetLastError()
        {
            return errorInfo;
        }

        public async Task<string> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            if (sourceText == "" || desLang == "" || srcLang == "")
            {
                errorInfo = "Param Missing";
                return null;
            }

            string payload = "text=" + sourceText
                + "&auth_key=" + secretKey
                + "&source_lang=" + transformSrcLangKey(srcLang)
                + "&target_lang=" + transformDesLangKey(desLang);

            StringContent request = new StringContent(payload, null, "application/x-www-form-urlencoded");

            try
            {
                HttpResponseMessage response = await TranslatorCommon.GetHttpClient().PostAsync(TRANSLATE_API_URL, request);
                if (response.IsSuccessStatusCode)
                {
                    string resultStr = await response.Content.ReadAsStringAsync();
                    DeepLTranslateResult translateResult = JsonSerializer.Deserialize<DeepLTranslateResult>(resultStr, TranslatorCommon.JsonOP);
                    if (translateResult.translations?.Length > 0)
                    {
                        return translateResult.translations[0].text;
                    }
                    else
                    {
                        errorInfo = "Cannot get translation from: " + resultStr;
                        return null;
                    }
                }
                else
                {
                    errorInfo = "API return code: " + response.StatusCode;
                    return null;
                }
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

        public void TranslatorInit(string param1, string param2)
        {
            secretKey = param1;
        }

        private string transformSrcLangKey(string langKey)
        {
            switch (langKey)
            {
                case "zh":
                    return "ZH";
                case "en":
                    return "EN";
                case "ja":
                    return "JA";
                case "ko":
                    errorInfo = "Korean is not supported by DeepL";
                    return null;
                case "ru":
                    return "RU";
                case "fr":
                    return "FR";

            }
            errorInfo = "Unknown language tag: " + langKey;
            return null;
        }

        private string transformDesLangKey(string langKey)
        {
            switch (langKey)
            {
                case "zh":
                    return "ZH";
                case "en":
                    return "EN-US";
                case "ja":
                    return "JA";
                case "ko":
                    errorInfo = "Korean is not supported by DeepL";
                    return null;
                case "ru":
                    return "RU";
                case "fr":
                    return "FR";

            }
            errorInfo = "Unknown language tag: " + langKey;
            return null;
        }
    }

#pragma warning disable 0649
    struct DeepLTranslateResult
    {
        public DeepLTranslations[] translations;
    }

    struct DeepLTranslations
    {
        public string detected_source_language;
        public string text;
    }
}
