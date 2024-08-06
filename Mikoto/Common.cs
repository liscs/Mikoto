using HandyControl.Controls;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using TextHookLibrary;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mikoto
{
    public enum Theme
    {
        Light, Dark,
    }

    public enum GuideMode
    {
        Hook = 1,
        Rehook = 3,
        Clipboard = 4,
    }

    public enum TransMode
    {
        Hook = 1,
        Clipboard = 4,
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

        public static TransMode TransMode { get; set; } //全局使用中的翻译模式 1=_hook 

        public static Guid GameID { get; set; } //全局使用中的游戏ID

        public static TextHookHandle? TextHooker { get; set; } //全局使用中的Hook对象
        public static string? UsingRepairFunc { get; set; } //全局使用中的去重方法

        public static string UsingSrcLang { get; set; } = "ja";//全局使用中的源语言
        public static string UsingDstLang { get; set; } = "zh"; //全局使用中的目标翻译语言

        public static bool IsAdmin
        {
            get
            {
                bool isElevated;
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                return isElevated;
            }
        }

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

        public static void ShowUpdateMessageBox(Version latestVersion)
        {
            //TODO 提示本地化
            MessageBoxResult dr = MessageBox.Show($"{CurrentVersion.ToString(3)}->{latestVersion}{Environment.NewLine}{Application.Current.Resources["MainWindow_AutoUpdateCheck"]}",
                                                  Application.Current.Resources["MessageBox_Ask"].ToString(),
                                                  MessageBoxButton.OKCancel);

            if (dr == MessageBoxResult.OK)
            {
                // Process.Start(new ProcessStartInfo("https://github.com/liscs/Mikoto/releases/latest") { UseShellExecute = true });
                //点击确认，自动下载最新版并替换重启
                _ = DownloadBackgroundAsync(latestVersion);
            }
        }

        private static async Task DownloadBackgroundAsync(Version latestVersion)
        {
            string filename = GetDownloadZipFilename();
            string url = $"https://github.com/liscs/Mikoto/releases/download/v{latestVersion.ToString(3)}/{filename}";
            var temp = Directory.CreateTempSubdirectory();
            string filePath = Path.Combine(temp.FullName, filename); // 替换为你希望保存文件的路径

            using HttpClient client = new HttpClient();
            try
            {
                byte[] fileBytes = await client.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(filePath, fileBytes);
                ZipFile.ExtractToDirectory(filePath, temp.FullName);

                string extractedFolder = Path.Combine(temp.FullName, Path.GetFileNameWithoutExtension(filename));
                AskUpdateNow(extractedFolder);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
            }
        }

        private static string GetDownloadZipFilename()
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => "mikoto-32bit.zip",
                Architecture.X64 => "mikoto-64bit.zip",
                Architecture.Arm64 => "mikoto-arm64.zip",
                _ => throw new UnreachableException(),
            };
        }

        private static void AskUpdateNow(string extractedFolder)
        {
            //TODO 提示本地化
            MessageBoxResult dr = MessageBox.Show("下载完成，是否立刻更新？", "", MessageBoxButton.OKCancel);
            if (dr == MessageBoxResult.OK)
            {
                ProcessStartInfo processStartInfo = new("Updater.exe");
                processStartInfo.ArgumentList.Add(extractedFolder);
                processStartInfo.ArgumentList.Add(true.ToString());
                Process.Start(processStartInfo);
                Environment.Exit(0);
            }
            else
            {
                Application.Current.MainWindow.Closing += (s, e) =>
                {
                    ProcessStartInfo processStartInfo = new("Updater.exe");
                    processStartInfo.ArgumentList.Add(extractedFolder);
                    processStartInfo.ArgumentList.Add(false.ToString());
                    Process.Start(processStartInfo);
                    Environment.Exit(0);
                };
            }
        }

        /// <summary>
        /// 检查软件更新
        /// </summary>
        public static async Task CheckUpdateAsync(bool activelyCheck = false)
        {
            Version currentVersion = CurrentVersion;
            try
            {
                Version latestVersion = await GetLatestVersionAsync();
                if (latestVersion > currentVersion)
                {
                    ShowUpdateMessageBox(latestVersion);
                }
                else if (activelyCheck)
                {
                    Growl.InfoGlobal(Application.Current.Resources["SoftwareSettingsPage_AlreadyLatest"].ToString());
                }
            }
            catch (HttpRequestException ex)
            {
                if (activelyCheck)
                    Growl.WarningGlobal(ex.Message + Environment.NewLine + Application.Current.Resources["SoftwareSettingsPage_RequestUpdateError"].ToString());
                Logger.Warn(ex);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                if (activelyCheck)
                    Growl.WarningGlobal(ex.InnerException.Message + Environment.NewLine + Application.Current.Resources["SoftwareSettingsPage_RequestUpdateError"].ToString());
                Logger.Warn(ex);
            }
            catch (TaskCanceledException ex)
            {
                if (activelyCheck)
                    Growl.WarningGlobal(ex.Message + Environment.NewLine + Application.Current.Resources["SoftwareSettingsPage_RequestUpdateError"].ToString());
                Logger.Warn(ex);
            }
        }

        public static Version CurrentVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version ?? new Version();
            }
        }

        public static Theme CurrentTheme
        {
            get
            {
                if (Enum.TryParse(typeof(Theme), (string)Application.Current.Resources["CurrentTheme"], out object? value))
                {
                    return (Theme)value;
                }
                else
                {
                    return Theme.Light;
                }

            }
        }

        /// <summary>
        /// 获取github最新release的tag作为最新版本
        /// </summary>
        private static async Task<Version> GetLatestVersionAsync()
        {
            string url = "https://api.github.com/repos/liscs/Mikoto/releases/latest";
            using HttpClient httpClient = new()
            {
                Timeout = TimeSpan.FromSeconds(30),
            };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mikoto");
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
                                                           .Select(int.Parse)
                                                           .ToArray();
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

    public enum CheckUpdateResult
    {
        CanUpdate,
        AlreadyLatest,
        RequestError,
    }
}