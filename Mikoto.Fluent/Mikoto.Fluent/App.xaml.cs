using Microsoft.UI.Xaml;
using Serilog;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mikoto.Fluent
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // 必须加上这一行，否则 Debug 级别的日志会被丢弃
                .WriteTo.Debug()      // 输出到 VS 的 Debug 窗口
                .CreateLogger();
            InitializeComponent();
            this.UnhandledException += (s, e) =>
            {
                var message = e.Message;
                System.Diagnostics.Debug.WriteLine($"Unhandled Error: {message}");
            };
        }

        public static AppEnvironment Env { get; } = new AppEnvironment();

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }
    }
}
