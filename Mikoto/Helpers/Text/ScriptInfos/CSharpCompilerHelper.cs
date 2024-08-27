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
    typeof(System.Text.StringBuilder).Assembly, // System.Text.StringBuilder
    typeof(System.Text.Encoding).Assembly,     // System.Text.Encoding
    typeof(System.Text.RegularExpressions.Regex).Assembly, // System.Text.RegularExpressions.Regex
    typeof(System.Globalization.CultureInfo).Assembly,     // System.Globalization.CultureInfo
    typeof(Convert).Assembly,           // System.Convert
    typeof(char).Assembly,              // System.Char
    typeof(StringComparer).Assembly,    // System.StringComparer
    typeof(StringComparison).Assembly,  // System.StringComparison
    typeof(System.Text.ASCIIEncoding).Assembly, // System.Text.ASCIIEncoding
    typeof(System.Text.UTF8Encoding).Assembly, // System.Text.UTF8Encoding
    typeof(System.Text.UnicodeEncoding).Assembly, // System.Text.UnicodeEncoding
    typeof(StringReader).Assembly,   // System.IO.StringReader
    typeof(StringWriter).Assembly,   // System.IO.StringWriter
    typeof(System.Globalization.TextInfo).Assembly, // System.Globalization.TextInfo
    typeof(System.Globalization.CompareInfo).Assembly, // System.Globalization.CompareInfo
    typeof(System.Globalization.DateTimeFormatInfo).Assembly, // System.Globalization.DateTimeFormatInfo
    typeof(System.Globalization.NumberFormatInfo).Assembly, // System.Globalization.NumberFormatInfo
    typeof(System.Net.WebUtility).Assembly,    // System.Net.WebUtility
    typeof(Uri).Assembly,               // System.Uri
    typeof(Enumerable).Assembly,   // System.Linq.Enumerable
    typeof(System.IO.File).Assembly,           // System.IO.File
    typeof(Path).Assembly,           // System.IO.Path
    typeof(Task).Assembly, // System.Threading.Tasks.Task
    typeof(System.Diagnostics.Debug).Assembly, // System.Diagnostics.Debug
    typeof(System.Diagnostics.Trace).Assembly, // System.Diagnostics.Trace
    typeof(System.Diagnostics.Stopwatch).Assembly, // System.Diagnostics.Stopwatch
    typeof(System.Security.Cryptography.HashAlgorithm).Assembly, // System.Security.Cryptography.HashAlgorithm
    typeof(System.Security.Cryptography.MD5).Assembly, // System.Security.Cryptography.MD5
    typeof(System.Security.Cryptography.SHA1).Assembly, // System.Security.Cryptography.SHA1
    typeof(System.Security.Cryptography.SHA256).Assembly, // System.Security.Cryptography.SHA256
    typeof(System.Security.Cryptography.SHA512).Assembly, // System.Security.Cryptography.SHA512
    typeof(MethodInfo).Assembly, // System.Reflection.MethodInfo
    typeof(PropertyInfo).Assembly, // System.Reflection.PropertyInfo
    typeof(FieldInfo).Assembly, // System.Reflection.FieldInfo
    typeof(List<>).Assembly, // System.Collections.Generic.List<T>
    typeof(Dictionary<,>).Assembly, // System.Collections.Generic.Dictionary<TKey, TValue>
    typeof(IEnumerable<>).Assembly, // System.Collections.Generic.IEnumerable<T>
    typeof(System.Collections.IEnumerable).Assembly, // System.Collections.IEnumerable
    typeof(System.Collections.ArrayList).Assembly, // System.Collections.ArrayList
    typeof(System.Collections.Hashtable).Assembly, // System.Collections.Hashtable
    typeof(System.Collections.Queue).Assembly, // System.Collections.Queue
    typeof(System.Collections.Stack).Assembly, // System.Collections.Stack
    typeof(IQueryable<>).Assembly, // System.Linq.IQueryable<T>
    typeof(System.Linq.Expressions.Expression).Assembly, // System.Linq.Expressions.Expression
    typeof(CancellationToken).Assembly, // System.Threading.CancellationToken
    typeof(Thread).Assembly, // System.Threading.Thread
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