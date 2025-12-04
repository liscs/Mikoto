using Mikoto.Translators.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mikoto.Translators.Implementations
{
    public class GoogleTranslateRequest
    {
        [JsonPropertyName("q")]
        public List<string> Queries { get; set; } = new List<string>();

        [JsonPropertyName("target")]
        public string TargetLanguage { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string SourceLanguage { get; set; } = string.Empty;

        [JsonPropertyName("format")]
        public string Format { get; set; } = "text";
    }

    public class GoogleTranslation
    {
        [JsonPropertyName("translatedText")]
        public string? TranslatedText { get; set; }
    }

    public class GoogleTranslationData
    {
        [JsonPropertyName("translations")]
        public List<GoogleTranslation> Translations { get; set; } = new List<GoogleTranslation>();
    }

    public class GoogleTranslateResponse
    {
        [JsonPropertyName("data")]
        public GoogleTranslationData? Data { get; set; }
    }

    public class GoogleApiErrorResponse
    {
        [JsonPropertyName("error")]
        public GoogleApiError? Error { get; set; }
    }

    public class GoogleApiError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("details")]
        public List<GoogleApiErrorDetail>? Details { get; set; }
    }

    public class GoogleApiErrorDetail
    {
        [JsonPropertyName("@type")]
        public string? Type { get; set; }

        [JsonPropertyName("fieldViolations")]
        public List<FieldViolation>? FieldViolations { get; set; }
    }

    public class FieldViolation
    {
        [JsonPropertyName("field")]
        public string? Field { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }



    public class GoogleCloudTranslator : ITranslator
    {
        public string? _apiKey;
        private string errorInfo = string.Empty;

        private readonly HttpClient _httpClient;

        // V2 RESTful API 地址
        private const string ApiBaseUrl = "https://translation.googleapis.com/language/translate/v2";

        public string TranslatorDisplayName { get; private set; }

        public GoogleCloudTranslator(string displayName, string apiKey, HttpClient httpClient)
        {
            TranslatorDisplayName = displayName;
            _apiKey = apiKey;
            _httpClient = httpClient;
        }

        public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            if (string.IsNullOrEmpty(sourceText) || string.IsNullOrEmpty(desLang) || string.IsNullOrEmpty(srcLang))
            {
                errorInfo = "Param Missing";
                return null;
            }

            string requestUrl = $"{ApiBaseUrl}?key={_apiKey}";

            var requestBody = new GoogleTranslateRequest
            {
                Queries = new List<string> { sourceText },
                TargetLanguage = desLang,
                SourceLanguage = srcLang
            };

            string jsonContent = JsonSerializer.Serialize(requestBody, TranslatorCommon.JsonSerializerOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            string retString;
            HttpResponseMessage response;

            try
            {
                response = await _httpClient.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    retString = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<GoogleApiErrorResponse>(errorDetails);
                        if (errorResponse?.Error != null)
                        {
                            string primaryMessage = $"HTTP {errorResponse.Error.Code} ({response.StatusCode}) - {errorResponse.Error.Message}";
                            var violations = errorResponse.Error.Details?
                                                .SelectMany(d => d.FieldViolations ?? new List<FieldViolation>())
                                                .ToList();

                            if (violations != null && violations.Any())
                            {
                                string violationDetail = string.Join("; ", violations
                                    .Select(v => $"Field: {v.Field}, Reason: {v.Description}"));

                                errorInfo = $"{primaryMessage}. Violations: {violationDetail}";
                            }
                            else
                            {
                                errorInfo = primaryMessage;
                            }
                        }
                        else
                        {
                            errorInfo = $"HTTP Error {response.StatusCode}. Raw Response: {errorDetails}";
                        }
                    }
                    catch (JsonException)
                    {
                        errorInfo = $"HTTP Error {response.StatusCode}. Failed to parse error JSON. Raw: {errorDetails}";
                    }

                    return null; // 错误处理完成，返回 null
                }
            }
            catch (HttpRequestException ex)
            {
                errorInfo = $"HTTP Request Error: {ex.Message}";
                return null;
            }
            catch (TaskCanceledException ex)
            {
                errorInfo = $"Request Timeout/Canceled: {ex.Message}";
                return null;
            }

            GoogleTranslateResponse? gResponse;
            try
            {
                gResponse = JsonSerializer.Deserialize<GoogleTranslateResponse>(retString);
            }
            catch (JsonException ex)
            {
                errorInfo = $"JSON Parse Error: {ex.Message}. Raw: {retString}";
                return null;
            }

            var translations = gResponse?.Data?.Translations;

            if (translations != null && translations.Any())
            {
                return translations.First().TranslatedText;
            }
            else
            {
                errorInfo = "Google API returned success status, but no translation data found.";
                return null;
            }
        }

        public string GetLastError()
        {
            return errorInfo;
        }

        public static string GetUrl_API() => "https://console.cloud.google.com/marketplace/product/google/translate.googleapis.com";
        public static string GetUrl_Bill() => "https://console.cloud.google.com/billing";
        public static string GetUrl_Doc() => "https://docs.cloud.google.com/translate/docs/reference/rest/v2/translate";

    }
}