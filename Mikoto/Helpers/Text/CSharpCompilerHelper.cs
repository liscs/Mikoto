using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.IO;
using System.Reflection;

namespace Mikoto
{

    public static class CSharpCompilerHelper
    {
        public static List<PortableExecutableReference> References =
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
                .Select(_ => MetadataReference.CreateFromFile(_.Location))
                .Concat(new[]
                {
            // add your app/lib specifics, e.g.:                      
            MetadataReference.CreateFromFile(typeof(Program).Assembly.Location),
                })
                .ToList();


        /// <summary>
        /// 编译顶级语句代码，从中取得第一个函数
        /// </summary>
        public static TextPreProcesFunction? GetProcessFunction(string scriptFile)
        {
            string code = File.ReadAllText(scriptFile);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            string assemblyName = Path.GetRandomFileName();


            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                [syntaxTree],
                References,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

            using var ms = new MemoryStream();
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                Logger.Warn($"Build {scriptFile} FAILED{Environment.NewLine}" + string.Join(Environment.NewLine, result.Diagnostics.Select(p => p.ToString())));
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
                return method?.CreateDelegate<TextPreProcesFunction>();
            }
        }
    }
}