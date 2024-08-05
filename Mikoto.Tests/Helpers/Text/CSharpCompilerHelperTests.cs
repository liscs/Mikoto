using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mikoto.Tests
{
    [TestClass()]
    public class CSharpCompilerHelperTests
    {
        [TestMethod()]
        public void CompileCSharpScriptTest()
        {
            string code = @"
        using System;

        string Process(string input)
        {
            return ""Hello, "" + input;
        }
        ";
            TextPreProcessFunction? method = CSharpCompilerHelper.GetProcessFunction(code);
            if (method != null)
            {
                Console.WriteLine(method("complier"));
            }
        }
    }
}