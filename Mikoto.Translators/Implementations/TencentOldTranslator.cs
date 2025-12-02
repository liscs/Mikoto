using Mikoto.Translators.Interfaces;
using Mikoto.Translators.LanguageCode;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mikoto.Translators.Implementations
{
    public class TencentOldTranslator : ITranslator
    {

        private string errorInfo = string.Empty;//错误信息
        public string? SecretId;//腾讯旧版API SecretId
        public string? SecretKey;//腾讯旧版API _secretKey

        public string TranslatorDisplayName { get; private set; }

        public string GetLastError()
        {
            return errorInfo;
        }

        public static string SHA256Hex(string s)
        {
            byte[] hashbytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashbytes.Length; ++i)
            {
                builder.Append(hashbytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public static byte[] HmacSHA256(byte[] key, byte[] msg)
        {
            using HMACSHA256 mac = new(key);
            return mac.ComputeHash(msg);
        }

        public static Dictionary<string, string> BuildHeaders(string secretid, string secretkey, string service,
                                                              string endpoint, string region, string action,
                                                              string version, DateTime date, string requestPayload)
        {
            string datestr = date.ToString("yyyy-MM-dd");
            long requestTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // ************* 步骤 1：拼接规范请求串 *************
            string algorithm = "TC3-HMAC-SHA256";
            string httpRequestMethod = "POST";
            string canonicalUri = "/";
            string canonicalQueryString = "";
            string contentType = "application/json";
            string canonicalHeaders = $"content-type:{contentType}; charset=utf-8\nhost:{endpoint}\nx-tc-action:{action.ToLower()}\n";
            string signedHeaders = "content-type;host;x-tc-action";
            string hashedRequestPayload = SHA256Hex(requestPayload);
            string canonicalRequest = $"{httpRequestMethod}\n{canonicalUri}\n{canonicalQueryString}\n{canonicalHeaders}\n{signedHeaders}\n{hashedRequestPayload}";

            // ************* 步骤 2：拼接待签名字符串 *************
            string credentialScope = $"{datestr}/{service}/tc3_request";
            string hashedCanonicalRequest = SHA256Hex(canonicalRequest);
            string stringToSign = $"{algorithm}\n{requestTimestamp}\n{credentialScope}\n{hashedCanonicalRequest}";

            // ************* 步骤 3：计算签名 *************
            byte[] tc3SecretKey = Encoding.UTF8.GetBytes("TC3" + secretkey);
            byte[] secretDate = HmacSHA256(tc3SecretKey, Encoding.UTF8.GetBytes(datestr));
            byte[] secretService = HmacSHA256(secretDate, Encoding.UTF8.GetBytes(service));
            byte[] secretSigning = HmacSHA256(secretService, Encoding.UTF8.GetBytes("tc3_request"));
            byte[] signatureBytes = HmacSHA256(secretSigning, Encoding.UTF8.GetBytes(stringToSign));
            string signature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

            // ************* 步骤 4：拼接 Authorization *************
            string authorization = $"{algorithm} Credential={secretid}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Authorization", authorization },
                { "Host", endpoint },
                { "Content-Type", $"{contentType}; charset=utf-8" },
                { "X-TC-Timestamp", requestTimestamp.ToString() },
                { "X-TC-Version", version },
                { "X-TC-Action", action },
                { "X-TC-Region", region }
            };
            return headers;
        }

        private const string SERVICE = "tmt";
        private const string ENDPOINT = "tmt.tencentcloudapi.com";
        private const string REGION = "ap-shanghai";
        private const string ACTION = "TextTranslate";
        private const string VERSION = "2018-03-21";

        public async Task<string?> TranslateAsync(string text, string dstLang, string srcLang)
        {
            string?[] paramStrs = [text, dstLang, srcLang, SecretId, SecretKey];
            if (paramStrs.Any(string.IsNullOrEmpty))
            {
                errorInfo = "Param Missing";
                return null;
            }

            srcLang = GetLanguageCode(new CultureInfo(srcLang));
            dstLang = GetLanguageCode(new CultureInfo(dstLang));

            DateTime date = DateTime.UtcNow;
            string requestPayload = JsonSerializer.Serialize(new
            {
                SourceText = text,
                Source = srcLang,
                Target = dstLang,
                ProjectId = 0

            });

            Dictionary<string, string> headers = BuildHeaders(SecretId!, SecretKey!, SERVICE, ENDPOINT, REGION, ACTION,
                                                              VERSION, date, requestPayload);

            HttpClient httpClient = TranslateHttpClient.Instance;
            HttpRequestMessage httpRequestMessage = new();
            httpRequestMessage.RequestUri = new Uri($"https://{ENDPOINT}");
            httpRequestMessage.Method = HttpMethod.Post;
            foreach (KeyValuePair<string, string> kv in headers)
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
            }
            httpRequestMessage.Content = new StringContent(requestPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage httpResponseMessage;
            try
            {
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
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

            string responseJson = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                JsonNode? jsonNode = JsonSerializer.Deserialize<JsonNode>(responseJson);
                string? result = jsonNode?["Response"]?["TargetText"]?.GetValue<string>();
                if (result == null)
                {
                    errorInfo = "Code: "
                        + jsonNode!["Response"]!["Error"]!["Code"]!.GetValue<string>()
                        + Environment.NewLine
                        + "ErrorMessage: "
                        + jsonNode["Response"]!["Error"]!["Message"]!.GetValue<string>();
                    return null;
                }
                else
                {
                    return result;
                }
            }
            catch (JsonException ex)
            {
                errorInfo = ex.Message;
                return null;
            }
        }

        private string GetLanguageCode(CultureInfo cultureInfo)
        {
            return TencentOldLanguageCodeConverter.GetLanguageCode(cultureInfo);
        }

        public TencentOldTranslator(string displayName, string secretId, string secretKey)
        {
            TranslatorDisplayName = displayName;
            SecretId = secretId;
            SecretKey =  secretKey;
        }


        /// <summary>
        /// 腾讯旧版翻译API申请地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_API()
        {
            return "https://cloud.tencent.com/product/tmt";
        }

        /// <summary>
        /// 腾讯旧版翻译API额度查询地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Bill()
        {
            return "https://console.cloud.tencent.com/tmt";
        }

        /// <summary>
        /// 腾讯旧版翻译API文档地址（错误代码）
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Doc()
        {
            return "https://cloud.tencent.com/document/api/551/15619";
        }
    }

}
