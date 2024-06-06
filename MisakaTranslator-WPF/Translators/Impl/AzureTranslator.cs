﻿using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace MisakaTranslator.Translators
{
    public class AzureTranslator : ITranslator
    {
        private AzureTranslator() { }
        //快速入门：Azure AI 翻译 REST API https://learn.microsoft.com/zh-cn/azure/ai-services/translator/quickstart-text-rest-api?tabs=csharp
        //语言简写列表 https://learn.microsoft.com/zh-CN/azure/ai-services/translator/language-support

        public string? secretKey;//Azure翻译API 的密钥
        public string? location;//Azure翻译API 的位置/区域
        private string errorInfo = string.Empty;//错误信息
        private readonly string endpoint = "https://api.cognitive.microsofttranslator.com";

        public string TranslatorDisplayName { get { return Application.Current.Resources["AzureTranslator"].ToString()!; } }

        public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            if (sourceText == "" || desLang == "" || srcLang == "")
            {
                errorInfo = "Param Missing";
                return null;
            }
            // Input and output languages are defined as parameters.
            string route = $"/translate?api-version=3.0&from={srcLang}&to={desLang}";
            string textToTranslate = sourceText;
            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonSerializer.Serialize(body);
            AzureTransOutInfo oinfo;
            var client = TranslatorCommon.HttpClientInstance;
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", secretKey);
                // location required if you're using a multi-service or regional (not global) resource.
                request.Headers.Add("Ocp-Apim-Subscription-Region", location);
                try
                {
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    string result = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        oinfo = JsonSerializer.Deserialize<List<AzureTransOutInfo>>(result, TranslatorCommon.JsonSerializerOptions)!.ElementAt(0);
                        if (oinfo.translations.Length == 0)
                            return string.Empty;
                        else if (oinfo.translations.Length == 1)
                            return oinfo.translations[0].text;
                        else
                        {
                            var sb2 = new StringBuilder();
                            foreach (var entry in oinfo.translations)
                                sb2.AppendLine(entry.text);
                            return sb2.ToString();
                        }
                    }
                    else
                    {
                        oinfo = JsonSerializer.Deserialize<AzureTransOutInfo>(result, TranslatorCommon.JsonSerializerOptions);
                        errorInfo = $"ErrorCode: {oinfo.error.code}, Message: {oinfo.error.message}";
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    errorInfo = ex.Message;
                    return null;
                }
            }
        }

        public static ITranslator TranslatorInit(params string[] param)
        {
            AzureTranslator azureTranslator = new();
            azureTranslator.secretKey = param.First();
            azureTranslator.location = param.Last();
            return azureTranslator;
        }


        public string GetLastError()
        {
            return errorInfo;
        }

        /// <summary>
        /// Azure翻译API申请地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_API()
        {
            return "https://azure.microsoft.com/zh-cn/products/ai-services/ai-translator";
        }

        /// <summary>
        /// Azure翻译API额度查询地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Bill()
        {
            return "https://portal.azure.com/#home";
        }

        /// <summary>
        /// Azure翻译API语言代码查询
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_lang()
        {
            return "https://learn.microsoft.com/zh-CN/azure/ai-services/translator/language-support";
        }

        /// <summary>
        /// Azure翻译API文档地址（错误代码）
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Doc()
        {
            return "https://docs.azure.cn/zh-cn/ai-services/translator/reference/v3-0-reference";
        }
    }

#pragma warning disable 0649
    internal struct AzureTransOutInfo
    {
        public AzureTransResult[] translations;
        public AzureErrorResult error;
    }

    internal struct AzureTransResult
    {
        public string text;
        public string to;
    }

    internal struct AzureErrorResult
    {
        public int code;
        public string message;
    }
}
