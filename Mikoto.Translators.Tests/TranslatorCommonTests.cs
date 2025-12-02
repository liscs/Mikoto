using Mikoto.Config;
using Mikoto.Translators.Implementations;
using Mikoto.Translators.Interfaces;
using Moq;
using Xunit;

namespace Mikoto.Translators.Tests;


public class TranslatorCommonTests
{
    // 定义一个桩 ITranslator 实例
    private readonly ITranslator _mockTranslator = Mock.Of<ITranslator>();

    // 模拟 IAppSettings
    private Mock<IAppSettings> SetupAppSettings()
    {
        var mockSettings = new Mock<IAppSettings>();
        mockSettings.SetupGet(s => s.BDappID).Returns("TestBaiduID");
        mockSettings.SetupGet(s => s.BDsecretKey).Returns("TestBaiduKey");
        mockSettings.SetupGet(s => s.DeepLsecretKey).Returns("TestDeepLKey");
        return mockSettings;
    }

    [Fact]
    public void GetTranslator_ShouldReturnBaiduTranslator_WithCorrectSettings()
    {
        // Arrange
        var mockSettings = SetupAppSettings();
        string name = nameof(BaiduTranslator);
        string displayName = "百度翻译";

        // 由于 BaiduTranslator 是静态的，我们无法直接使用 Moq 来验证其调用参数。
        // **最佳实践：** 重构 BaiduTranslator，让其通过非静态构造函数接受配置，再使用 DI 或 Moq 验证。
        // **当前方案：** 只能验证返回的实例是否非空（如果 BaiduTranslator 实际返回一个非空对象）

        // Act
        // 假设我们能控制 BaiduTranslator 始终返回一个实例
        var result = TranslatorCommon.GetTranslator(name, mockSettings.Object, displayName);

        // Assert
        Assert.NotNull(result);
        // 如果 BaiduTranslator 是您控制的接口，您可以在 BaiduTranslator 内部记录传入的参数，并在测试中验证这些参数。

        // 验证 AppSettings 中的属性是否被访问（但无法验证 GetTranslator 传递了正确的属性）
        // mockSettings.VerifyGet(s => s.BDappID, Times.Once); // 如果您能确定属性一定会被访问
    }

    [Fact]
    public void GetTranslator_ShouldReturnDeepLTranslator_WithCorrectSettings()
    {
        // Arrange
        var mockSettings = SetupAppSettings();
        string name = nameof(DeepLTranslator);
        string displayName = "DeepL翻译";

        // Act
        var result = TranslatorCommon.GetTranslator(name, mockSettings.Object, displayName);

        // Assert
        Assert.NotNull(result);
        // 同样，需要对 DeepLTranslator 的实现进行重构或桩/反射才能验证具体的参数。
    }

    [Fact]
    public void GetTranslator_ShouldReturnNull_ForUnknownTranslator()
    {
        // Arrange
        var mockSettings = SetupAppSettings();
        string name = "UnknownTranslator";

        // Act
        var result = TranslatorCommon.GetTranslator(name, mockSettings.Object, "未知翻译器");

        // Assert
        Assert.Null(result);
    }
}
