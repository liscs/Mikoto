using System.IO;
using System.Reflection;
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
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            PrintErrorMessageToFile(nowTime, e.Exception);
            ShowExceptionMessageBox(e.Exception, nowTime);
        }

        private static void ShowExceptionMessageBox(object e, string nowTime)
        {
            MessageBox.Show($"{Current.Resources["App_Global_ErrorHint_left"]}{nowTime}{Current.Resources["App_Global_ErrorHint_right"]}{Environment.NewLine}{e}",
                            Current.Resources["MessageBox_Error"].ToString());
        }

        /// <summary>
        /// 非UI线程未捕获异常处理事件
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Mikoto.MainWindow.Instance.CloseNotifyIcon();
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            PrintErrorMessageToFile(nowTime, e.ExceptionObject);
            EndHook();
            ShowExceptionMessageBox(e.ExceptionObject, nowTime);
        }

        /// <summary>
        /// Task线程内未捕获异常处理事件
        /// </summary>
        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Mikoto.MainWindow.Instance.CloseNotifyIcon();
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            PrintErrorMessageToFile(nowTime, e.Exception);
            ShowExceptionMessageBox(e.Exception, nowTime);
        }

        /// <summary>
        /// 打印错误信息到文本文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="e">异常</param>
        private static void PrintErrorMessageToFile(string fileName, object e)
        {
            if (!Directory.Exists($"{Environment.CurrentDirectory}\\data\\logs"))
            {
                Directory.CreateDirectory($"{Environment.CurrentDirectory}\\data\\logs");
            }
            using FileStream fs = new($"{Environment.CurrentDirectory}\\data\\logs\\{fileName}.txt", FileMode.Create);

            using StreamWriter sw = new(fs);

            sw.WriteLine("==============System Info================");
            sw.WriteLine("System:" + Environment.OSVersion);
            sw.WriteLine("CurrentTime:" + DateTime.Now.ToString("g"));
            sw.WriteLine("dotNetVersion:" + Environment.Version);
            Version version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version();
            sw.WriteLine("MikotoVersion:" + version.ToString());

            sw.WriteLine("==============Exception Info================");
            sw.WriteLine(e);

            Exception? exceptionPointer = e as Exception;
            while (exceptionPointer != null && exceptionPointer.InnerException != null)
            {
                sw.WriteLine("InnerException:" + exceptionPointer.InnerException);
                exceptionPointer = exceptionPointer.InnerException;
            }

            sw.Flush();
            sw.Close();
            fs.Close();
        }


        /// <summary>
        /// 执行Hook是否完全卸载的检查
        /// </summary>
        private void EndHook()
        {
            if (Common.TextHooker != null)
            {
                Common.TextHooker = null;
                GC.Collect();
            }
        }
    }
}
