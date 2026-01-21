using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Mikoto.Core.Messages;
using Mikoto.Core.ViewModels;
using Mikoto.Core.ViewModels.AddGamePages;
using Mikoto.Fluent.AddGamePages;
using System.Diagnostics;

namespace Mikoto.Fluent;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    MainViewModel ViewModel = App.Services.GetRequiredService<MainViewModel>();
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
            var pageType = App.GetViewFromViewModel(m.ViewModelType);
            ContentFrame.Navigate(pageType, m.Parameter);
        });

        WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(HomeViewModel)));

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
