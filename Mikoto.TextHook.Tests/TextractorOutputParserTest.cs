using System;
using System.Collections.Generic;
using System.Text;

namespace Mikoto.TextHook.Tests
{
    [TestClass]
    public class TextractorOutputParserTest
    {
        [TestMethod]
        public void GetMiddleString_ShouldExtractCorrectly()
        {
            // Arrange
            const string text = "start[middle]end";

            // Act
            string? result = TextractorOutputParser.GetMiddleString(text, "[", "]", 0);

            // Assert
            Assert.AreEqual("middle", result, "应能正确提取被方括号包裹的文本。");
        }

        [TestMethod]
        public void GetMiddleString_ShouldReturnNull_OnNotFound()
        {
            // Arrange
            const string text = "start[middle"; // 缺少后半部分

            // Act
            string? result = TextractorOutputParser.GetMiddleString(text, "[", "]", 0);

            // Assert
            Assert.IsNull(result, "如果分隔符未找到，应返回 null。");
        }

        [TestMethod]
        public void GetMiddleString_ShouldHandleStartingLocation()
        {
            // Arrange
            const string text = "[A] [B] [C]";

            // Act
            string? result = TextractorOutputParser.GetMiddleString(text, "[", "]", 3); // 从索引 5 之后开始搜索

            // Assert
            Assert.AreEqual("B", result, "应从指定的起始位置开始搜索。");
        }



        [TestMethod]
        public void DealTextratorOutput_ShouldParseStandardOutput()
        {
            // 标准格式：[版本:PID:Hook地址:值1:值2:方法名:HookCode] 文本
            const string output = "[0:3A98:401000:00000000:00000000:TID_Eng:HB0@401000] Actual Game Text";

            // Act
            var result = TextractorOutputParser.DealTextratorOutput(output);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0x3A98, result!.GamePID, "PID 解析错误。");
            Assert.AreEqual("TID_Eng", result.HookFunc, "方法名解析错误。");
            Assert.AreEqual("HB0@401000", result.HookCode, "HookCode 解析错误。");
            Assert.AreEqual("401000", result.HookAddress, "Hook地址解析错误。");
            Assert.AreEqual("【401000:00000000:00000000】", result.MisakaHookCode, "MisakaCode 组合错误。");
            Assert.AreEqual("Actual Game Text", result.Data, "实际内容解析错误。");
        }

        [TestMethod]
        public void DealTextratorOutput_ShouldHandleTextContinuation()
        {
            // Arrange
            const string firstLine = "[0:1234:401000:00000000:00000000:Func:Code] This is the first part";
            const string secondLine = " and this is the second part."; // 没有 [] 头的后续行

            // Act 
            // 第一次调用：解析并设置内部的 _thData
            TextractorOutputParser.DealTextratorOutput(firstLine);
            // 第二次调用：文本被分割，应该连接到 _thData
            var result = TextractorOutputParser.DealTextratorOutput(secondLine, TextractorOutputParser.DealTextratorOutput(firstLine));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("This is the first part and this is the second part.", result!.Data, "文本连接失败。");
        }

        [TestMethod]
        public void DealTextratorOutput_ShouldReturnNull_OnMalformedInfo()
        {
            // 缺少部分字段
            const string output = "[0:3A98:401000:00000000:TID_Eng:HB0@401000] Invalid Text";

            // Act
            var result = TextractorOutputParser.DealTextratorOutput(output);

            // Assert
            Assert.IsNull(result, "信息头格式不完整时，应返回 null。");
        }

        [TestMethod]
        public void DealTextratorOutput_ShouldHandleFormatException()
        {
            // PID 字段包含非十六进制字符 'X'，会导致 FormatException
            const string output = "[0:3A9X:401000:00000000:00000000:TID_Eng:HB0@401000] Format Error";

            // Act
            var result = TextractorOutputParser.DealTextratorOutput(output);

            // Assert
            Assert.IsNull(result, "PID 字段格式错误时，应捕获异常并返回 null。");
        }
    }
}
