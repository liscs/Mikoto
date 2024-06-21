//参考 https://github.com/Dark-20001/volcengine-sdk-c-

using Mikoto.Helpers.Exceptions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;

namespace Mikoto.Translators
{
    public class VolcanoTranslator : ITranslator
    {
        private VolcanoTranslator() { }
        private string? _apiKey;
        private string? _apiSecret;

        private string errorInfo = string.Empty;

        public string TranslatorDisplayName { get { return Application.Current.Resources["VolcanoTranslator"].ToString()!; } }

        public string GetLastError()
        {
            return errorInfo;
        }

        private const string HOST = "open.volcengineapi.com";
        private const string URL = "HTTP://" + HOST + "/?" + URL_QUERY;
        private const string SERVICE = "translate";
        private const string REGION = "cn-north-1";
        private const string URL_QUERY = "Action=TranslateText&Version=2020-06-01";
        private const string ALGORITHM = "HMAC-SHA256";
        private readonly List<string> H_INCLUDE = ["Content-Type", "Content-Md5", "Host"];



        public async Task<string?> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
        {
            string?[] paramStrs = [text, sourceLanguage, targetLanguage, _apiKey, _apiSecret];
            if (paramStrs.Any(string.IsNullOrEmpty))
            {
                errorInfo = "Param Missing";
                return null;
            }
            string requestBody = BuildRequestJson(text, sourceLanguage, targetLanguage);
            string? translated = await SendRequestAsync(requestBody);
            return translated;
        }

        protected static string ComputeHash256(string input)
        {
            byte[] hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return ToHexString(hashedBytes);
        }

        /*
         * Example
        {
            "SourceLanguage": "en"
            "TargetLanguage": "zh",
            "TextList": [
                "Hello world"
            ]
        }
        */

        private static string BuildRequestJson(string text, string targetLanguage, string sourceLanguage)
        {
            VolcanoRequest volcanoRequest = new VolcanoRequest(targetLanguage, [text])
            {
                SourceLanguage = sourceLanguage,
            };
            return JsonSerializer.Serialize(volcanoRequest);
        }

        private static string BuildRequestJson(string[] text, string sourceLanguage, string TargetLanguage)
        {
            VolcanoRequest volcanoRequest = new VolcanoRequest(TargetLanguage, text)
            {
                SourceLanguage = sourceLanguage,
            };
            return JsonSerializer.Serialize(volcanoRequest);
        }


        /*
         {
            "TranslationList": [
                {
                    "Translation": "你好世界",
                    "DetectedSourceLanguage": "en"
                }
            ],
            "ResponseMetadata": {
                "RequestId": "202004092306480100140440781F5D7119",
                "Action": "TranslateText",
                "Version": "2020-06-01",
                "Service": "translate",
                "Region": "cn-north-1",
                "Error": null
            }
        } 
         *
         */

        private async Task<string?> SendRequestAsync(string requestBody)
        {
            DateTime dateTimeSign = DateTime.UtcNow;
            string nowDate = dateTimeSign.ToString("yyyyMMdd");
            string nowTime = dateTimeSign.ToString("HHmmss");
            string dateTimeSignStr = nowDate + "T" + nowTime + "Z";

            HttpClient httpClient = TranslatorCommon.HttpClientInstance;
            using HttpRequestMessage httpRequestMessage = new();
            httpRequestMessage.RequestUri = new Uri(URL);
            httpRequestMessage.Method = HttpMethod.Post;
            httpRequestMessage.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            httpRequestMessage.Headers.Add("X-Date", dateTimeSignStr);
            string bodyHash = ComputeHash256(requestBody);
            httpRequestMessage.Headers.Add("X-Content-Sha256", bodyHash);
            string AuthHeader = GetAuthHeader(nowDate, dateTimeSignStr, httpRequestMessage.Headers, bodyHash);
            httpRequestMessage.Headers.TryAddWithoutValidation("Authorization", AuthHeader);
            HttpResponseMessage httpResponseMessage;
            try
            {
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            }
            catch (HttpRequestException ex)
            {
                errorInfo = ex.GetOriginalException().Message;
                return null;
            }
            catch (TaskCanceledException ex)
            {
                errorInfo = ex.GetOriginalException().Message;
                return null;
            }

            string responseJson = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                JsonNode? jsonNode = JsonSerializer.Deserialize<JsonNode>(responseJson);
                string? result = jsonNode?["TranslationList"]?[0]?["Translation"]?.GetValue<string>();
                if (result == null)
                {
                    errorInfo = "ErrorCodeN: " + jsonNode!["ResponseMetadata"]!["Error"]!["CodeN"]!.GetValue<int>() + Environment.NewLine
                    + "ErrorCode: " + jsonNode["ResponseMetadata"]!["Error"]!["Code"]!.GetValue<string>() + Environment.NewLine
                    + "ErrorMessage: " + jsonNode["ResponseMetadata"]!["Error"]!["Message"]!.GetValue<string>();
                    return null;
                }
                else
                {
                    return result;
                }
            }
            catch (JsonException ex)
            {
                errorInfo = ex.GetOriginalException().Message;
                return null;
            }
        }


        private string GetAuthHeader(string nowDate, string dateTimeSignStr, HttpRequestHeaders headers, string bodyHash)
        {
            List<string> signedHeaders = new List<string>();

            foreach (var item in headers)
            {
                string headerName = item.Key;
                if (H_INCLUDE.Contains(headerName) || headerName.StartsWith("X-"))
                {
                    signedHeaders.Add(headerName.ToLower());
                }
            }
            signedHeaders.Add("host");
            signedHeaders.Sort();
            StringBuilder signedHeadersToSignStr = new();

            string headerValue;
            foreach (string signedHeader in signedHeaders)
            {
                if (signedHeader.Equals("host"))
                {
                    headerValue = HOST;
                }
                else
                {
                    headerValue = headers.GetValues(signedHeader).First().Trim();
                }

                signedHeadersToSignStr.Append(signedHeader).Append(':').Append(headerValue).Append('\n');
            }

            string signedHeadersStr = string.Join(";", signedHeaders.ToArray());

            string canonicalRequest = string.Join(
                "\n",
                [
                    "POST",
                "/",
                URL_QUERY,
                signedHeadersToSignStr.ToString(),
                signedHeadersStr,
                bodyHash
                ]);
            //step 1
            string hashedCanonReq = ComputeHash256(canonicalRequest);
            //step 2
            string stringToSign = ALGORITHM + "\n" +
                                    dateTimeSignStr + "\n" +
                                    string.Join("/",
                                    [
                                        nowDate,
                                    REGION,
                                    SERVICE,
                                    "request"
                                    ]) + "\n" +
                                    hashedCanonReq
                                    ;
            //step 3
            //String secretKey, String date, String region, String service
            byte[] kDate = HMACSHA256.HashData(Encoding.UTF8.GetBytes(_apiSecret!), Encoding.UTF8.GetBytes(nowDate));
            byte[] kRegion = HMACSHA256.HashData(kDate, Encoding.UTF8.GetBytes(REGION));
            byte[] kService = HMACSHA256.HashData(kRegion, Encoding.UTF8.GetBytes(SERVICE));
            byte[] signingKey = HMACSHA256.HashData(kService, Encoding.UTF8.GetBytes("request"));
            byte[] signature = HMACSHA256.HashData(signingKey, Encoding.UTF8.GetBytes(stringToSign));

            string AuthHeader = $"{ALGORITHM} Credential={_apiKey}/{nowDate}/{REGION}/{SERVICE}/request, SignedHeaders={signedHeadersStr}, Signature={ToHexString(signature)}";
            return AuthHeader;
        }

        public static string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF "
        {
            return Convert.ToHexString(bytes).ToLower();
        }

        public static ITranslator TranslatorInit(params string[] param)
        {
            VolcanoTranslator volcanoTranslator = new();
            volcanoTranslator._apiKey = param.First();
            volcanoTranslator._apiSecret = param.Last();
            return volcanoTranslator;
        }

        public static string GetUrl_lang()
        {
            return "https://www.volcengine.com/docs/4640/35107";
        }

        public static string GetUrl_Bill()
        {
            return "https://console.volcengine.com/home";
        }

        public static string GetUrl_Doc()
        {
            return "https://www.volcengine.com/docs/4640/65080";
        }

        public static string GetUrl_API()
        {
            return "https://www.volcengine.com/";
        }
    }

    internal class VolcanoRequest
    {
        public string? SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
        public string[] TextList { get; set; }

        public VolcanoRequest(string targetLanguage, string[] textList)
        {
            TargetLanguage = targetLanguage;
            TextList = textList;
        }
    }
}
