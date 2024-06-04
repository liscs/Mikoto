using DataAccessLibrary;
using System.Windows;
using System.Windows.Controls;

namespace MisakaTranslator.GuidePages
{
    /// <summary>
    /// CompletationPage.xaml 的交互逻辑
    /// </summary>
    public partial class CompletationPage : Page
    {
        public CompletationPage()
        {
            InitializeComponent();
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            //刷新主界面
            Dispatcher.BeginInvoke(() =>
            {
                MainWindow.Instance.GameInfoList = GameHelper.GetAllCompletedGames();
                MainWindow.Instance.Refresh();
            });

            //使用路由事件机制通知窗口来完成下一步操作
            PageChangeRoutedEventArgs args = new PageChangeRoutedEventArgs(PageChange.PageChangeRoutedEvent, this);
            this.RaiseEvent(args);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            PageChangeRoutedEventArgs args = new(PageChange.PageChangeRoutedEvent, this)
            {
                IsBack = true
            };
            this.RaiseEvent(args);
        }
    }
}
