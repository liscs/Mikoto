using Mikoto.Translators.Implementations;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Mikoto.Translators.Tests.Implementations;

public class GoogleCloudTranslatorTests
{
    private const string TestApiKey = "TEST_API_KEY";
    private const string SourceLang = "ja-JP"; // 传入的 CultureInfo 字符串
    private const string TargetLang = "zh-CN";

    // 辅助方法：创建模拟的 HttpMessageHandler
    private HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string responseContent)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent)
            });

        return new HttpClient(mockHandler.Object);
    }

    [Fact]
    public async Task TranslateAsync_Success_ReturnsTranslatedText()
    {
        // Arrange
        const string mockTranslatedText = "感谢您使用这款软件！";
        // 模拟一个成功的 Google API 响应
        var successResponseJson = new GoogleTranslateResponse
        {
            Data = new GoogleTranslationData
            {
                Translations = new List<GoogleTranslation>
                {
                    new GoogleTranslation { TranslatedText = mockTranslatedText }
                }
            }
        };
        string responseContent = JsonSerializer.Serialize(successResponseJson);

        var mockHttpClient = CreateMockHttpClient(HttpStatusCode.OK, responseContent);

        var translator = new GoogleCloudTranslator("GoogleTest", TestApiKey, mockHttpClient);

        // Act
        var result = await translator.TranslateAsync("このソフトを使ってくれてありがとう！", TargetLang, SourceLang);

        // Assert
        Assert.Equal(mockTranslatedText, result);
        Assert.Equal(string.Empty, translator.GetLastError());
    }

    [Fact]
    public async Task TranslateAsync_400BadRequest_ReturnsDetailedError()
    {
        // Arrange
        const string expectedViolation = "Source language: jp";

        // 模拟一个 400 错误响应 (Source language: jp)
        var errorResponseJson = new GoogleApiErrorResponse
        {
            Error = new GoogleApiError
            {
                Code = 400,
                Message = "Invalid Value",
                Details = new List<GoogleApiErrorDetail>
                {
                    new GoogleApiErrorDetail
                    {
                        FieldViolations = new List<FieldViolation>
                        {
                            new FieldViolation { Field = "source", Description = expectedViolation }
                        }
                    }
                }
            }
        };
        string errorContent = JsonSerializer.Serialize(errorResponseJson);

        var mockHttpClient = CreateMockHttpClient(HttpStatusCode.BadRequest, errorContent);
        var translator = new GoogleCloudTranslator("GoogleTest", TestApiKey, mockHttpClient);

        // Act
        var result = await translator.TranslateAsync("Text to translate", TargetLang, SourceLang);

        // Assert
        Assert.Null(result);

        // 检查 GetLastError 是否包含详细的解析信息
        var lastError = translator.GetLastError();
        Assert.Contains("HTTP 400 (BadRequest) - Invalid Value", lastError);
        Assert.Contains($"Field: source, Reason: {expectedViolation}", lastError);
    }

    [Theory]
    [InlineData("", "en", "zh", "Param Missing")]
    [InlineData("test", "", "zh", "Param Missing")]
    [InlineData("test", "en", "", "Param Missing")]
    public async Task TranslateAsync_MissingParams_ReturnsParamMissingError(string text, string des, string src, string expectedError)
    {
        // Arrange
        // 无需模拟 HttpClient
        var mockHttpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");
        var translator = new GoogleCloudTranslator("GoogleTest", TestApiKey, mockHttpClient);

        // Act
        var result = await translator.TranslateAsync(text, des, src);

        // Assert
        Assert.Null(result);
        Assert.Equal(expectedError, translator.GetLastError());
    }
}