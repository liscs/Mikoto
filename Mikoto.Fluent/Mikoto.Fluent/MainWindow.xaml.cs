using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Mikoto.Core.Models;
using Mikoto.Core.ViewModels;
using Mikoto.Fluent.TranslatorSettingPages;

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
        // 1. 让内容延伸进标题栏
        ExtendsContentIntoTitleBar = true;
        // 2. 将侧边栏的空白区域设为标题栏的可拖拽区域
        SetTitleBar(AppTitleBar);

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
        WeakReferenceMessenger.Default.Register<SetNavigationViewMessage>(this, (r, m) =>
        {
            var pageType = App.GetViewFromViewModel(m.ViewModelType);

            ContentFrame.Navigate(pageType, m.Parameter);
            if (ContentFrame.BackStack.Count > 0)
            {
                ContentFrame.BackStack.RemoveAt(ContentFrame.BackStack.Count - 1);
            }
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

    private void RootNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        // 1. 判断是否点击了系统内置的“设置”按钮
        if (args.IsSettingsInvoked)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        // 2. 获取被点击容器的 Tag
        var itemTag = args.InvokedItemContainer?.Tag?.ToString();

        switch (itemTag)
        {
            case "TranslateAPI":
                // 跳转到翻译 API 对应的页面
                ContentFrame.Navigate(typeof(TranslatorSettingPage));
                break;

            case "OtherPage":
                // ContentFrame.Navigate(typeof(OtherPage));
                break;
        }
    }
}
