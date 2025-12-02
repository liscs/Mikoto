using Mikoto.Translators.Interfaces;
using Mikoto.Translators.LanguageCode;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Windows;

namespace Mikoto.Translators.Implementations
{
    public class BaiduTranslator : ITranslator
    {
        private BaiduTranslator() { }

        //语言简写列表 https://api.fanyi.baidu.com/product/113

        public string? _appId;//百度翻译API 的APP ID
        public string? _secretKey;//百度翻译API 的密钥
        private string errorInfo = string.Empty;//错误信息
        private static Random _random = new Random();

        public string TranslatorDisplayName { get; private set; }

        public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            if (sourceText == "" || desLang == "" || srcLang == "")
            {
                errorInfo = "Param Missing";
                return null;
            }
            srcLang = GetLanguageCode(new CultureInfo(srcLang));
            desLang = GetLanguageCode(new CultureInfo(desLang));

            // 原文
            string q = sourceText;

            string retString;

            string salt = _random.Next(100000).ToString();

            string sign = EncryptString(_appId + q + salt + _secretKey);
            var sb = new StringBuilder("https://api.fanyi.baidu.com/api/trans/vip/translate?")
                .Append("q=").Append(HttpUtility.UrlEncode(q))
                .Append("&from=").Append(srcLang)
                .Append("&to=").Append(desLang)
                .Append("&appid=").Append(_appId)
                .Append("&salt=").Append(salt)
                .Append("&sign=").Append(sign);
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

            BaiduTransOutInfo oinfo = JsonSerializer.Deserialize<BaiduTransOutInfo>(retString, TranslatorCommon.JsonSerializerOptions);

            if (oinfo.error_code == null || oinfo.error_code == "52000")
            {
                // 52000就是成功
                if (oinfo.trans_result.Length == 0)
                    return "";
                else if (oinfo.trans_result.Length == 1)
                    return oinfo.trans_result[0].dst;
                else
                {
                    var sb2 = new StringBuilder();
                    foreach (var entry in oinfo.trans_result)
                        sb2.AppendLine(entry.dst);
                    return sb2.ToString();
                }
            }
            else
            {
                errorInfo = "ErrorID:" + oinfo.error_code;
                return null;
            }

        }

        public static ITranslator TranslatorInit(params string[] param)
        {
            BaiduTranslator baiduTranslator = new()
            {
                TranslatorDisplayName = param[0],
                _appId = param[1],
                _secretKey = param[2],
            };
            return baiduTranslator;
        }

        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EncryptString(string str)
        {
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = MD5.HashData(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }


        public string GetLastError()
        {
            return errorInfo;
        }

        /// <summary>
        /// 百度翻译API申请地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_API()
        {
            return "https://api.fanyi.baidu.com/product/11";
        }

        /// <summary>
        /// 百度翻译API额度查询地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Bill()
        {
            return "https://api.fanyi.baidu.com/api/trans/product/desktop";
        }

        /// <summary>
        /// 百度翻译API文档地址（错误代码）
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Doc()
        {
            return "https://api.fanyi.baidu.com/doc/21";
        }

        private string GetLanguageCode(CultureInfo cultureInfo)
        {
            return BaiduLanguageCodeConverter.GetLanguageCode(cultureInfo);
        }
    }

#pragma warning disable 0649
    internal struct BaiduTransOutInfo
    {
        public string from;
        public string to;
        public BaiduTransResult[] trans_result;
        public string error_code;
    }

    internal struct BaiduTransResult
    {
        public string src;
        public string dst;
    }
}
