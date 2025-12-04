using Mikoto.Core;
using Mikoto.Translators.Implementations;
using Moq;
using Xunit;

namespace Mikoto.Translators.Tests
{
    public class TranslatorCommonTests
    {
        [Fact]
        public void Refresh_ShouldClearAndRepopulateDictionariesCorrectly()
        {
            // Arrange
            var mockResourceService = new Mock<IResourceService>();

            // 模拟 IResourceService 的行为 (Act 1)
            mockResourceService.Setup(r => r.Get(nameof(BaiduTranslator))).Returns("百度翻译");
            mockResourceService.Setup(r => r.Get(nameof(CaiyunTranslator))).Returns("彩云小译");
            // 假设其他（如 DeepL）返回 null 或空字符串

            // Act 1: 第一次刷新
            TranslatorCommon.Refresh(mockResourceService.Object);

            // Assert 1: 验证初始填充
            Assert.Equal(nameof(BaiduTranslator), TranslatorCommon.DisplayNameTranslatorNameDict["百度翻译"]);
            Assert.Contains("彩云小译", TranslatorCommon.GetTranslatorDisplayNameList());


            // 验证清理和重新填充

            // 1. 清理模拟对象状态，保证状态隔离
            mockResourceService.Reset();

            // 2. 设置新的模拟行为 (Act 2)
            //    核心：让旧的名称返回空值，以验证它们被正确移除。
            mockResourceService.Setup(r => r.Get(nameof(BaiduTranslator))).Returns((string)null!); // 模拟不再有资源
            mockResourceService.Setup(r => r.Get(nameof(CaiyunTranslator))).Returns(string.Empty);   // 模拟资源为空

            //    让新的名称返回有效值
            mockResourceService.Setup(r => r.Get(nameof(DeepLTranslator))).Returns("DeepL");

            // Re-Act: 第二次刷新
            // TranslatorCommon.Refresh() 会执行 Clear()
            TranslatorCommon.Refresh(mockResourceService.Object);

            // Assert 2: 验证清理和新数据

            // 验证清理: 之前添加的名称应该被移除，因为它们现在返回 null/空字符串，不会被重新添加。
            Assert.DoesNotContain("百度翻译", TranslatorCommon.GetTranslatorDisplayNameList());
            Assert.DoesNotContain("彩云小译", TranslatorCommon.GetTranslatorDisplayNameList());

            // 验证重新填充: 新数据应该存在。
            Assert.Contains("DeepL", TranslatorCommon.GetTranslatorDisplayNameList());
            Assert.Single(TranslatorCommon.GetTranslatorDisplayNameList());
        }
    }
}
