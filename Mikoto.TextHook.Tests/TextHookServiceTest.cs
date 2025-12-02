// 假设 TextHookData 和其他依赖（如 EditDistance）是可访问的

namespace Mikoto.TextHook.Tests;

using Xunit;

public class TextHookServiceTests
{
    [Fact]
    public void AddHistory_ShouldKeepMax1000()
    {
        var svc = new TextHookService();

        for (int i = 0; i < 1200; i++)
            svc.AddTextractorHistory("test");

        Assert.Equal(1000, svc.TextractorOutPutHistory.Count);
    }

    [Fact]
    public void GetHookAddressByMisakaCode_ShouldReturnNull_OnInvalidFormat()
    {
        var svc = new TextHookService();
        const string misakaCode = "【401000_00000001_00000002】"; // 下划线，格式错误

        string? address = svc.GetHookAddressByMisakaCode(misakaCode);

        Assert.Null(address);
    }

    [Fact]
    public void GetHookAddressByMisakaCode_ShouldExtractAddress()
    {
        var svc = new TextHookService();
        const string misakaCode = "【401000:00000001:00000002】";

        string? address = svc.GetHookAddressByMisakaCode(misakaCode);

        Assert.Equal("401000", address);
    }
}
