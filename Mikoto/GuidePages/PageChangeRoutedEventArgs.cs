using System.Windows;
using System.Windows.Controls;

namespace Mikoto.GuidePages
{
    public class PageChangeRoutedEventArgs : RoutedEventArgs
    {
        public PageChangeRoutedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }

        public Page? Page { get; set; }
        public bool IsBack { get; internal set; }

        /// <summary>
        /// 部分方法需要用到的额外参数
        /// </summary>
        public object? ExtraArgs;
    }

    public class PageChange
    {
        public string? XamlPath;

        //声明和注册路由事件
        public static readonly RoutedEvent PageChangeRoutedEvent =
            EventManager.RegisterRoutedEvent("PageChange", RoutingStrategy.Bubble, typeof(EventHandler<PageChangeRoutedEventArgs>), typeof(PageChange));

    }


}
