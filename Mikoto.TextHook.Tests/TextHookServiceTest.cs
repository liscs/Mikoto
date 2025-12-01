using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Reflection;
using Mikoto.TextHook;
// 假设 TextHookData 和其他依赖（如 EditDistance）是可访问的

namespace Mikoto.TextHook.Tests
{
    [TestClass]
    public class TextHookServiceTest
    {

        [TestMethod]
        public void AddHistory_ShouldKeepMax1000()
        {
            var svc = new TextHookService();

            for (int i = 0; i < 1200; i++)
                svc.AddTextractorHistory("test");

            // 在 MSTest 中，推荐使用 Assert.AreEqual(expected, actual)
            Assert.HasCount(1000, svc.TextractorOutPutHistory, "历史记录队列大小应限制在 1000。");
        }

        [TestMethod]
        public void GetHookAddressByMisakaCode_ShouldReturnNull_OnInvalidFormat()
        {
            // Arrange
            var svc = new TextHookService();
            const string misakaCode = "【401000_00000001_00000002】"; // 使用下划线而不是冒号

            // Act
            string? address = svc.GetHookAddressByMisakaCode(misakaCode);

            // Assert
            Assert.IsNull(address, "格式不正确时，应返回 null。");
        }

        [TestMethod]
        public void GetHookAddressByMisakaCode_ShouldExtractAddress()
        {
            // Arrange
            var svc = new TextHookService();
            const string misakaCode = "【401000:00000001:00000002】";

            // Act
            string? address = svc.GetHookAddressByMisakaCode(misakaCode);

            // Assert
            Assert.AreEqual("401000", address, "应正确提取 MisakaCode 中的 HookAddress 部分。");
        }
    }
}