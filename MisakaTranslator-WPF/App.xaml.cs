using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace MisakaTranslator
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
            DoHookCheck();
        }

        /// <summary>
        /// UI线程未捕获异常处理事件
        /// </summary>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MisakaTranslator.MainWindow.Instance.CloseNotifyIcon();
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            PrintErrorMessageToFile(nowTime, e.Exception, 0);
            DoHookCheck();
            ShowExceptionMessageBox(e.Exception, nowTime);
        }

        private static void ShowExceptionMessageBox(object e, string nowTime)
        {
            if (e is Exception ex)
            {
                MessageBox.Show($"{Current.Resources["App_Global_ErrorHint_left"]}{nowTime}{Current.Resources["App_Global_ErrorHint_right"]}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}"
                    , Current.Resources["MessageBox_Error"].ToString());
            }
            else
            {
                MessageBox.Show($"{Current.Resources["App_Global_ErrorHint_left"]}{nowTime}{Current.Resources["App_Global_ErrorHint_right"]}{Environment.NewLine}{e}"
                , Current.Resources["MessageBox_Error"].ToString());
            }
        }

        /// <summary>
        /// 非UI线程未捕获异常处理事件
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MisakaTranslator.MainWindow.Instance.CloseNotifyIcon();
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            if (e.ExceptionObject is Exception exception)
            {
                PrintErrorMessageToFile(nowTime, exception, 1);
            }
            else
            {
                PrintErrorMessageToFile(nowTime, null, 1, e.ExceptionObject.ToString());
            }

            DoHookCheck();
            ShowExceptionMessageBox((e.ExceptionObject as Exception) ?? e.ExceptionObject, nowTime);

        }

        /// <summary>
        /// Task线程内未捕获异常处理事件
        /// </summary>
        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            MisakaTranslator.MainWindow.Instance.CloseNotifyIcon();
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            PrintErrorMessageToFile(nowTime, e.Exception, 2);

            DoHookCheck();
            ShowExceptionMessageBox(e.Exception, nowTime);

        }

        /// <summary>
        /// 打印错误信息到文本文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="e">异常</param>
        /// <param name="exceptionThread">异常线程</param>
        /// <param name="errorMessage">错误消息</param>
        private static void PrintErrorMessageToFile(string fileName, Exception? e, int exceptionThread, string? errorMessage = null)
        {
            if (!Directory.Exists($"{Environment.CurrentDirectory}\\data\\logs"))
            {
                Directory.CreateDirectory($"{Environment.CurrentDirectory}\\data\\logs");
            }
            FileStream fs = new FileStream($"{Environment.CurrentDirectory}\\data\\logs\\{fileName}.txt", FileMode.Create);

            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine("==============System Info================");
            sw.WriteLine("System:" + Environment.OSVersion);
            sw.WriteLine("CurrentTime:" + DateTime.Now.ToString("g"));
            sw.WriteLine("dotNetVersion:" + Environment.Version);
            Version version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version();
            sw.WriteLine("MisakaTranslatorVersion:" + version.ToString());

            if (errorMessage != null)
            {
                sw.WriteLine("==============Exception Info================");
                sw.WriteLine("ExceptionType:" + "Non UI Thread But not Exception");
                sw.WriteLine("ErrorMessage:" + errorMessage);
            }
            else
            {
                sw.WriteLine("==============Exception Info================");
                switch (exceptionThread)
                {
                    case 0:
                        sw.WriteLine("ExceptionType:" + "UI Thread");
                        break;
                    case 1:
                        sw.WriteLine("ExceptionType:" + "Non UI Thread");
                        break;
                    case 2:
                        sw.WriteLine("ExceptionType:" + "Task Thread");
                        break;
                }
                if (e != null)
                {
                    sw.WriteLine("ExceptionName:" + e.GetType());
                    sw.WriteLine("ExceptionSource:" + e.Source);
                    sw.WriteLine("ExceptionMessage:" + e.Message);
                    sw.WriteLine("ExceptionStackTrace:" + e.StackTrace);
                    if (e.InnerException != null)
                        sw.WriteLine("InnerExceptionStackTrace:" + e.InnerException);
                }
            }

            sw.Flush();
            sw.Close();
            fs.Close();
        }


        /// <summary>
        /// 执行Hook是否完全卸载的检查
        /// </summary>
        public void DoHookCheck()
        {
            if (Common.TextHooker != null)
            {
                Common.TextHooker = null;
                GC.Collect();
            }
        }
    }
}
