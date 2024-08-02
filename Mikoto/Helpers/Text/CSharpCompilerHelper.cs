using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.IO;
using System.Reflection;

namespace Mikoto
{
    public delegate string TextPreProcessMethod(string str);

    public static class CSharpCompilerHelper
    {

        /// <summary>
        /// 编译顶级语句代码，从中取得第一个函数
        /// </summary>
        public static TextPreProcessMethod? GetProcessMethod(string code)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references =
            [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
            ];

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

            using var ms = new MemoryStream();
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                foreach (Diagnostic diagnostic in result.Diagnostics)
                {
                    Console.WriteLine(diagnostic.ToString());
                }
                return null;
            }
            else
            {
                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());

                // 获取包含顶级语句的类型
                Type? type = assembly.GetType("Program");

                MethodInfo? method = type?.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                          .First(p => p != assembly.EntryPoint);
                return method?.CreateDelegate<TextPreProcessMethod>();
            }
        }
    }
}