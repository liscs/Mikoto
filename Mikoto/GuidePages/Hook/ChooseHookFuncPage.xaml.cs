using Mikoto.Helpers;
using Mikoto.ProcessInterop;
using Mikoto.TextHook;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mikoto.GuidePages.Hook
{
    /// <summary>
    /// ChooseHookFuncPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseHookFuncPage : Page
    {
        private BindingList<TextHookData> lstData = new BindingList<TextHookData>();
        private string? LastCustomHookCode;
        private int sum = 0;
        private GameInfoBuilder _gameInfoBuilder;

        public ChooseHookFuncPage(GameInfoBuilder gameInfoBuilder)
        {
            InitializeComponent();
            _gameInfoBuilder = gameInfoBuilder;
            if (Environment.IsPrivilegedProcess)
            {
                NoAdminPrivilegesTextBlock.Visibility = Visibility.Collapsed;
            }

            LastCustomHookCode = null;

            HookFunListView.ItemsSource = lstData;
            sum = 0;
            App.Env.TextHookService.HookMessageReceived += FilterAndDisplayData;
            App.Env.TextHookService.StartHookAsync(_gameInfoBuilder.GameInfo, Convert.ToBoolean(Common.AppSettings.AutoHook)).FireAndForget();
        }

        public void FilterAndDisplayData(object sender, HookReceivedEventArgs e)
        {
            //加一步判断防止卡顿，部分不可能使用的方法刷新速度过快，在几秒之内就能刷新超过100个，这时候就停止对他们的刷新,直接卸载这个方法
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (InvalidMisakaCodeRegex().IsMatch(e.Data.MisakaHookCode))
                {
                    e.Data.MisakaHookCode = string.Empty;
                }
                if (e.Index < sum)
                {
                    var index = HookFunListView.SelectedIndex;
                    lstData[e.Index] = e.Data;
                    HookFunListView.SelectedIndex = index;
                }
                else
                {
                    lstData.Add(e.Data);
                    sum++;
                }
            }, System.Windows.Threading.DispatcherPriority.DataBind);


        }

        private void AddHookBtn_Click(object sender, RoutedEventArgs e)
        {
            InputDrawer.IsOpen = true;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (HookFunListView.SelectedIndex != -1)
            {
                string hookAdd = lstData[HookFunListView.SelectedIndex].HookAddress;
                int pid = lstData[HookFunListView.SelectedIndex].GamePID;
                App.Env.TextHookService.HookMessageReceived -= FilterAndDisplayData;

                List<string> usedHook = new List<string>
                {
                    hookAdd
                };

                //用户开启了自动卸载
                if (Common.AppSettings.AutoDetach)
                {
                    App.Env.TextHookService.DetachUnrelatedHooks(pid, usedHook);
                }

                _gameInfoBuilder.GameInfo.TransMode = 1;
                _gameInfoBuilder.GameInfo.HookCode = lstData[HookFunListView.SelectedIndex].HookCode;
                _gameInfoBuilder.GameInfo.MisakaHookCode = lstData[HookFunListView.SelectedIndex].MisakaHookCode;

                if (LastCustomHookCode != null)
                {
                    MessageBoxResult result = HandyControl.Controls.MessageBox.Show(
                        Application.Current.Resources["ChooseHookFuncPage_MBOX_hookcodeConfirm_left"] + "\n" + LastCustomHookCode + "\n" + Application.Current.Resources["ChooseHookFuncPage_MBOX_hookcodeConfirm_right"],
                        Application.Current.Resources["MessageBox_Ask"].ToString(),
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        //记录这个特殊码到数据库
                        _gameInfoBuilder.GameInfo.HookCodeCustom = LastCustomHookCode;
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        //返回界面，否则会自动进入下一个界面
                        return;
                    }
                    else
                    {
                        //不记录特殊码，但也要写NULL
                        _gameInfoBuilder.GameInfo.HookCodeCustom = null;
                    }
                }
                else
                {
                    _gameInfoBuilder.GameInfo.HookCodeCustom = null;
                }

                //使用路由事件机制通知窗口来完成下一步操作
                PageChangeRoutedEventArgs args = new(PageChange.PageChangeRoutedEvent, this)
                {
                    Page = new ChooseTextRepairFuncPage(_gameInfoBuilder),
                };
                this.RaiseEvent(args);
            }
            else
            {
                HandyControl.Controls.Growl.Error(Application.Current.Resources["ChooseHookFuncPage_NextErrorHint"].ToString());
            }


        }

        private void HookCodeConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (HookCodeTextBox.Text != "")
            {
                _ = App.Env.TextHookService.AttachProcessByHookCodeAsync(_gameInfoBuilder.GameProcessId, HookCodeTextBox.Text);
                LastCustomHookCode = HookCodeTextBox.Text;
                InputDrawer.IsOpen = false;
                HandyControl.Controls.Growl.Info(Application.Current.Resources["ChooseHookFuncPage_HookApplyHint"].ToString());
            }
            else
            {
                HandyControl.Controls.MessageBox.Show(Application.Current.Resources["ChooseHookFuncPage_HookApplyErrorHint"].ToString(), Application.Current.Resources["MessageBox_Error"].ToString());
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            InputDrawer.IsOpen = false;
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                App.Env.TextHookService.HookMessageReceived -= FilterAndDisplayData;
                HandyControl.Controls.Growl.Warning(Application.Current.Resources["ChooseHookFuncPage_PauseHint"].ToString());
            }

        }

        private void CannotfindHookBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.ShellStart("https://github.com/hanmin0822/MisakaHookFinder");
        }

        [GeneratedRegex("【0:FFFFFFFFFFFFFFFF:FFFFFFFFFFFFFFFF】|【FFFFFFFFFFFFFFFF:FFFFFFFFFFFFFFFF:FFFFFFFFFFFFFFFF】")]
        private static partial Regex InvalidMisakaCodeRegex();

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
