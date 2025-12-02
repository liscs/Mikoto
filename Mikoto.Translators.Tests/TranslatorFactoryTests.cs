using Mikoto.Config;
using Mikoto.Translators.Implementations;

namespace Mikoto.Translators.Tests;


using Mikoto.Translators;
using Mikoto.Translators.Interfaces;
using Moq;
// TranslatorFactoryTests.cs
using Xunit;


public class TranslatorFactoryTests
{
    private IAppSettings GetMockedAppSettings()
    {
        var mockSettings = new Mock<IAppSettings>();

        mockSettings.SetupGet(s => s.BDappID).Returns("MockBaiduId");
        mockSettings.SetupGet(s => s.BDsecretKey).Returns("MockBaiduSecret");

        mockSettings.SetupGet(s => s.DeepLsecretKey).Returns("MockDeepLKey");

        return mockSettings.Object;
    }

    [Fact]
    public void GetTranslator_Baidu_ShouldReturnInstanceWithCorrectConfig()
    {
        // Arrange
        var factory = new TranslatorFactory();
        var mockSettings = GetMockedAppSettings();
        string name = nameof(BaiduTranslator);
        string displayName = "百度翻译";

        // Act
        ITranslator translator = factory.GetTranslator(name, mockSettings, displayName);

        // Assert
        Assert.NotNull(translator);
        Assert.IsType<BaiduTranslator>(translator);
    }

    [Fact]
    public void GetTranslator_DeepL_ShouldReturnDeepLInstance()
    {
        // Arrange
        var factory = new TranslatorFactory();
        var mockSettings = GetMockedAppSettings();
        string name = nameof(DeepLTranslator);
        string displayName = "DeepL翻译";

        // Act
        ITranslator translator = factory.GetTranslator(name, mockSettings, displayName);

        // Assert
        Assert.NotNull(translator);
        Assert.IsType<DeepLTranslator>(translator);
    }

    [Fact]
    public void GetTranslator_UnknownName_ShouldReturnNull()
    {
        // Arrange
        var factory = new TranslatorFactory();
        var mockSettings = GetMockedAppSettings();

        // Act
        ITranslator translator = factory.GetTranslator("NonExistentTranslator", mockSettings, "不存在");

        // Assert
        Assert.Null(translator);
    }
}
