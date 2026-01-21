using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Mikoto.Core;
using Mikoto.Core.Interfaces;
using Mikoto.Core.Messages;
using Mikoto.Core.ViewModels;
using Mikoto.Core.ViewModels.AddGamePages;
using Mikoto.Fluent.AddGamePages;
using Mikoto.Fluent.Services;
using Serilog;
using System.Diagnostics.CodeAnalysis;

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

            // 2. 配置依赖注入容器
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // --- 注册服务 (接口 vs 实现) ---

                    // 注册环境配置 (你的 AppEnvironment 逻辑)
                    services.AddSingleton<IAppEnvironment>(new AppEnvironment());

                    // 注册线程服务 (这里的实现是 WinUI 特有的)
                    services.AddSingleton<IMainThreadService, WinUIMainThreadService>();

                    // 注册资源服务
                    services.AddSingleton<IResourceService, WinUIResourceService>();

                    // --- 注册 ViewModels (这样它们就能通过构造函数拿服务了) ---
                    RegisterNavigation<AddGameViewModel, AddGamePage>(services);
                    RegisterNavigation<PreProcessViewModel, PreProcessPage>(services);
                    RegisterNavigation<HookSettingsViewModel, HookSettingsPage>(services);
                    RegisterNavigation<LanguageViewModel, LanguagePage>(services);
                    RegisterNavigation<SelectProcessViewModel, SelectProcessPage>(services);
                    RegisterNavigation<MainViewModel, MainWindow>(services);
                    RegisterNavigation<HomeViewModel, HomePage>(services);
                    RegisterNavigation<TranslateViewModel, TranslatePage>(services);
                })
                .Build();

            Services = host.Services;
        }

        private void RegisterNavigation<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TVM,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TPage>(IServiceCollection services)
            where TVM : class
            where TPage : class
        {
            services.AddTransient<TVM>();
            _viewModelMap[typeof(TVM)] = typeof(TPage);
        }

        // 建立 VM -> Page 的映射关系
        private static Dictionary<Type, Type> _viewModelMap = new();
        internal static Type GetViewFromViewModel(Type vmType)
        {
            if (_viewModelMap.TryGetValue(vmType, out var viewType))
            {
                return viewType;
            }
            else
            {
                throw new ViewModelMappingNotFoundException(vmType);
            }
        }

        public static IServiceProvider Services { get; private set; } = default!;

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
