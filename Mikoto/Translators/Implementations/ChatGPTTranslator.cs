using Mikoto.Helpers.Network;
using Mikoto.Translators.Interfaces;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

/*
 * ChatGPT translator integration
 * Author: bychv
 * API version: v1
 */
namespace Mikoto.Translators.Implementations
{
    public class ChatGPTTranslator : ITranslator
    {
        private ChatGPTTranslator() { }
        public static readonly string SIGN_UP_URL = "https://platform.openai.com";
        public static readonly string BILL_URL = "https://platform.openai.com/account/usage";
        public static readonly string DOCUMENT_URL = "https://platform.openai.com/docs/introduction/overview";

        private string? openai_model = "gpt-3.5-turbo";
        private string? apiKey; // ChatGPT 翻译 API 的密钥
        private string? apiUrl; // ChatGPT 翻译 API 的 URL
        private string lastErrorMessage = string.Empty; 

        public string TranslatorDisplayName => Application.Current.Resources["ChatGPTTranslator"]?.ToString() ?? "ChatGPT Translator";

        public string GetLastError()
        {
            return lastErrorMessage;
        }

        public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            // 参数校验
            if (string.IsNullOrWhiteSpace(sourceText) || string.IsNullOrWhiteSpace(desLang) || string.IsNullOrWhiteSpace(srcLang))
            {
                SetError("Missing required parameters.");
                return null;
            }

            var requestPayload = new
            {
                model = openai_model,
                messages = new[]
                {
                    new { role = "system", content = $"Translate {srcLang} To {desLang}" },
                    new { role = "user", content = sourceText }
                }
            };

            string jsonParam = JsonSerializer.Serialize(requestPayload, TranslatorCommon.JsonSerializerOptions);
            var hc = CommonHttpClient.Instance;
            var req = new StringContent(jsonParam, Encoding.UTF8, "application/json");
            hc.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            try
            {
                using HttpResponseMessage httpResponseMessage = await hc.PostAsync(apiUrl, req);

                // 检查 HTTP 状态码
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    string errorContent = await httpResponseMessage.Content.ReadAsStringAsync();
                    HandleApiError(errorContent);
                    return null;
                }

                string responseString = await httpResponseMessage.Content.ReadAsStringAsync();

                // 解析 API 响应
                if (JsonSerializer.Deserialize<ChatResponse>(responseString, TranslatorCommon.JsonSerializerOptions) is { } oinfo)
                {
                    return oinfo.choices.FirstOrDefault().message.content ?? HandleUnknownError(responseString);
                }

                return HandleUnknownError(responseString);
            }
            catch (HttpRequestException ex)
            {
                SetError($"Network error: {ex.Message}");
                return null;
            }
            catch (TaskCanceledException ex)
            {
                SetError($"Request timeout: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                SetError($"Unexpected error: {ex.Message}");
                return null;
            }
        }

        public static ITranslator TranslatorInit(params string[] param)
        {
            if (param.Length < 3)
                throw new ArgumentException("Expected 3 parameters: API Key, API URL, Model");

            return new ChatGPTTranslator
            {
                apiKey = param[0],
                apiUrl = param[1],
                openai_model = param[2],
            };
        }

        private void SetError(string message)
        {
            lastErrorMessage = message;
        }

        private string? HandleUnknownError(string response)
        {
            SetError($"Unknown API response: {response}");
            return null;
        }

        private void HandleApiError(string errorContent)
        {
            try
            {
                if (JsonSerializer.Deserialize<ChatResErr>(errorContent, TranslatorCommon.JsonSerializerOptions) is ChatResErr err && err.error.code != null)
                {
                    SetError(err.error.message);
                }
                else
                {
                    HandleUnknownError(errorContent);
                }
            }
            catch
            {
                HandleUnknownError(errorContent);
            }
        }
    }

    public struct ChatResponse
    {
        public string id;
        public string _object;
        public int created;
        public string model;
        public ChatUsage usage;
        public ChatChoice[] choices;
    }

    public struct ChatUsage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }

    public struct ChatChoice
    {
        public ChatMessage message;
        public string finish_reason;
        public int index;
    }

    public struct ChatMessage
    {
        public string role;
        public string content;
    }

    public struct ChatResErr
    {
        public ChatError error;
    }

    public struct ChatError
    {
        public string message;
        public string type;
        public object param;
        public object code;
    }
}

