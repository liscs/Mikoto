﻿using HandyControl.Controls;
using Microsoft.Scripting.Utils;
using Mikoto.Helpers;
using Mikoto.Helpers.Text;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using static Community.CsharpSqlite.Sqlite3;
using static IronPython.Modules._ast;


namespace Mikoto
{
    public delegate string TextPreProcesFunction(string str);
    public static partial class TextRepair
    {
        public static Task CustomScriptInitTask { get; private set; } = default!;

        private const string SCRIPTS_PATH = "data\\custom scripts\\";
        public static string? RegexReplacement { get; set; }
        public static string? RegexPattern { get; set; }
        public static int SentenceRepeatFindCharNum { get; set; }
        public static int SingleWordRepeatTimes { get; set; }
        public static Lazy<Dictionary<string, string>> RepairFunctionNameDict { get; } = new(() => new() {
            { Application.Current.Resources["NoDeal"].ToString()!, nameof(RepairFun_NoDeal) },
            { Application.Current.Resources["RemoveSingleWordRepeat"].ToString()!, nameof(RepairFun_RemoveSingleWordRepeat) },
            { Application.Current.Resources["RemoveSentenceRepeat"].ToString()!, nameof(RepairFun_RemoveSentenceRepeat) },
            { Application.Current.Resources["RemoveLetterNumber"].ToString()!, nameof(RepairFun_RemoveLetterNumber) },
            { Application.Current.Resources["RemoveHTML"].ToString()!, nameof(RepairFun_RemoveHTML) },
            { Application.Current.Resources["RegexReplace"].ToString()!, nameof(RepairFun_RegexReplace) },
            });

        public static SuppressibleObservableCollection<string> RepairFunctionNameList { get; set; } = new(RepairFunctionNameDict.Value.Keys);


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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RepairFunctionNameList.SuppressNotification = true;
                    RepairFunctionNameList.AddRange(CustomMethodsDict.Keys.ToList().Order());
                    RepairFunctionNameList.SuppressNotification = false;
                });
            });
            return;
        }


        public static Dictionary<string, TextPreProcesFunction> CustomMethodsDict { get; } = new();



        /// <summary>
        /// 调用字符串处理方法
        /// </summary>
        /// <param name="functionName">提供方法函数名</param>
        /// <param name="sourceText">源文本</param>
        /// <returns></returns>
        public static string RepairFun_Auto(string functionName, string sourceText)
        {
            if (CustomMethodsDict.TryGetValue(functionName, out TextPreProcesFunction? function))
            {
                try
                {
                    return function(sourceText);
                }
                catch (Exception ex)
                {
                    //脚本运行时错误
                    Growl.ErrorGlobal($"{functionName}{Environment.NewLine}{ex.Message}");
                    Logger.Warn($"{functionName}{Environment.NewLine}{ex}");
                }
            }
            return functionName switch
            {
                nameof(RepairFun_NoDeal) => RepairFun_NoDeal(sourceText),
                nameof(RepairFun_RemoveSingleWordRepeat) => RepairFun_RemoveSingleWordRepeat(sourceText),
                nameof(RepairFun_RemoveSentenceRepeat) => RepairFun_RemoveSentenceRepeat(sourceText),
                nameof(RepairFun_RemoveLetterNumber) => RepairFun_RemoveLetterNumber(sourceText),
                nameof(RepairFun_RemoveHTML) => RepairFun_RemoveHTML(sourceText),
                nameof(RepairFun_RegexReplace) => RepairFun_RegexReplace(sourceText),
                _ => Application.Current.Resources["MethodError"].ToString()!,
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
                return Application.Current.Resources["SentenceRepeatError"].ToString()!;
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


    }

}
