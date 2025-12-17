using Mikoto.DataAccess;
using Mikoto.Enums;
using Mikoto.GuidePages;
using Mikoto.GuidePages.Hook;
using Mikoto.Helpers;
using System.Windows;

namespace Mikoto
{
    /// <summary>
    /// GameGuideWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GameGuideWindow
    {
        private readonly TransMode _transMode;
        private bool buildCompleted = false;//是否是在完成状态下退出的，作为检验，默认为假
        private readonly GameInfoBuilder _gameInfoBuilder = new GameInfoBuilder();

        public GameGuideWindow(TransMode mode)
        {
            InitializeComponent();

            this.AddHandler(PageChange.PageChangeRoutedEvent, new RoutedEventHandler(SwitchPage));

            _transMode = mode;
            switch (mode)
            {
                case TransMode.Hook:
                    {
                        //Hook模式
                        List<string> lstStep =
                        [
                            App.Env.ResourceService.Get("GameGuideWin_Hook_Step_1"),
                            App.Env.ResourceService.Get("GameGuideWin_Hook_Step_2"),
                            App.Env.ResourceService.Get("GameGuideWin_Hook_Step_3"),
                            App.Env.ResourceService.Get("GameGuideWin_Step_4"),
                            App.Env.ResourceService.Get("GameGuideWin_Step_5")
                        ];

                        GuideStepBar.ItemsSource = lstStep;
                        FuncHint.Text = App.Env.ResourceService.Get("GameGuideWin_FuncHint_Hook");
                        GuidePageFrame.Navigate(new ChooseGamePage(_gameInfoBuilder));
                        break;
                    }

                case TransMode.Clipboard:
                    {
                        //剪贴板监控
                        List<string> lstStep =
                        [
                            App.Env.ResourceService.Get("GameGuideWin_Hook_Step_3"),
                            App.Env.ResourceService.Get("GameGuideWin_Step_4"),
                            App.Env.ResourceService.Get("GameGuideWin_Step_5")
                        ];

                        GuideStepBar.ItemsSource = lstStep;
                        FuncHint.Text = App.Env.ResourceService.Get("GameGuideWin_FuncHint_ClipBoard");
                        GuidePageFrame.Navigate(new ChooseTextRepairFuncPage(_gameInfoBuilder));
                        break;
                    }
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
                App.Env.Context.TransMode = _transMode;
                switch (_transMode)
                {
                    case TransMode.Hook:
                        _gameInfoBuilder.GameInfo.LastPlayAt = DateTime.Now;
                        GameHelper.SaveGameInfo(_gameInfoBuilder.GameInfo);
                        MainWindow.Instance.RefreshAsync().FireAndForget();
                        break;
                    case TransMode.Clipboard:
                        break;
                }
                new TranslateWindow().Show();
                buildCompleted = true;
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
            if (!buildCompleted)
            {
                App.Env.TextHookService.Dispose();
            }
        }
    }
}