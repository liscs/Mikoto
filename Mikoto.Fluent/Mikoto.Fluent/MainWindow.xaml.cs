using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mikoto.Fluent
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        MainViewModel ViewModel = new MainViewModel();
        public MainWindow()
        {
            InitializeComponent();

            // 监听 Frame 的导航完成事件
            ContentFrame.Navigated += (s, e) =>
            {
                // 核心逻辑：只要 Frame 能后退，按钮就亮起
                RootNav.IsBackEnabled = ContentFrame.CanGoBack;
            };

            // 监听导航消息
            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                ContentFrame.Navigate(m.PageType);
            });

            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(HomePage)));

        }

        // 处理左上角返回按钮
        private void RootNav_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }
    }
}
