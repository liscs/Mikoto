using Mikoto.Helpers.Text.ScriptInfos;
using Xunit;

namespace Mikoto.Tests;

public class CSharpCompilerHelperTests
{
    [Fact]
    public void CompileCSharpScriptTest()
    {
        string code = @"
        using System;

        string Process(string input)
        {
            return ""Hello, "" + input;
        }
        ";
        // 假设 TextPreProcessFunction 是一个委托类型
        TextPreProcessFunction? method = CSharpCompilerHelper.GetProcessFunction(code);

        if (method != null)
        {
            // 补充一个断言以验证编译成功后的执行结果
            Assert.Equal("Hello, complier", method("complier"));
        }
        else
        {
            // 如果编译失败，断言 method 不应该为 null
            Assert.True(method != null, "C#脚本编译失败。");
        }
    }
}