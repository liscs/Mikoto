using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Reflection;
using System.Text.RegularExpressions;
using TextRepairLibrary.lang;

namespace TextRepairLibrary
{
    public static partial class TextRepair
    {
        public static string? RegexReplacement { get; set; }
        public static string? RegexPattern { get; set; }
        public static int SentenceRepeatFindCharNum { get; set; }
        public static int SingleWordRepeatTimes { get; set; }
        public static Dictionary<string, string> LstRepairFun { get; set; } = new Dictionary<string, string>() {
            { Strings.NoDeal , nameof(RepairFun_NoDeal) },
            { Strings.RemoveSingleWordRepeat , nameof(RepairFun_RemoveSingleWordRepeat) },
            { Strings.RemoveSentenceRepeat , nameof(RepairFun_RemoveSentenceRepeat) },
            { Strings.RemoveLetterNumber , nameof(RepairFun_RemoveLetterNumber) },
            { Strings.RemoveHTML , nameof(RepairFun_RemoveHTML) },
            { Strings.RegexReplace , nameof(RepairFun_RegexReplace) },
            { Strings.Custom , nameof(RepairFun_Custom) }
            };

        static TextRepair()
        {
            try
            {
                string[] handlers = Directory.GetFiles("textRepairPlugins");
                foreach (var handler in handlers)
                {
                    string stem = Path.GetFileNameWithoutExtension(handler);
                    string ext = Path.GetExtension(handler);
                    if (ext != ".py" || stem == "__init__")
                    {
                        continue;
                    }
                    LstRepairFun.Add(Strings.CustomPythonScript + stem, "#" + stem);
                }
            }
            catch { }
        }


        /// <summary>
        /// 调用字符串处理方法
        /// </summary>
        /// <param name="functionName">提供方法函数名</param>
        /// <param name="sourceText">源文本</param>
        /// <returns></returns>
        public static string RepairFun_Auto(string? functionName, string sourceText)
        {
            if (functionName?.StartsWith('#') ?? false)
            {
                return RepairFun_PythonScript(functionName.Substring(1), sourceText);
            }
            return functionName switch
            {
                nameof(RepairFun_NoDeal) => RepairFun_NoDeal(sourceText),
                nameof(RepairFun_RemoveSingleWordRepeat) => RepairFun_RemoveSingleWordRepeat(sourceText),
                nameof(RepairFun_RemoveSentenceRepeat) => RepairFun_RemoveSentenceRepeat(sourceText),
                nameof(RepairFun_RemoveLetterNumber) => RepairFun_RemoveLetterNumber(sourceText),
                nameof(RepairFun_RemoveHTML) => RepairFun_RemoveHTML(sourceText),
                nameof(RepairFun_RegexReplace) => RepairFun_RegexReplace(sourceText),
                nameof(RepairFun_Custom) => RepairFun_Custom(sourceText),
                _ => Strings.MethodError,
            };
        }

        public static string RepairFun_NoDeal(string source) => source;

        /// <summary>
        /// 处理单字重复，置0以自动检测处理单字重复
        /// 可以设置重复次数更准确的进行去重
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RepairFun_RemoveSingleWordRepeat(string source)
        {
            if (source == string.Empty)
            {
                return string.Empty;
            }

            int repeatTimes = SingleWordRepeatTimes;
            int flag = 0;
            string ret = string.Empty;

            if (repeatTimes <= 1)
            {
                //未设置固定重复次数时，逢重复就删
                for (int i = 1; i < source.Length; i++)
                {
                    if (source[i] != source[flag])
                    {
                        ret += source[flag];
                        flag = i;
                    }
                }
                ret += source[source.Length - 1];
            }
            else
            {
                //设置了固定重复次数且重复次数大于等于1的情况
                int r = 0;
                for (int i = 1; i < source.Length; i++)
                {
                    if (source[i] == source[flag])
                    {
                        r++;
                        if (r > repeatTimes)
                        {
                            ret += source[i];
                            r = 0;
                        }
                    }
                    else
                    {
                        ret += source[flag];
                        flag = i;
                        r = 0;
                    }
                }
                ret += source[source.Length - 1];
            }
            return ret;
        }

        /// <summary>
        /// 句子重复处理
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RepairFun_RemoveSentenceRepeat(string source)
        {
            if (SentenceRepeatFindCharNum <= 1) { return source; }
            int findNum = SentenceRepeatFindCharNum;

            if (source == string.Empty || source.Length < findNum)
            {
                return source;
            }

            char[] arr = source.ToCharArray();
            Array.Reverse(arr);
            string text = new(arr);

            string cmp = string.Empty;
            for (int i = 1; i <= findNum; i++)
            {
                cmp += text[i];
            }

            int pos = text.IndexOf(cmp, findNum);
            if (pos == -1)
            {
                return Strings.SentenceRepeatError;
            }

            string t1 = text.Remove(pos, text.Length - pos);

            char[] arr1 = t1.ToCharArray();
            Array.Reverse(arr1);
            string ret = new(arr1, 1, arr1.Length - 1);

            return ret;
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
        public static string RepairFun_RegexReplace(string source)
        {
            if (RegexPattern == null || RegexReplacement == null || source == string.Empty)
            {
                return string.Empty;
            }
            return Regex.Replace(source, RegexPattern, RegexReplacement);
        }


        /// <summary>
        /// 用户自定义方法
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RepairFun_Custom(string source)
        {
            if (source == string.Empty)
            {
                return string.Empty;
            }
            try
            {
                Assembly asb = Assembly.LoadFrom(Environment.CurrentDirectory + "\\UserCustomRepairRepeat.dll");
                Type? t = asb.GetType("UserCustomRepairRepeat.RepairRepeat");//获取类名 命名空间+类名
                if (t == null) throw new NullReferenceException("namespace or type not found!");
                object? o = Activator.CreateInstance(t);
                MethodInfo? method = t.GetMethod("UserCustomRepairRepeatFun");//functionname:方法名字
                object[] obj =
                {
                    source
                };
                string? ret = method?.Invoke(o, obj) as string;
                if (ret == null)
                {
                    return string.Empty;
                }
                else
                {
                    return ret;
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private static ScriptEngine pythonEngine = Python.CreateEngine();
        private static ScriptScope scope = pythonEngine.CreateScope();
        private static string nowHandler = "example";
        private static ScriptSource pythonScript = pythonEngine.CreateScriptSourceFromString(
                $"import textRepairPlugins.example as customHandler\n" +
                "ResultStr = customHandler.process(SourceStr)\n"
                );
        /// <summary>
        /// 用户自定义Python脚本
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RepairFun_PythonScript(string handler, string source)
        {
            if (source == string.Empty)
            {
                return string.Empty;
            }
            if (nowHandler != handler)
            {
                nowHandler = handler;
                pythonScript = pythonEngine.CreateScriptSourceFromString(
                $"import textRepairPlugins.{handler} as customHandler\n" +
                "ResultStr = customHandler.process(SourceStr)\n"
                );
            }
            scope.SetVariable("SourceStr", source);
            try
            {
                pythonScript.Execute(scope);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return (string)scope.GetVariable("ResultStr");
        }

    }
}
