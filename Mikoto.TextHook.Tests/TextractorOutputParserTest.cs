using Xunit;

namespace Mikoto.TextHook.Tests;

public class TextractorOutputParserTest
{
    [Fact]
    public void GetMiddleString_ShouldExtractCorrectly()
    {
        // Arrange
        const string text = "start[middle]end";

        // Act
        string? result = TextractorOutputParser.GetMiddleString(text, "[", "]", 0);

        // Assert
        Assert.Equal("middle", result);
    }

    [Fact]
    public void GetMiddleString_ShouldReturnNull_OnNotFound()
    {
        // Arrange
        const string text = "start[middle";

        // Act
        string? result = TextractorOutputParser.GetMiddleString(text, "[", "]", 0);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetMiddleString_ShouldHandleStartingLocation()
    {
        // Arrange
        const string text = "[A] [B] [C]";

        // Act
        string? result = TextractorOutputParser.GetMiddleString(text, "[", "]", 3);

        // Assert
        Assert.Equal("B", result);
    }



    [Fact]
    public void DealTextratorOutput_ShouldParseStandardOutput()
    {
        // 标准格式：[版本:PID:Hook地址:值1:值2:方法名:HookCode] 文本
        const string output = "[0:3A98:401000:00000000:00000000:TID_Eng:HB0@401000] Actual Game Text";

        // Act
        var result = TextractorOutputParser.DealTextratorOutput(output);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0x3A98, result!.GamePID);
        Assert.Equal("TID_Eng", result.HookFunc);
        Assert.Equal("HB0@401000", result.HookCode);
        Assert.Equal("401000", result.HookAddress);
        Assert.Equal("【401000:00000000:00000000】", result.MisakaHookCode);
        Assert.Equal("Actual Game Text", result.Data);
    }

    [Fact]
    public void DealTextratorOutput_ShouldHandleTextContinuation()
    {
        // Arrange
        const string firstLine = "[0:1234:401000:00000000:00000000:Func:Code] This is the first part";
        const string secondLine = " and this is the second part.";

        // Act 
        // 第一次调用：解析并设置内部的 _thData
        TextractorOutputParser.DealTextratorOutput(firstLine);
        // 第二次调用：文本被分割，应该连接到 _thData
        var result = TextractorOutputParser.DealTextratorOutput(secondLine, TextractorOutputParser.DealTextratorOutput(firstLine));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("This is the first part and this is the second part.", result!.Data);
    }

    [Fact]
    public void DealTextratorOutput_ShouldReturnNull_OnMalformedInfo()
    {
        // 缺少部分字段
        const string output = "[0:3A98:401000:00000000:TID_Eng:HB0@401000] Invalid Text";

        // Act
        var result = TextractorOutputParser.DealTextratorOutput(output);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DealTextratorOutput_ShouldHandleFormatException()
    {
        // PID 字段包含非十六进制字符 'X'，会导致 FormatException
        const string output = "[0:3A9X:401000:00000000:00000000:TID_Eng:HB0@401000] Format Error";

        // Act
        var result = TextractorOutputParser.DealTextratorOutput(output);

        // Assert
        Assert.Null(result);
    }
}