using System.Globalization;
using System.Text.Json;
using TranslatorLibrary.lang;
using TranslatorLibrary.LanguageCode;

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

        private string? secretKey; //DeepL翻译API的秘钥
        private string errorInfo = string.Empty; //错误信息

        public string TranslatorDisplayName { get { return Strings.DeepLTranslator; } }

        public string GetLastError()
        {
            return errorInfo;
        }

        public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            if (sourceText == "" || desLang == "" || srcLang == "")
            {
                errorInfo = "Param Missing";
                return null;
            }
            srcLang = GetLanguageCode(new CultureInfo(srcLang));
            desLang = GetLanguageCode(new CultureInfo(desLang));

            string payload = "text=" + sourceText
                + "&auth_key=" + secretKey
                + "&source_lang=" + srcLang
                + "&target_lang=" + desLang;

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

        public static ITranslator TranslatorInit(params string[] param)
        {
            DeepLTranslator deepLTranslator = new();
            deepLTranslator.secretKey = param.First();
            return deepLTranslator;
        }

        private string GetLanguageCode(CultureInfo cultureInfo)
        {
            return DeepLLanguageCodeConverter.GetLanguageCode(cultureInfo);
        }
    }

#pragma warning disable 0649
    internal struct DeepLTranslateResult
    {
        public DeepLTranslations[] translations;
    }

    internal struct DeepLTranslations
    {
        public string detected_source_language;
        public string text;
    }
}
