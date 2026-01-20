using Mikoto.Translators.Interfaces;
using Serilog;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

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

        var requestPayload = GetRequestPayload(sourceText, desLang, srcLang);

        string jsonParam = requestPayload.ToJsonString(TranslatorJsonContext.AotSafeContext.Options);

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
            if (JsonSerializer.Deserialize<ChatResponse>(respString, TranslatorJsonContext.AotSafeContext.ChatResponse) is { } info)
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

    private JsonObject GetRequestPayload(string sourceText, string desLang, string srcLang, bool streamMode = false)
    {
        return new JsonObject
        {
            ["model"] = openai_model,
            ["messages"] = new JsonArray
        {
            new JsonObject
            {
                ["role"] = "system",
                ["content"] = $"You are a professional translation engine. Translate from {srcLang} to {desLang}."
            },
            new JsonObject { ["role"] = "user", ["content"] = sourceText }
        },
            ["stream"] = streamMode
        };
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
            ChatResErr errObj = JsonSerializer.Deserialize<ChatResErr>(errorContent, TranslatorJsonContext.AotSafeContext.ChatResErr);
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

    public bool IsStreamSupported => true;
    public async IAsyncEnumerable<string> StreamTranslateAsync(
    string sourceText,
    string desLang,
    string srcLang,
    [EnumeratorCancellation] CancellationToken token = default)
    {
        var payload = GetRequestPayload(sourceText, desLang, srcLang, true);

        using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new("Bearer", apiKey);
        // 使用 ResponseHeadersRead 避免预加载整个响应体到内存
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

        // 抛出非成功状态码异常（如 401, 500 等）
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(token);
        // 显式指定枚举器的读取行为
        using var reader = new StreamReader(stream);

        while (!token.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(token);

            // 如果 line 为 null，说明流已经关闭（到达末尾）
            if (line == null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("data: "))
            {
                var data = line[6..].Trim();

                if (data == "[DONE]")
                {
                    yield break;
                }

                yield return GetContentFromStreamResponse(data);
            }
        }
    }

    private static string GetContentFromStreamResponse(string data)
    {
        try
        {
            // 使用 JsonDocument 解析每一行 data 字符串
            using var jsonDoc = JsonDocument.Parse(data);

            // 按照路径 choices[0] -> delta -> content 提取文本
            if (jsonDoc.RootElement.TryGetProperty("choices", out var choices) &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("delta", out var delta) &&
                delta.TryGetProperty("content", out var content))
            {
                return content.GetString()??string.Empty;
            }
        }
        catch (JsonException ex)
        {
            Log.Warning(ex, "解析流响应json失败");
            return string.Empty;
        }

        return string.Empty;
    }
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
