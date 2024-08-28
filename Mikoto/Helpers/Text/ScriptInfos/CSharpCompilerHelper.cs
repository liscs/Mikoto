using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Mikoto.Helpers.Text.ScriptInfos
{
    public static class CSharpCompilerHelper
    {
        public static List<MetadataReference> References { get; set; } = GetGlobalReferences();

        private static List<MetadataReference> GetGlobalReferences()
        {
            Assembly[] assemblies =
            [
    typeof(string).Assembly,            // System.String
    typeof(System.Text.RegularExpressions.Regex).Assembly, // System.Text.RegularExpressions.Regex
    typeof(Uri).Assembly,               // System.Uri
    typeof(Enumerable).Assembly,   // System.Linq.Enumerable
    typeof(System.Diagnostics.Trace).Assembly, // System.Diagnostics.Trace
    typeof(System.Security.Cryptography.HashAlgorithm).Assembly, // System.Security.Cryptography.HashAlgorithm
    typeof(System.Collections.Queue).Assembly, // System.Collections.Queue
    typeof(IQueryable<>).Assembly, // System.Linq.IQueryable<T>
            ];


            var returnList = assemblies
                .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p.Location))
                .ToList();

            //The location of the .NET assemblies
            string assemblyPath = RuntimeEnvironment.GetRuntimeDirectory();


            //Adding some necessary .NET assemblies
            //These assemblies couldn't be loaded correctly via the same construction as above,
            //in specific the System.Runtime.

            returnList.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")));
            returnList.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")));
            returnList.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")));
            returnList.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

            return returnList;
        }




        /// <summary>
        /// 编译顶级语句代码，从中取得第一个函数
        /// </summary>
        public static TextPreProcessFunction? GetProcessFunction(string script)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(script);
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
                string error = string.Join(Environment.NewLine, result.Diagnostics.Select(p => p.ToString()));
                throw new ApplicationException(error);
            }
            else
            {
                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());

                // 获取包含顶级语句的类型
                Type? type = assembly.GetType("Program");

                MethodInfo? method = type?.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                          .First(p => p != assembly.EntryPoint);
                return method?.CreateDelegate<TextPreProcessFunction>();
            }
        }
    }
}