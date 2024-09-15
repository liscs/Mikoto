using Mikoto.Enums;
using Mikoto.TextHook;
using System.IO;
using System.Windows;

namespace Mikoto
{
    internal class GlobalWorkingData
    {
        private GlobalWorkingData() { }
        private static readonly Lazy<GlobalWorkingData> _instance = new(() => new GlobalWorkingData());

        public static GlobalWorkingData Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        public TransMode TransMode { get; set; } //全局使用中的翻译模式 1=_hook 


        public TextHookHandle TextHooker { get; set; } = new();//全局使用中的Hook对象

        public Guid GameID { get; set; } //全局使用中的游戏ID
        public string? UsingRepairFunc { get; set; } //全局使用中的去重方法

        public string UsingSrcLang { get; set; } = "ja";//全局使用中的源语言
        public string UsingDstLang { get; set; } = "zh"; //全局使用中的目标翻译语言

        /// <summary>
        /// 导出Textractor历史记录，返回是否成功的结果
        /// </summary>
        /// <returns></returns>
        public bool ExportTextractorHistory()
        {
            try
            {
                using FileStream fs = new("TextractorOutPutHistory.txt", FileMode.Create);
                using StreamWriter sw = new(fs);

                sw.WriteLine(Application.Current.Resources["Common_TextractorHistory"]);
                string[] history = TextHooker.TextractorOutPutHistory.ToArray();
                for (int i = 0; i < history.Length; i++)
                {
                    sw.WriteLine(history[i]);
                }

                sw.Flush();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}