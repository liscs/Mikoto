using Mikoto.Translators.Interfaces;
using Mikoto.Translators.LanguageCode;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Mikoto.Translators.Implementations
{
    public class XiaoniuTranslator : ITranslator
    {
        private XiaoniuTranslator() { }
        public string? apiKey;//小牛翻译API 的APIKEY
        private string errorInfo = string.Empty;//错误信息

        public string TranslatorDisplayName { get; private set; }

        public string GetLastError()
        {
            return errorInfo;
        }

        public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            srcLang = GetLanguageCode(new CultureInfo(srcLang));
            desLang = GetLanguageCode(new CultureInfo(desLang));
            // 原文
            string q = sourceText;

            string retString;

            var sb = new StringBuilder("https://api.niutrans.com/NiuTransServer/translation?")
                .Append("&from=").Append(srcLang)
                .Append("&to=").Append(desLang)
                .Append("&apikey=").Append(apiKey)
                .Append("&src_text=").Append(Uri.EscapeDataString(q));

            string url = sb.ToString();

            var hc = TranslateHttpClient.Instance;
            try
            {
                retString = await hc.GetStringAsync(url);
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                errorInfo = ex.Message;
                return null;
            }
            catch (TaskCanceledException ex)
            {
                errorInfo = ex.Message;
                return null;
            }

            XiaoniuTransOutInfo oinfo = JsonSerializer.Deserialize<XiaoniuTransOutInfo>(retString, TranslatorCommon.JsonSerializerOptions);

            if (oinfo.error_code == null || oinfo.error_code == "52000")
            {
                //得到翻译结果
                if (oinfo.tgt_text != null)
                {
                    return oinfo.tgt_text;
                }
                else
                {
                    errorInfo = "UnknownError";
                    return null;
                }
            }
            else
            {
                if (oinfo.error_msg != null)
                {
                    errorInfo = "ErrorID:" + oinfo.error_msg;
                    return null;
                }
                else
                {
                    errorInfo = "UnknownError";
                    return null;
                }
            }

        }

        private string GetLanguageCode(CultureInfo cultureInfo)
        {
            return XiaoniuLanguageCodeConverter.GetLanguageCode(cultureInfo);
        }

        public static ITranslator TranslatorInit(params string[] param)
        {
            XiaoniuTranslator xiaoniuTranslator = new()
            {
                TranslatorDisplayName = param[0],
                apiKey = param[1]
            };
            return xiaoniuTranslator;
        }

        /// <summary>
        /// 小牛翻译API申请地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_API()
        {
            return "https://niutrans.com/API";
        }

        /// <summary>
        /// 小牛翻译API额度查询地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Bill()
        {
            return "https://niutrans.com/cloud/console/statistics/free";
        }

        /// <summary>
        /// 小牛翻译API文档地址（错误代码）
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Doc()
        {
            return "https://niutrans.com/documents/develop/develop_text/free#error";
        }
    }

#pragma warning disable 0649
    internal struct XiaoniuTransOutInfo
    {
        public string from;
        public string to;
        public string src_text;
        public string tgt_text;
        public string error_code;
        public string error_msg;
    }
}
