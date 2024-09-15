using Mikoto.DataAccess;
using Mikoto.Enums;
using Mikoto.GuidePages;
using Mikoto.GuidePages.Hook;
using System.Windows;

namespace Mikoto
{
    /// <summary>
    /// GameGuideWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GameGuideWindow
    {
        private TransMode _transMode;
        private bool isComplete;//是否是在完成状态下退出的，作为检验，默认为假

        public GameGuideWindow(TransMode Mode)
        {
            InitializeComponent();

            this.AddHandler(PageChange.PageChangeRoutedEvent, new RoutedEventHandler(SwitchPage));

            isComplete = false;
            _transMode = Mode;
            if (Mode == TransMode.Hook)
            {
                //Hook模式
                List<string> lstStep = new List<string>()
                {
                Application.Current.Resources["GameGuideWin_Hook_Step_1"].ToString()!,
                Application.Current.Resources["GameGuideWin_Hook_Step_2"].ToString()!,
                Application.Current.Resources["GameGuideWin_Hook_Step_3"].ToString()!,
                Application.Current.Resources["GameGuideWin_Step_4"].ToString()!,
                Application.Current.Resources["GameGuideWin_Step_5"].ToString()!
                };

                GuideStepBar.ItemsSource = lstStep;
                FuncHint.Text = Application.Current.Resources["GameGuideWin_FuncHint_Hook"].ToString();
                GuidePageFrame.Navigate(new ChooseGamePage());
            }
            else if (Mode == TransMode.Clipboard)
            {
                //剪贴板监控
                List<string> lstStep = new List<string>()
                {
                Application.Current.Resources["GameGuideWin_Hook_Step_3"].ToString()!,
                Application.Current.Resources["GameGuideWin_Step_4"].ToString()!,
                Application.Current.Resources["GameGuideWin_Step_5"].ToString()!
                };

                GuideStepBar.ItemsSource = lstStep;
                FuncHint.Text = Application.Current.Resources["GameGuideWin_FuncHint_ClipBoard"].ToString();
                GuidePageFrame.Navigate(new ChooseTextRepairFuncPage());
            }
        }

        /// <summary>
        /// 触发进入下一步的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchPage(object sender, RoutedEventArgs e)
        {
            PageChangeRoutedEventArgs args = (e as PageChangeRoutedEventArgs)!;
            if (args.IsBack)
            {
                GuidePageFrame.NavigationService.GoBack();
                GuideStepBar.Prev();
            }
            else if (args.Page == null)
            {
                switch (_transMode)
                {
                    case TransMode.Hook:
                        GlobalWorkingData.Instance.TransMode = TransMode.Hook;
                        GameInfoBuilder.GameInfo.LastPlayAt = DateTime.Now;
                        GameHelper.SaveGameInfo(GameInfoBuilder.GameInfo);
                        break;
                    case TransMode.Clipboard:
                        GlobalWorkingData.Instance.TransMode = TransMode.Clipboard;
                        break;
                }
                new TranslateWindow().Show();
                isComplete = true;
                this.Close();
            }
            else
            {
                //其他情况就跳转指定页面
                GuidePageFrame.NavigationService.Navigate(args.Page);
                GuideStepBar.Next();
            }

        }

        private void GuideWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isComplete)
            {
                GlobalWorkingData.Instance.TextHooker.Dispose();
            }
        }
    }
}