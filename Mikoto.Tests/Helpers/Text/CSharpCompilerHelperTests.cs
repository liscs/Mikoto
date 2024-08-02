using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

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
            TextPreProcessMethod? method = CSharpCompilerHelper.GetProcessMethod(code);
            if (method != null)
            {
                Console.WriteLine(method("complier"));
            }
        }
    }
}