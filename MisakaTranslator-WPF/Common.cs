using KeyboardMouseHookLibrary;
using OCRLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using TextHookLibrary;
using TextRepairLibrary;

namespace MisakaTranslator
{
    public enum GuideMode
    {
        Hook = 1,
        Ocr = 2,
        Rehook = 3,
        Clipboard = 4,
    }

    public enum TransMode
    {
        Hook = 1,
        Ocr = 2,
    }

    public enum TTSMode
    {
        Azure = 1,
        Local = 2,
    }

    public enum PhoneticNotationType
    {
        Hiragana = 1,
        Katakana = 2,
        Romaji = 3,
    }

    public static class Common
    {
        public static IAppSettings AppSettings { get; set; } = default!;//应用设置
        public static IRepeatRepairSettings RepairSettings { get; set; } = default!; //去重方法参数

        public static TransMode TransMode { get; set; } //全局使用中的翻译模式 1=hook 2=ocr

        public static Guid GameID { get; set; } //全局使用中的游戏ID(数据库)

        public static TextHookHandle? TextHooker { get; set; } //全局使用中的Hook对象
        public static string? UsingRepairFunc { get; set; } //全局使用中的去重方法

        public static string UsingSrcLang { get; set; } = "ja";//全局使用中的源语言
        public static string UsingDstLang { get; set; } = "zh"; //全局使用中的目标翻译语言

        public static OCREngine? Ocr { get; set; } //全局使用中的OCR对象
        public static bool IsAllWindowCap { get; set; } //是否全屏截图
        public static IntPtr OCRWinHwnd { get; set; } //全局的OCR的工作窗口
        public static HotKeyInfo UsingHotKey { get; set; } = default!; //全局使用中的触发键信息
        public static int UsingOCRDelay { get; set; } //全局使用中的OCR延时

        public static GlobalHotKey GlobalOCRHotKey { get; set; } = default!; //全局OCR热键

        /// <summary>
        /// 导出Textractor历史记录，返回是否成功的结果
        /// </summary>
        /// <returns></returns>
        public static bool ExportTextractorHistory()
        {
            try
            {
                if (TextHooker != null)
                {
                    FileStream fs = new FileStream("TextractorOutPutHistory.txt", FileMode.Create);
                    StreamWriter sw = new StreamWriter(fs);

                    sw.WriteLine(Application.Current.Resources["Common_TextractorHistory"]);
                    string[] history = TextHooker.TextractorOutPutHistory.ToArray();
                    for (int i = 0; i < history.Length; i++)
                    {
                        sw.WriteLine(history[i]);
                    }

                    sw.Flush();
                    sw.Close();
                    fs.Close();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        /// <summary>
        /// 文本去重方法初始化
        /// </summary>
        public static void RepairFuncInit()
        {
            TextRepair.SingleWordRepeatTimes = RepairSettings.SingleWordRepeatTimes;
            TextRepair.SentenceRepeatFindCharNum = RepairSettings.SentenceRepeatFindCharNum;
            TextRepair.RegexPattern = RepairSettings.Regex;
            TextRepair.RegexReplacement = RepairSettings.Regex_Replace;
        }

        /// <summary>
        /// 全局OCR
        /// </summary>
        public static void GlobalOCR()
        {
            BitmapImage img = ImageProcFunc.ImageToBitmapImage(ScreenCapture.GetAllWindow())!;
            ScreenCaptureWindow scw = new ScreenCaptureWindow(img, 2);
            scw.Width = img.Width;
            scw.Height = img.Height;
            scw.Left = 0;
            scw.Top = 0;
            scw.Show();
        }

        static double scale;

        /// <summary>
        /// 获取DPI缩放倍数
        /// </summary>
        /// <returns>DPI缩放倍数</returns>
        public static double GetScale()
        {
            if (scale == 0)
                scale = Graphics.FromHwnd(new WindowInteropHelper(Application.Current.MainWindow).Handle).DpiX / 96;
            return scale;
        }

        /// <summary>
        /// 检查软件更新
        /// </summary>
        public static async Task<(bool, Version)> IsUpdateAvailable()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version();
            Version latestVersion = await GetLatestVersionAsync();
            return (latestVersion > version, latestVersion);
        }
        private static async Task<Version> GetLatestVersionAsync()
        {

            string url = "https://api.github.com/repos/liscs/MisakaTranslator/releases/latest";
            using HttpClient httpClient = new()
            {
                Timeout = TimeSpan.FromSeconds(30),
            };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MisakaTranslator");
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(url);
                request.Headers.Add("Cache-Control", "no-cache");
                using HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonNode? jsonNode = JsonSerializer.Deserialize<JsonNode>(result);
                    string? versionString = jsonNode?["tag_name"]?.GetValue<string>();

                    if (!string.IsNullOrEmpty(versionString))
                    {
                        int[] versionNumber = versionString.Split('v', '.')
                            .Where(p => !string.IsNullOrEmpty(p))
                            .Select(int.Parse).ToArray();
                        return new Version(versionNumber[0], versionNumber[1], versionNumber[2]);
                    }
                }
                return new Version();
            }
        }

        /// <summary>
        /// 取字符串中间
        /// </summary>
        /// <param name="oldStr"></param>
        /// <param name="preStr"></param>
        /// <param name="nextStr"></param>
        /// <returns></returns>
        public static string? GetMiddleStr(string oldStr, string preStr, string nextStr)
        {
            try
            {
                string tempStr = oldStr.Substring(oldStr.IndexOf(preStr) + preStr.Length);
                tempStr = tempStr.Substring(0, tempStr.IndexOf(nextStr));
                return tempStr;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}