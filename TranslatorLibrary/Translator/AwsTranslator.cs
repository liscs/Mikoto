//参考 https://stackoverflow.com/questions/50507461/aws-translatetext-rest-api-call-adding-signature-v4

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TranslatorLibrary.Translator
{
    internal class AwsTranslator : ITranslator
    {
        private string? _accessKey;
        private string? _secretKey;

        private string errorInfo = string.Empty;

        public string TranslatorDisplayName { get { return "AWS"; } }

        public string GetLastError()
        {
            return errorInfo;
        }
        public async Task<string?> TranslateAsync(string text, string targetLang, string sourceLang)
        {
            var date = DateTime.UtcNow;

            const string algorithm = "AWS4-HMAC-SHA256";
            const string regionName = "ap-northeast-1";
            const string serviceName = "translate";
            const string method = "POST";
            const string canonicalUri = "/";
            const string canonicalQueryString = "";
            const string x_amz_target_header = "AWSShineFrontendService_20170701.TranslateText";

            const string contentType = "application/x-amz-json-1.1";


            const string host = serviceName + "." + regionName + ".amazonaws.com";

            var obj = new
            {
                SourceLanguageCode = sourceLang,
                TargetLanguageCode = targetLang,
                Text = text
            };
            var requestPayload = JsonSerializer.Serialize(obj);

            var hashedRequestPayload = HexEncode(Hash(ToBytes(requestPayload)));

            var dateStamp = date.ToString("yyyyMMdd");
            var requestDate = date.ToString("yyyyMMddTHHmmss") + "Z";
            var credentialScope = $"{dateStamp}/{regionName}/{serviceName}/aws4_request";

            var bytes = ToBytes(requestPayload);


            var headers = new SortedDictionary<string, string>
            {

                     {"content-length", bytes.Length.ToString()},
                     {"content-type", contentType},
                {"host", host},
                     {"x-amz-date", requestDate},
                     {"x-amz-target", x_amz_target_header}
            };

            string canonicalHeaders =
                string.Join("\n", headers.Select(x => x.Key.ToLowerInvariant() + ":" + x.Value.Trim())) + "\n";

            string signedHeaders =
            string.Join(";", headers.Select(x => x.Key.ToLowerInvariant()));

            // Task 1: Create a Canonical Request For Signature Version 4
            var canonicalRequest = method + '\n' + canonicalUri + '\n' + canonicalQueryString +
                                   '\n' + canonicalHeaders + '\n' + signedHeaders + '\n' + hashedRequestPayload;

            var hashedCanonicalRequest = HexEncode(Hash(ToBytes(canonicalRequest)));


            // Task 2: Create a String to Sign for Signature Version 4
            // StringToSign  = Algorithm + '\n' + RequestDate + '\n' + CredentialScope + '\n' + HashedCanonicalRequest

            var stringToSign = $"{algorithm}\n{requestDate}\n{credentialScope}\n{hashedCanonicalRequest}";


            // Task 3: Calculate the AWS Signature Version 4

            // HMAC(HMAC(HMAC(HMAC("AWS4" + kSecret,"20130913"),"eu-west-1"),"tts"),"aws4_request")
            byte[] signingKey = GetSignatureKey(_secretKey!, dateStamp, regionName, serviceName);

            // signature = HexEncode(HMAC(derived-signing-key, string-to-sign))
            var signature = HexEncode(HmacSha256(stringToSign, signingKey));


            // Task 4: Prepare a signed request
            // Authorization: algorithm Credential=access key ID/credential scope, SignedHeadaers=SignedHeaders, Signature=signature

            var authorization =
                $"{algorithm} Credential={_accessKey}/{dateStamp}/{regionName}/{serviceName}/aws4_request, SignedHeaders={signedHeaders}, Signature={signature}";

            // Send the request
            string endpoint = "https://" + host; // + canonicalUri ;


            return await SendRequestAsync(bytes, endpoint, contentType, requestDate, authorization, x_amz_target_header);
        }
        private async Task<string?> SendRequestAsync(byte[] bytes, string endpoint, string contentType, string requestDate, string authorization, string x_amz_target_header)
        {
            HttpClient httpClient = new();
            using HttpRequestMessage httpRequestMessage = new();
            httpRequestMessage.RequestUri = new Uri(endpoint);
            httpRequestMessage.Method = HttpMethod.Post;
            httpRequestMessage.Content = new ByteArrayContent(bytes);
            httpRequestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            httpRequestMessage.Headers.Add("X-Amz-Date", requestDate);
            httpRequestMessage.Headers.TryAddWithoutValidation("Authorization", authorization);
            httpRequestMessage.Headers.Add("X-Amz-Target", x_amz_target_header);
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
            JsonNode? jsonNode = JsonSerializer.Deserialize<JsonNode>(responseJson);
            string? result = jsonNode?["TranslatedText"]?.GetValue<string>();
            if (result == null)
            {
                errorInfo = "__type: " + jsonNode!["__type"]!.GetValue<string>() + Environment.NewLine
                + "message: " + jsonNode["message"]!.GetValue<string>();
                return null;
            }
            else
            {
                return result;
            }
        }



        private static byte[] GetSignatureKey(String key, String dateStamp, String regionName, String serviceName)
        {
            byte[] kDate = HmacSha256(dateStamp, ToBytes("AWS4" + key));
            byte[] kRegion = HmacSha256(regionName, kDate);
            byte[] kService = HmacSha256(serviceName, kRegion);
            return HmacSha256("aws4_request", kService);
        }

        private static byte[] ToBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str.ToCharArray());
        }

        private static string HexEncode(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static byte[] Hash(byte[] bytes)
        {
            return SHA256.HashData(bytes);
        }

        private static byte[] HmacSha256(string data, byte[] key)
        {
            return HMACSHA256.HashData(key, ToBytes(data));
        }

        public static ITranslator TranslatorInit(params string[] param)
        {
            AwsTranslator awsTranslator = new();
            awsTranslator._accessKey = param.First();
            awsTranslator._secretKey = param.Last();
            return awsTranslator;
        }
    }


}
