using Mikoto.Translators.Interfaces;
using System.Text;
using System.Text.Json;

// ChatGPT translator integration
// Original Author: bychv, Modified by Mikoto
// API version: v1


namespace Mikoto.Translators.Implementations;

public class ChatGPTTranslator : ITranslator
{
    public static readonly string SIGN_UP_URL = "https://platform.openai.com";
    public static readonly string BILL_URL = "https://platform.openai.com/account/usage";
    public static readonly string DOCUMENT_URL = "https://platform.openai.com/docs/introduction/overview";

    private readonly string openai_model = "gpt-4o-mini";

    private readonly HttpClient _httpClient;
    private readonly string apiKey;
    private readonly string apiUrl;
    private string lastErrorMessage = string.Empty;

    public string DisplayName { get; private set; }

    public string GetLastError() => lastErrorMessage;

    public ChatGPTTranslator(string displayName, string apiKey, string url, string model, HttpClient instance)
    {
        DisplayName = displayName;
        this.apiKey = apiKey;
        apiUrl = url;
        openai_model = model;
        _httpClient = instance;
    }

    public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
    {
        // 参数校验
        if (string.IsNullOrWhiteSpace(desLang) ||
            string.IsNullOrWhiteSpace(srcLang))
        {
            SetError("Missing required parameters.");
            return null;
        }

        // 空文本：直接返回空文本，不做请求
        if (sourceText is null)
            return null;
        if (sourceText.Length == 0)
            return string.Empty;

        var requestPayload = new
        {
            model = openai_model,
            messages = new[]
            {
                new {
                    role = "system",
                    content = $"You are a professional multilingual translation engine. " +
                              $"Translate everything from {srcLang} to {desLang}. " +
                              $"Preserve meaning, tone, punctuation, spacing, markdown and numbers. " +
                              $"Do NOT explain, do NOT add comments."
                },
                new { role = "user", content = sourceText }
            }
        };

        string jsonParam = JsonSerializer.Serialize(requestPayload, TranslatorCommon.JsonSerializerOptions);

        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            req.Content = new StringContent(jsonParam, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await _httpClient.SendAsync(req);

            string respString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                HandleApiError(respString);
                return null;
            }

            // 解析正常返回
            if (JsonSerializer.Deserialize<ChatResponse>(respString, TranslatorCommon.JsonSerializerOptions) is { } info)
            {
                var msg = info.choices?.FirstOrDefault().message.content;
                if (!string.IsNullOrEmpty(msg))
                    return msg;

                return HandleUnknownError(respString);
            }

            return HandleUnknownError(respString);
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

    private void SetError(string message) => lastErrorMessage = message;

    private string? HandleUnknownError(string resp)
    {
        SetError($"Unknown API response: {resp}");
        return null;
    }

    private void HandleApiError(string errorContent)
    {
        try
        {
            ChatResErr errObj = JsonSerializer.Deserialize<ChatResErr>(errorContent, TranslatorCommon.JsonSerializerOptions);
            if (!string.IsNullOrWhiteSpace(errObj.error.message))
            {
                SetError(errObj.error.message);
                return;
            }

            HandleUnknownError(errorContent);
        }
        catch
        {
            HandleUnknownError(errorContent);
        }
    }

    public static string GetUrl_Bill() => BILL_URL;
    public static string GetUrl_Doc() => DOCUMENT_URL;
    public static string GetUrl_API() => SIGN_UP_URL;
}

public struct ChatResponse
{
    public string id { get; set; }
    public string @object { get; set; }
    public int created { get; set; }
    public string model { get; set; }
    public ChatUsage usage { get; set; }
    public ChatChoice[] choices { get; set; }
}

public struct ChatUsage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}

public struct ChatChoice
{
    public int index { get; set; }
    public ChatMessage message { get; set; }
    public string finish_reason { get; set; }
}

public struct ChatMessage
{
    public string role { get; set; }
    public string content { get; set; }
}

public struct ChatResErr
{
    public ChatError error { get; set; }
}

public struct ChatError
{
    public string message { get; set; }
    public string type { get; set; }
    public object param { get; set; }
    public object code { get; set; }
}
