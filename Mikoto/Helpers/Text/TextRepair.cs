using HandyControl.Controls;
using Microsoft.Scripting.Utils;
using Mikoto.Helpers.Container;
using Mikoto.Helpers.Text.ScriptInfos;
using Serilog;
using System.Buffers;
using System.Text.RegularExpressions;
using System.Windows;


namespace Mikoto
{
    public delegate string TextPreProcessFunction(string str);
    public static partial class TextRepair
    {
        public static Task CustomScriptInitTask { get; private set; } = default!;
        private static readonly Lazy<Dictionary<string, string>> defaultDisplayNameDict = new(() => new() {
            { Application.Current.Resources["NoDeal"].ToString()!, nameof(RepairFun_NoDeal) },
            { Application.Current.Resources["RemoveSingleWordRepeat"].ToString()!, nameof(RepairFun_RemoveSingleWordRepeat) },
            { Application.Current.Resources["RemoveSentenceRepeat"].ToString()!, nameof(RepairFun_RemoveSentenceRepeat) },
            { Application.Current.Resources["RemoveLetterNumber"].ToString()!, nameof(RepairFun_RemoveLetterNumber) },
            { Application.Current.Resources["RemoveHTML"].ToString()!, nameof(RepairFun_RemoveHTML) },
            { Application.Current.Resources["RegexReplace"].ToString()!, nameof(RepairFun_RegexReplace) },
            });
        public static Lazy<Dictionary<string, string>> DisplayNameToNameDict { get; } = new(() => new(defaultDisplayNameDict.Value));

        public static Lazy<SuppressibleObservableCollection<string>> RepairFunctionNameList { get; set; } = new(() => new(DisplayNameToNameDict.Value.Keys));


        public static void InitCustomScripts()
        {
            CustomScriptInitTask = Task.Run(() =>
            {
                Parallel.Invoke(
                [
                    new CSharpScriptInfo().Init,
                    new PythonScriptInfo().Init,
                    new JsScriptInfo().Init,
                    new LuaScriptInfo().Init,
                ]);
                RefreshListOrder();

            });
            return;
        }

        public static void RefreshListOrder()
        {
            DisplayNameToNameDict.Value.Clear();
            foreach (var item in defaultDisplayNameDict.Value)
            {
                DisplayNameToNameDict.Value[item.Key] = item.Value;
            }
            foreach (var item in CustomMethodsDict.Keys)
            {
                DisplayNameToNameDict.Value[item] = item;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                RepairFunctionNameList.Value.SuppressNotification = true;
                for (int i = defaultDisplayNameDict.Value.Keys.Count; i < RepairFunctionNameList.Value.Count;)
                {
                    RepairFunctionNameList.Value.RemoveAt(i);
                }
                RepairFunctionNameList.Value.AddRange(CustomMethodsDict.Keys.ToList().Order());
                RepairFunctionNameList.Value.SuppressNotification = false;
            });

        }

        public static Dictionary<string, TextPreProcessFunction> CustomMethodsDict { get; } = new();



        /// <summary>
        /// 调用字符串处理方法
        /// </summary>
        /// <param name="functionName">提供方法函数名</param>
        /// <param name="sourceText">源文本</param>
        /// <returns></returns>
        public static string PreProcessSrc(string functionName, string sourceText)
        {
            if (CustomMethodsDict.TryGetValue(functionName, out TextPreProcessFunction? function))
            {
                try
                {
                    return function(sourceText);
                }
                catch (Exception ex)
                {
                    //脚本运行时错误
                    Growl.ErrorGlobal($"{functionName}{Environment.NewLine}{ex.Message}");
                    Log.Warning(ex, "自定义脚本 {FunctionName} 运行时异常", functionName);
                }
            }
            return functionName switch
            {
                nameof(RepairFun_NoDeal) => RepairFun_NoDeal(sourceText),
                nameof(RepairFun_RemoveSingleWordRepeat) => RepairFun_RemoveSingleWordRepeat(sourceText, Convert.ToInt32(App.Env.Context.GameInfo.RepairParamA)),
                nameof(RepairFun_RemoveSentenceRepeat) => RepairFun_RemoveSentenceRepeat(sourceText, Convert.ToInt32(App.Env.Context.GameInfo.RepairParamA)),
                nameof(RepairFun_RemoveLetterNumber) => RepairFun_RemoveLetterNumber(sourceText),
                nameof(RepairFun_RemoveHTML) => RepairFun_RemoveHTML(sourceText),
                nameof(RepairFun_RegexReplace) => RepairFun_RegexReplace(sourceText, App.Env.Context.GameInfo.RepairParamA, App.Env.Context.GameInfo.RepairParamB),
                _ => Application.Current.Resources["MethodError"].ToString()!,
            };
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

}
