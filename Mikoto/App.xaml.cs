using Mikoto.DataAccess;
using Mikoto.Helpers.Input;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace Mikoto
{
    public partial class App
    {
        public App()
        {
            //注册开始和退出事件
            this.Startup += App_Startup;
            this.Exit += App_Exit;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 初始化全局快捷键管理
            GlobalKeyboardHook.Initialize();
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            //UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            SetDefaultFrameRate();
        }

        /// <summary>
        /// 设置目标刷新率与系统一致，获取失败则设置为60fps
        /// </summary>
        private static void SetDefaultFrameRate()
        {
            DEVMODEW dm = new();
            if (PInvoke.EnumDisplaySettings(null, ENUM_DISPLAY_SETTINGS_MODE.ENUM_CURRENT_SETTINGS, ref dm))
            {
                Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = (int)dm.dmDisplayFrequency });
            }
            else
            {
                Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 60 });
            }
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            //程序退出时检查是否断开Hook
            EndHook();
        }

        /// <summary>
        /// UI线程未捕获异常处理事件
        /// </summary>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            PrintErrorMessageToFile(e.Exception);
            ShowExceptionMessageBox(e.Exception);
        }

        private static void ShowExceptionMessageBox(object e)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            MessageBox.Show($"{Current.Resources["App_Global_ErrorHint_left"]}{nowTime}{Current.Resources["App_Global_ErrorHint_right"]}{Environment.NewLine}{e}",
                            Current.Resources["MessageBox_Error"].ToString());
        }

        /// <summary>
        /// 非UI线程未捕获异常处理事件
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Mikoto.MainWindow.Instance.CloseNotifyIcon();

            PrintErrorMessageToFile(e.ExceptionObject as Exception ??
                new Exception("Unknown exception: " + e.ExceptionObject));

            ShowExceptionMessageBox(e.ExceptionObject as Exception ??
                new Exception("Unknown exception: " + e.ExceptionObject));

            EndHook();
        }

        /// <summary>
        /// Task线程内未捕获异常处理事件
        /// </summary>
        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Mikoto.MainWindow.Instance.CloseNotifyIcon();
            PrintErrorMessageToFile(e.Exception);
            ShowExceptionMessageBox(e.Exception);
        }


        private static readonly int LogReserveDays = 30; // 保留天数（按需调整）

        /// <summary>
        /// 打印错误信息到文本文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="e">异常</param>
        private static void PrintErrorMessageToFile(Exception ex)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_fff"); // 毫秒防冲突
            string logsFolder = Path.Combine(DataFolder.Path, "logs");
            try
            {
                Directory.CreateDirectory(logsFolder);
                string logFile = Path.Combine(logsFolder, $"{time}.txt");

                using var sw = new StreamWriter(logFile, false, Encoding.UTF8);

                sw.WriteLine("==============System Info================");
                sw.WriteLine($"Time: {DateTime.Now:O}");
                sw.WriteLine($"Operating System: {Environment.OSVersion}");
                sw.WriteLine($".NET Version: {Environment.Version}");
                sw.WriteLine($"Mikoto Version: {Common.CurrentVersion}");

                sw.WriteLine();
                sw.WriteLine("==============Exception Info================");
                sw.WriteLine(ex.ToString());
                sw.Flush();


                //自动清理旧日志
                CleanupOldLogs(logsFolder, LogReserveDays);
            }
            catch (Exception logEx)
            {
                Console.Error.WriteLine("Logging failed: " + logEx);
            }
        }

        private static void CleanupOldLogs(string folder, int reserveDays)
        {
            try
            {
                var deadline = DateTime.Now.AddDays(-reserveDays);

                foreach (var file in Directory.EnumerateFiles(folder, "*.txt"))
                {
                    var lastWrite = File.GetLastWriteTime(file);
                    if (lastWrite < deadline)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Clean log failed: " + ex);
            }
        }

        /// <summary>
        /// 执行Hook是否完全卸载的检查
        /// </summary>
        private void EndHook()
        {
            GlobalWorkingData.Instance.TextHooker.Dispose();
        }
    }
}
