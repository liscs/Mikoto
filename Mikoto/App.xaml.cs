using Mikoto.DataAccess;
using Mikoto.Helpers.Input;
using Mikoto.Windows.Logger;
using Serilog;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace Mikoto
{
    public partial class App
    {
        public App()
        {
            ConfigureLogging();

            this.Startup += App_Startup;
            this.Exit += App_Exit;
        }

        public static AppEnvironment Env { get; private set; } = new AppEnvironment();

        private static void ConfigureLogging()
        {
            var logPath = Path.Combine(DataFolder.Path, "logs", "mikoto-.log");

            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug() // 调试模式下开启详细细节
#else
                .MinimumLevel.Information() // 发布模式下只保留重要信息，节省 IO 性能
#endif
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    encoding: Encoding.UTF8
                )
                .WriteTo.Sink(new CallbackSink(LogViewer.Sink))
                .CreateLogger();

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            GlobalKeyboardHook.Initialize();
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            // 统一异常处理注册
            this.DispatcherUnhandledException += (s, args) =>
            {
                args.Handled = true;
                HandleException(args.Exception, "UI线程异常");
            };

            TaskScheduler.UnobservedTaskException += (s, args) =>
            {
                args.SetObserved();
                HandleException(args.Exception, "任务线程异常");
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception
                    ?? new Exception("未知异常", args.ExceptionObject as Exception);
                HandleException(ex, "非UI线程异常");
            };

            // 异步事件未捕获异常（如 async void）
            SynchronizationContext.SetSynchronizationContext(new ExceptionSynchronizationContext());

            SetDefaultFrameRate();

            Log.Information("程序启动完成");
        }

        private static void SetDefaultFrameRate()
        {
            DEVMODEW dm = new();
            if (PInvoke.EnumDisplaySettings(null, ENUM_DISPLAY_SETTINGS_MODE.ENUM_CURRENT_SETTINGS, ref dm))
            {
                Timeline.DesiredFrameRateProperty.OverrideMetadata(
                    typeof(Timeline),
                    new FrameworkPropertyMetadata { DefaultValue = (int)dm.dmDisplayFrequency });
            }
            else
            {
                Timeline.DesiredFrameRateProperty.OverrideMetadata(
                    typeof(Timeline),
                    new FrameworkPropertyMetadata { DefaultValue = 60 });
                Log.Warning("获取系统刷新率失败，使用默认 60Hz");
            }
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            Log.Information("程序退出中");
            CloseHooksSafe();
            Log.CloseAndFlush();
        }

        /// <summary>
        /// 统一异常处理方法
        /// </summary>
        private static void HandleException(Exception ex, string title)
        {
            try
            {
                Log.Fatal(ex, "{Title}：程序发生未处理异常", title);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Mikoto.MainWindow.Instance.CloseNotifyIcon();
                    ShowExceptionMessageBox(ex);
                });

                CloseHooksSafe();
            }
            catch (Exception ex2)
            {
                Log.Error(ex2, "异常处理执行失败");
            }
        }

        private static void ShowExceptionMessageBox(Exception ex)
        {
            string logFile = GetLatestLogFile();

#if DEBUG
            string message = $"{Current.Resources["App_Global_ErrorHint_left"]}{logFile}{Current.Resources["App_Global_ErrorHint_right"]}{Environment.NewLine}{ex}";
#else
    string message = $"{Current.Resources["App_Global_ErrorHint_left"]}{logFile}{Current.Resources["App_Global_ErrorHint_right"]}";
#endif

            MessageBox.Show(
                message,
                Current.Resources["MessageBox_Error"].ToString(),
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        private static string GetLatestLogFile()
        {
            string logFolder = Path.Combine(DataFolder.Path, "logs");
            if (!Directory.Exists(logFolder)) return logFolder; // 返回目录

            var files = Directory.GetFiles(logFolder, "*.log");
            if (files.Length == 0) return logFolder;

            // 按最后写入时间排序，获取最新日志文件
            var latestFile = files.OrderByDescending(File.GetLastWriteTime).First();
            return latestFile;
        }



        private static void CloseHooksSafe()
        {
            try
            {
                Env.TextHookService.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "释放文本Hook服务失败");
            }
        }

        /// <summary>
        /// 自定义同步上下文，用于捕获 async void 异常
        /// </summary>
        private class ExceptionSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback d, object? state)
            {
                base.Post(s =>
                {
                    try
                    {
                        d(s);
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex, "异步事件异常 (async void)");
                    }
                }, state);
            }

            public override void Send(SendOrPostCallback d, object? state)
            {
                try
                {
                    base.Send(d, state);
                }
                catch (Exception ex)
                {
                    HandleException(ex, "同步事件异常");
                }
            }
        }
    }
}
