﻿using Mikoto.Helpers.Network;
using Mikoto.Translators.Interfaces;
using Mikoto.Translators.LanguageCode;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Windows;

namespace Mikoto.Translators.Implementations
{
    public class YoudaoZhiyun : ITranslator
    {
        private YoudaoZhiyun() { }
        private static readonly string TRANSLATE_API_URL = "https://openapi.youdao.com/api";
        private string? appId, appSecret;
        private string errorInfo = string.Empty;

        public string TranslatorDisplayName { get { return Application.Current.Resources["YoudaoZhiyun"].ToString()!; } }

        public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            if (sourceText == "" || desLang == "" || srcLang == "")
            {
                errorInfo = "Param Missing";
                return null;
            }
            srcLang = GetLanguageCode(new CultureInfo(srcLang));
            desLang = GetLanguageCode(new CultureInfo(desLang));

            string q = sourceText;
            string input = q.Length <= 20 ? q : q.Substring(0, 10) + q.Length + q.Substring(q.Length - 10);
            string salt = DateTime.Now.Millisecond.ToString();
            string curtime = GetTimeStamp();
            string sign = BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(appId + input + salt + curtime + appSecret))).Replace("-", "").ToLower();

            Dictionary<string, string?> dic = new Dictionary<string, string?>
            {
                { "from", srcLang },
                { "to", desLang },
                { "signType", "v3" },
                { "curtime", curtime },
                { "appKey", appId },
                { "salt", salt },
                { "sign", sign },
                { "q", HttpUtility.UrlEncode(q) }
            };

            string payload = BuildPayload(dic);

            StringContent request = new StringContent(payload, null, "application/x-www-form-urlencoded");

            try
            {
                HttpResponseMessage response = await CommonHttpClient.Instance.PostAsync(TRANSLATE_API_URL, request);
                if (response.IsSuccessStatusCode)
                {
                    string resultStr = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<YoudaoZhiyunResult>(resultStr);
                    if (result.errorCode == "0")
                    {
                        return string.Join("\n", result.translation);
                    }
                    else
                    {
                        errorInfo = "API error code: " + result.errorCode;
                        return null;
                    }
                }
                else
                {
                    errorInfo = "API response code: " + response.StatusCode;
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
            YoudaoZhiyun youdaoZhiyun = new();
            youdaoZhiyun.appId = param.First();
            youdaoZhiyun.appSecret = param.Last();
            return youdaoZhiyun;
        }

        /// <summary>
        /// 计算时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }
        public string GetLastError()
        {
            return errorInfo;
        }

        /// <summary>
        /// 有道智云API申请地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_API()
        {
            return "https://ai.youdao.com/product-fanyi-text.s";
        }

        /// <summary>
        /// 有道智云API额度查询地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Bill()
        {
            return "https://ai.youdao.com/console";
        }

        /// <summary>
        /// 有道智云API文档地址（错误代码）
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Doc()
        {
            return "https://ai.youdao.com/DOCSIRMA/html/自然语言翻译/API文档/文本翻译服务/文本翻译服务-API文档.html";
        }

        private string BuildPayload(Dictionary<string, string?> dic)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append('&');
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }

            return builder.ToString();
        }

        private string GetLanguageCode(CultureInfo cultureInfo)
        {
            return YoudaoZhiyunLanguageCodeConverter.GetLanguageCode(cultureInfo);
        }
    }

#pragma warning disable 0649
    internal struct YoudaoZhiyunResult
    {
        public string errorCode, query, l;
        public string[] translation;
    }
}
