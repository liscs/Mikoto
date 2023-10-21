using System.Windows;
using System.Windows.Controls;

namespace MisakaTranslator_WPF.GuidePages {
    /// <summary>
    /// CompletationPage.xaml 的交互逻辑
    /// </summary>
    public partial class CompletationPage : Page {
        public CompletationPage() {
            InitializeComponent();
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e) {
            //使用路由事件机制通知窗口来完成下一步操作
            PageChangeRoutedEventArgs args = new PageChangeRoutedEventArgs(PageChange.PageChangeRoutedEvent, this);
            args.XamlPath = "1";//表示完成
            this.RaiseEvent(args);
        }
    }
}
