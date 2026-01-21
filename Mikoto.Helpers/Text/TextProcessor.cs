using Serilog;
using System.Buffers;
using System.Text.RegularExpressions;

namespace Mikoto.Helpers.Text;
/// <summary>
/// 字符串处理辅助类
/// </summary>
public partial class TextProcessor
{
    public static Dictionary<string, Func<string, string>> CustomMethodsDict { get; } = new();


    /// <summary>
    /// 调用字符串处理方法 (AOT 安全且带参数防御)
    /// </summary>
    public static string PreProcessSrc(string functionName, string sourceText, string? paramA, string? paramB)
    {
        if (string.IsNullOrEmpty(sourceText)) return string.Empty;

        // 1. 优先处理自定义脚本
        if (CustomMethodsDict.TryGetValue(functionName, out Func<string, string>? function))
        {
            try
            {
                return function(sourceText);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "自定义脚本 {FunctionName} 运行时异常", functionName);
                return sourceText; // 脚本报错时返回原文本，防止后续流程卡死
            }
        }

        // 2. 辅助解析方法：安全转换 int
        static int SafeParseInt(string? input, int defaultValue = 0)
            => int.TryParse(input, out int result) ? result : defaultValue;

        // 3. 处理内置方法
        try
        {
            return functionName switch
            {
                nameof(RepairFun_NoDeal) => RepairFun_NoDeal(sourceText),

                // 比如：用户在 paramA 误填了 "abc"，SafeParseInt 会返回 0，避免崩溃
                nameof(RepairFun_RemoveSingleWordRepeat) => RepairFun_RemoveSingleWordRepeat(sourceText, SafeParseInt(paramA)),

                nameof(RepairFun_RemoveSentenceRepeat) => RepairFun_RemoveSentenceRepeat(sourceText, SafeParseInt(paramA)),

                nameof(RepairFun_RemoveLetterNumber) => RepairFun_RemoveLetterNumber(sourceText),

                nameof(RepairFun_RemoveHTML) => RepairFun_RemoveHTML(sourceText),

                // 正则替换需要处理 pattern 为 null 的情况
                nameof(RepairFun_RegexReplace) => RepairFun_RegexReplace(sourceText, paramA ?? string.Empty, paramB ?? string.Empty),

                _ => sourceText // 未知方法名时直接返回原文本，不抛出异常
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "内置处理函数 {FunctionName} 执行失败", functionName);
            return sourceText;
        }
    }

    public static string RepairFun_NoDeal(string source) => source;

    /// <summary>
    /// 处理单字重复，置0以自动检测处理单字重复
    /// 可以设置重复次数更准确的进行去重
    /// </summary>
    public static string RepairFun_RemoveSingleWordRepeat(string source, int repeatTimes)
    {
        char[]? destinationArray = null;

        try
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return string.Empty;
            }

            int flag = 0;
            ReadOnlySpan<char> input = source.AsSpan();
            Span<char> destination = input.Length <= 256
            ? stackalloc char[input.Length]
            : (destinationArray = ArrayPool<char>.Shared.Rent(input.Length));
            int destIndex = 0;

            if (repeatTimes <= 1)
            {
                //未设置固定重复次数时，逢重复就删
                for (int i = 1; i < input.Length; i++)
                {
                    if (input[i] != input[flag])
                    {
                        destination[destIndex] = input[flag];
                        destIndex++;
                        flag = i;
                    }
                }
                destination[destIndex] = input[input.Length - 1];
                destIndex++;
            }
            else
            {
                //设置了固定重复次数且重复次数大于等于1的情况
                int r = 0;
                for (int i = 1; i < input.Length; i++)
                {
                    if (input[i] == input[flag])
                    {
                        r++;
                        if (r > repeatTimes)
                        {
                            destination[destIndex] = input[flag];
                            destIndex++;
                            r = 0;
                        }
                    }
                    else
                    {
                        destination[destIndex] = input[flag];
                        destIndex++;
                        flag = i;
                        r = 0;
                    }
                }
                destination[destIndex] = input[input.Length - 1];
                destIndex++;
            }
            return destination[..destIndex].ToString();
        }
        finally
        {
            // 必须归还从 ArrayPool 借出的内存
            if (destinationArray != null)
            {
                ArrayPool<char>.Shared.Return(destinationArray);
            }
        }
    }

    /// <summary>
    /// 句子重复处理
    /// </summary>
    /// <param name="source">源字符串</param>
    /// <param name="threshold">重复句子长度阈值</param>
    /// <returns>处理后的字符串</returns>
    public static string RepairFun_RemoveSentenceRepeat(string source, int threshold)
    {
        string pattern = $@"^(.{{{threshold},}}?)\1+$";
        return Regex.Replace(source, pattern, "$1");
    }


    /// <summary>
    /// 去字母和数字（包括大写和小写字母）
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string RepairFun_RemoveLetterNumber(string source)
    {
        string strRemoved = LetterRegex().Replace(source, string.Empty);
        strRemoved = NumberRegex().Replace(strRemoved, string.Empty);
        return strRemoved;
    }
    [GeneratedRegex("[0-9]")]
    private static partial Regex NumberRegex();
    [GeneratedRegex("[a-z]", RegexOptions.IgnoreCase)]
    private static partial Regex LetterRegex();

    /// <summary>
    /// 去除HTML标签
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string RepairFun_RemoveHTML(string source)
    {
        string strRemoved = AngleBracketsRegex().Replace(source, string.Empty);
        strRemoved = EscapeCharRegex().Replace(strRemoved, string.Empty);
        return strRemoved;
    }
    [GeneratedRegex("<[^>]+>")]
    private static partial Regex AngleBracketsRegex();
    [GeneratedRegex("&[^;]+;")]
    private static partial Regex EscapeCharRegex();

    /// <summary>
    /// 正则表达式替换
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string RepairFun_RegexReplace(string source, string? pattern, string? replace)
    {
        if (pattern == null || replace == null || source == string.Empty)
        {
            return string.Empty;
        }
        return Regex.Replace(source, pattern, replace);
    }
}
