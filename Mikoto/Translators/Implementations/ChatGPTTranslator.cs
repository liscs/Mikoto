using Mikoto.Helpers.Network;
using Mikoto.Translators.Interfaces;
using System.Net.Http;
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
        private string? apiKey; //ChatGPT翻译API的密钥
        private string? apiUrl; //ChatGPT翻译API的URL
        private string errorInfo = string.Empty; //错误信息

        public string TranslatorDisplayName { get { return Application.Current.Resources["ChatGPTTranslator"].ToString()!; } }

        public string GetLastError()
        {
            return errorInfo;
        }

        public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            string q = sourceText;

            if (sourceText == "" || desLang == "" || srcLang == "")
            {
                errorInfo = "Param Missing";
                return null;
            }
            string retString;
            string jsonParam = $"{{\"model\": \"{openai_model}\",\"messages\": [{{\"role\": \"system\", \"content\": \"Translate {srcLang} To {desLang}\"}},{{\"role\": \"user\", \"content\": \"{q}\"}}]}}";
            var hc = CommonHttpClient.Instance;
            var req = new StringContent(jsonParam, null, "application/json");
            hc.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            try
            {
                HttpResponseMessage? httpResponseMessage = await hc.PostAsync(apiUrl, req);
                retString = await httpResponseMessage.Content.ReadAsStringAsync();
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
            finally
            {
                req.Dispose();
            }

            ChatResponse oinfo;

            try
            {
                oinfo = JsonSerializer.Deserialize<ChatResponse>(retString, TranslatorCommon.JsonSerializerOptions);
            }
            catch
            {
                errorInfo = "JsonConvert Error";
                return null;
            }

            try
            {
                return oinfo.choices[0].message.content;
            }
            catch
            {
                try
                {
                    var err = JsonSerializer.Deserialize<ChatResErr>(retString, TranslatorCommon.JsonSerializerOptions);
                    errorInfo = err.error.message;
                    return null;
                }
                catch
                {
                    errorInfo = "Unknown error";
                    return null;
                }
            }
        }

        public static ITranslator TranslatorInit(params string[] param)
        {
            if (param.Length < 3)
                throw new ArgumentException("Expected 3 parameters: API Key, API URL, Model");


            ChatGPTTranslator chatGPTTranslator = new()
            {
                apiKey = param[0],
                apiUrl = param[1],
                openai_model = param[2],
            };
            return chatGPTTranslator;
        }
    }

#pragma warning disable 0649
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
