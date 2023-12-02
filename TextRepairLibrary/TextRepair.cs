using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TextRepairLibrary.lang;

namespace TextRepairLibrary
{
    public static partial class TextRepair
    {
        public static string RegexReplacement { get; set; }
        public static string RegexPattern { get; set; }
        public static int SentenceRepeatFindCharNum { get; set; }
        public static int SingleWordRepeatTimes { get; set; }
        public static Dictionary<string, string> LstRepairFun { get; set; }

        static TextRepair()
        {
            Refresh();
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
        /// 反射调用字符串处理方法
        /// </summary>
        /// <param name="functionName">提供方法函数名</param>
        /// <param name="sourceText">源文本</param>
        /// <returns></returns>
        public static string RepairFun_Auto(string functionName, string sourceText)
        {
            if (functionName.StartsWith('#'))
            {
                return RepairFun_PythonScript(functionName.Substring(1), sourceText);
            }
            Type t = typeof(TextRepair);//括号中的为所要使用的函数所在的类的类名
            MethodInfo mt = t.GetMethod(functionName);
            if (mt != null)
            {
                string str = (string)mt.Invoke(null, new object[] { sourceText });
                return str;
            }
            else
            {
                return Strings.MethodError;
            }
        }

        public static string RepairFun_NoDeal(string source) => source;

        /// <summary>
        /// 处理单字重复
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
            int findNum = SentenceRepeatFindCharNum;

            if (source == string.Empty || source.Length < findNum)
            {
                return string.Empty;
            }

            char[] arr = source.ToCharArray();
            Array.Reverse(arr);
            string text = new string(arr);

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
            string ret = new string(arr1);

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
                Type t = asb.GetType("UserCustomRepairRepeat.RepairRepeat");//获取类名 命名空间+类名
                object o = Activator.CreateInstance(t);
                MethodInfo method = t.GetMethod("UserCustomRepairRepeatFun");//functionname:方法名字
                object[] obj =
                {
                    source
                };
                var ret = method.Invoke(o, obj);
                return (string)ret;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

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

            ScriptEngine pythonEngine = Python.CreateEngine();
            ScriptSource pythonScript = pythonEngine.CreateScriptSourceFromString(
                $"import textRepairPlugins.{handler} as customHandler\n" +
                "ResultStr = customHandler.process(SourceStr)\n"
                );
            ScriptScope scope = pythonEngine.CreateScope();
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

        public static void Refresh()
        {
            LstRepairFun = new Dictionary<string, string>() {
            { Strings.NoDeal , "RepairFun_NoDeal" },
            { Strings.RemoveSingleWordRepeat , "RepairFun_RemoveSingleWordRepeat" },
            { Strings.RemoveSentenceRepeat , "RepairFun_RemoveSentenceRepeat" },
            { Strings.RemoveLetterNumber , "RepairFun_RemoveLetterNumber" },
            { Strings.RemoveHTML , "RepairFun_RemoveHTML" },
            { Strings.RegexReplace , "RepairFun_RegexReplace" },
            { Strings.Custom , "RepairFun_Custom" }
            };

        }
    }
}
