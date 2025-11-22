using HandyControl.Controls;
using Mikoto.Enums;
using Mikoto.Helpers.Network;
using Mikoto.Windows.Logger;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mikoto
{
    public static class Common
    {
        public static IAppSettings AppSettings { get; set; } = default!;//应用设置
        public static IRepeatRepairSettings RepairSettings { get; set; } = default!; //去重方法参数

        private static void ShowUpdateMessageBox(Version latestVersion)
        {
            //TODO 提示本地化
            MessageBoxResult dr = MessageBox.Show($"{CurrentVersion.ToString(3)}->{latestVersion}{Environment.NewLine}{Application.Current.Resources["MainWindow_AutoUpdateCheck"]}",
                                                  Application.Current.Resources["MessageBox_Ask"].ToString(),
                                                  MessageBoxButton.OKCancel);

            if (dr == MessageBoxResult.OK)
            {
                Process.Start(new ProcessStartInfo("https://github.com/liscs/Mikoto/releases/latest") { UseShellExecute = true });
                //点击确认，自动下载最新版并替换重启
                //DownloadBackground(latestVersion);
            }
        }

        private static async void DownloadBackground(Version latestVersion)
        {
            string filename = GetDownloadZipFilename();
            string url = $"https://github.com/liscs/Mikoto/releases/download/v{latestVersion.ToString(3)}/{filename}";
            var temp = Directory.CreateTempSubdirectory();
            string filePath = Path.Combine(temp.FullName, filename); // 替换为你希望保存文件的路径

            HttpClient client = new() { Timeout = Timeout.InfiniteTimeSpan };
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                using (Stream stream = await response.Content.ReadAsStreamAsync())
                using (FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    await stream.CopyToAsync(fileStream);
                }

                ZipFile.ExtractToDirectory(filePath, temp.FullName);

                string extractedFolder = Path.Combine(temp.FullName, Path.GetFileNameWithoutExtension(filename));
                AskUpdateNow(extractedFolder);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
            }
            finally
            {
                client.Dispose();
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
                ProcessStartInfo processStartInfo = new("Mikoto.Updater.exe");
                processStartInfo.ArgumentList.Add(extractedFolder);
                processStartInfo.ArgumentList.Add(true.ToString());
                Process.Start(processStartInfo);
                Environment.Exit(0);
            }
            else
            {
                Application.Current.MainWindow.Closing += (s, e) =>
                {
                    ProcessStartInfo processStartInfo = new("Mikoto.Updater.exe");
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
            HttpClient httpClient = CommonHttpClient.Instance;
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

    }
}