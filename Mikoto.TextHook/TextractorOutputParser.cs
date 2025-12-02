using System.Runtime.CompilerServices;

namespace Mikoto.TextHook
{
    internal static class TextractorOutputParser
    {
        /// <summary>
        /// 智能处理来自Textrator的输出并返回一个TextHookData用于下一步处理(TextHookData可为空)
        /// 具体的含义参见TextHookData定义
        /// </summary>
        /// <param name="outputText">来自Textrator的输出</param>
        /// <returns></returns>
        public static TextHookData? DealTextratorOutput(string outputText, TextHookData? preData = null)
        {
            if (outputText == "" || outputText == null)
            {
                return null;
            }

            string? info = GetMiddleString(outputText, "[", "]", 0);
            if (info == null)
            {
                if (preData == null)
                {
                    return null;
                }
                //得到的是第二段被截开的输出，需要连到上一段内
                preData.Data += outputText;
                return preData;
            }

            string[] Infores = info.Split(':');

            if (Infores.Length >= 7)
            {
                TextHookData thd = new TextHookData();

                string content = outputText.Replace("[" + info + "] ", "");//删除信息头部分
                try
                {
                    thd.GamePID = int.Parse(Infores[1], System.Globalization.NumberStyles.HexNumber); //游戏/本体进程ID（为0一般代表Textrator本体进程ID）

                }
                catch (FormatException)
                {
                    return null;
                }

                thd.HookFunc = Infores[5]; //方法名：Textrator注入游戏进程获得文本时的方法名（为 Console 时代表Textrator本体控制台输出；为 Clipboard 时代表从剪贴板获取的文本）

                thd.HookCode = Infores[6]; //特殊码：Textrator注入游戏进程获得文本时的方法的特殊码，是一个唯一值，可用于判断

                thd.Data = content; //实际获取到的内容

                thd.HookAddress = Infores[2]; //Hook入口地址：可用于以后卸载Hook

                thd.MisakaHookCode = "【" + Infores[2] + ":" + Infores[3] + ":" + Infores[4] + "】"; //【值1:值2:值3】见上方格式说明


                return thd;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 提取两个标记之间的中间文本（支持从指定位置开始搜索）
        /// </summary>
        /// <param name="text">完整文本</param>
        /// <param name="front">前标记</param>
        /// <param name="back">后标记</param>
        /// <param name="startIndex">开始搜索的位置，默认为 0</param>
        /// <returns>找到返回中间内容，未找到返回 null</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static string? GetMiddleString(string text, string front, string back, int startIndex = 0)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(front) || string.IsNullOrEmpty(back))
                return null;

            if (startIndex < 0) startIndex = 0;

            int frontIndex = text.IndexOf(front, startIndex, StringComparison.Ordinal);
            if (frontIndex == -1) return null;

            int start = frontIndex + front.Length;
            int backIndex = text.IndexOf(back, start, StringComparison.Ordinal);
            if (backIndex == -1) return null;

            return text[start..backIndex];
        }
    }
}