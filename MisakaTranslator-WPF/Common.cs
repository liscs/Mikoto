﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using KeyboardMouseHookLibrary;
using OCRLibrary;
using SQLHelperLibrary;
using TextHookLibrary;
using TextRepairLibrary;

namespace MisakaTranslator_WPF
{
    public class Common
    {
        public static IAppSettings appSettings; //应用设置
        public static IRepeatRepairSettings repairSettings; //去重方法参数

        public static int transMode; //全局使用中的翻译模式 1=hook 2=ocr

        public static int GameID; //全局使用中的游戏ID(数据库)

        public static TextHookHandle textHooker; //全局使用中的Hook对象
        public static string UsingRepairFunc; //全局使用中的去重方法

        public static string UsingSrcLang; //全局使用中的源语言
        public static string UsingDstLang; //全局使用中的目标翻译语言

        public static OCREngine ocr; //全局使用中的OCR对象
        public static bool isAllWindowCap; //是否全屏截图
        public static IntPtr OCRWinHwnd; //全局的OCR的工作窗口
        public static HotKeyInfo UsingHotKey; //全局使用中的触发键信息
        public static int UsingOCRDelay; //全局使用中的OCR延时

        public static Window mainWin; //全局的主窗口对象

        public static GlobalHotKey GlobalOCRHotKey; //全局OCR热键

        /// <summary>
        /// 导出Textractor历史记录，返回是否成功的结果
        /// </summary>
        /// <returns></returns>
        public static bool ExportTextractorHistory()
        {
            try
            {
                if (textHooker != null)
                {
                    FileStream fs = new FileStream("TextractorOutPutHistory.txt", FileMode.Create);
                    StreamWriter sw = new StreamWriter(fs);

                    sw.WriteLine("=================以下是Textractor的历史输出记录================");
                    string[] history = textHooker.TextractorOutPutHistory.ToArray();
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
            catch (System.NullReferenceException)
            {
                return false;
            }
        }

        /// <summary>
        /// 文本去重方法初始化
        /// </summary>
        public static void RepairFuncInit()
        {
            TextRepair.SingleWordRepeatTimes = repairSettings.SingleWordRepeatTimes;
            TextRepair.SentenceRepeatFindCharNum = repairSettings.SentenceRepeatFindCharNum;
            TextRepair.regexPattern = repairSettings.Regex;
            TextRepair.regexReplacement = repairSettings.Regex_Replace;
        }

        /// <summary>
        /// 全局OCR
        /// </summary>
        public static void GlobalOCR()
        {
            BitmapImage img = ImageProcFunc.ImageToBitmapImage(ScreenCapture.GetAllWindow());
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
                scale = Graphics.FromHwnd(new WindowInteropHelper(mainWin).Handle).DpiX / 96;
            return scale;
        }

        /// <summary>
        /// 检查软件更新
        /// </summary>
        /// <returns>如果已经是最新或获取更新失败，返回NULL，否则返回更新信息可直接显示</returns>
        public static List<string> CheckUpdate() {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string currentVersion = version.ToString();

            string url = "https://hanmin0822.github.io/MisakaTranslator/index.html";

            string strResult = "";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //声明一个HttpWebRequest请求
                request.Timeout = 30000;
                //设置连接超时时间
                request.Headers.Set("Pragma", "no-cache");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream streamReceive = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("GB2312");
                StreamReader streamReader = new StreamReader(streamReceive, encoding);
                strResult = streamReader.ReadToEnd();
            }
            catch
            {
                return null;
            }

            if (strResult != null) {
                string newVersion = GetMiddleStr(strResult, "LatestVersion[", "]");

                if (newVersion == null) {
                    return null;
                }

                if (currentVersion == newVersion)
                {
                    return null;
                }
                else {
                    string downloadPath = GetMiddleStr(strResult, "DownloadPath[", "]");
                    return new List<string>() {
                        newVersion,downloadPath
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// 取字符串中间
        /// </summary>
        /// <param name="oldStr"></param>
        /// <param name="preStr"></param>
        /// <param name="nextStr"></param>
        /// <returns></returns>
        public static string GetMiddleStr(string oldStr, string preStr, string nextStr)
        {
            try
            {
                string tempStr = oldStr.Substring(oldStr.IndexOf(preStr) + preStr.Length);
                tempStr = tempStr.Substring(0, tempStr.IndexOf(nextStr));
                return tempStr;
            }
            catch (Exception) {
                return null;
            }
        }
    }
}