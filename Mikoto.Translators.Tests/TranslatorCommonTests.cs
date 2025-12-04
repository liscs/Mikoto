using Mikoto.Config;
using Mikoto.Core;
using Mikoto.Translators.Implementations;
using Moq;
using Xunit;

namespace Mikoto.Translators.Tests
{
    public class TranslatorCommonTests
    {
        [Fact]
        public void Refresh_ShouldPopulateDisplayNameTranslatorNameDict()
        {
            // Arrange
            var mockResourceService = new Mock<IResourceService>();
            mockResourceService.Setup(r => r.Get(nameof(BaiduTranslator))).Returns("百度翻译");
            mockResourceService.Setup(r => r.Get(nameof(GoogleCloudTranslator))).Returns("谷歌翻译");

            // Act
            TranslatorCommon.Refresh(mockResourceService.Object);

            // Assert
            Assert.Equal(nameof(BaiduTranslator), TranslatorCommon.DisplayNameTranslatorNameDict["百度翻译"]);
            Assert.Equal(nameof(GoogleCloudTranslator), TranslatorCommon.DisplayNameTranslatorNameDict["谷歌翻译"]);
        }

        [Fact]
        public void GetTranslator_ShouldReturnTranslatorInstance()
        {
            // Arrange
            var mockAppSettings = new Mock<IAppSettings>();
            var translatorName = nameof(GoogleCloudTranslator);
            var displayName = "谷歌翻译";

            // Act
            var translator = TranslatorCommon.GetTranslator(translatorName, mockAppSettings.Object, displayName);

            // Assert
            Assert.NotNull(translator);
            Assert.Equal(displayName, translator.TranslatorDisplayName);
        }
    }
}
