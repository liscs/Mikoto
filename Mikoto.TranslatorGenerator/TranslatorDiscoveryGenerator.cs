using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Mikoto.TranslatorGenerator;

// 标记这个类可以被编译器识别为 Source Generator
[Generator]
public class TranslatorDiscoveryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 核心逻辑集中在这里，定义数据流管道

        // 1. 定义一个 Provider 来查找所有实现了 ITranslator 的类型
        var translatorDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                // 筛选出可能是类声明的节点
                predicate: static (s, _) => s is ClassDeclarationSyntax,
                // 根据语义模型确认它是否实现了 ITranslator
                transform: static (ctx, _) => GetTranslatorClassInfo(ctx))
            // 过滤掉不相关的 null 值
            .Where(static m => m is not null)
            .Collect(); // 将所有结果收集到一个 IReadOnlyList 中

        // 2. 注册 Source Output，使用收集到的数据生成代码
        context.RegisterSourceOutput(translatorDeclarations,
            static (spc, classes) =>
            {
                var sourceCode = GenerateTranslatorList(classes!);

                // 添加生成的代码到编译
                spc.AddSource("GeneratedTranslators.g.cs", sourceCode);
            });
    }

    // 辅助方法：获取翻译器类的信息
    private static string? GetTranslatorClassInfo(GeneratorSyntaxContext context)
    {
        // 使用 context.SemanticModel 来检查 classDeclaration 是否实现了 ITranslator

        if (context.Node is ClassDeclarationSyntax classDeclaration)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
            if (symbol is ITypeSymbol typeSymbol)
            {
                // 替换为正确的 ITranslator 命名空间
                if (typeSymbol.AllInterfaces.Any(i => i.ToDisplayString() == "Mikoto.Translators.Interfaces.ITranslator"))
                {
                    return typeSymbol.Name; // 返回类名
                }
            }
        }
        return null;
    }

    private static string GenerateTranslatorList(IReadOnlyList<string> classNames)
    {

        return @"
namespace Mikoto.Translators
{
    public static partial class TranslatorCommon
    {
        public static readonly System.Collections.Generic.List<string> AllTranslatorClassNames = new()
        {
            " + string.Join(",\n            ", classNames.Select(n => $"\"{n}\"")) + @"
        };
    }
}";
    }
}