using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TextHookLibrary;

namespace MisakaTranslator.GuidePages.Hook
{
    /// <summary>
    /// ChooseHookFuncPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseHookFuncPage : Page
    {
        BindingList<TextHookData> lstData = new BindingList<TextHookData>();

        string LastCustomHookCode;

        int sum = 0;

        public ChooseHookFuncPage()
        {
            InitializeComponent();

            if (Common.IsAdmin)
            {
                NoAdminPrivilegesTextBlock.Visibility = Visibility.Collapsed;
            }

            LastCustomHookCode = null;

            HookFunListView.ItemsSource = lstData;
            sum = 0;
            Common.TextHooker!.HookMessageReceived += FilterAndDisplayData;
            _ = Common.TextHooker.StartHook(Convert.ToBoolean(Common.AppSettings.AutoHook));
        }

        public void FilterAndDisplayData(object sender, HookReceivedEventArgs e)
        {
            //加一步判断防止卡顿，部分不可能使用的方法刷新速度过快，在几秒之内就能刷新超过100个，这时候就停止对他们的刷新,直接卸载这个方法
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (InvalidMisakaCodeRegex().Match(e.Data.MisakaHookCode).Success)
                {
                    e.Data.MisakaHookCode = string.Empty;
                }
                if (e.Index < sum)
                {
                    lstData[e.Index] = e.Data;
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

                //先关闭对本窗口的输出
                Common.TextHooker!.HookMessageReceived -= FilterAndDisplayData;

                //先要将需要用到的方法注明，再进行后续卸载操作
                Common.TextHooker.HookCodeList.Add(lstData[HookFunListView.SelectedIndex].HookCode);
                Common.TextHooker.MisakaCodeList.Add(lstData[HookFunListView.SelectedIndex].MisakaHookCode);

                List<string> usedHook = new List<string>
                {
                    hookAdd
                };

                //用户开启了自动卸载
                if (Common.AppSettings.AutoDetach)
                {
                    Common.TextHooker.DetachUnrelatedHooks(pid, usedHook);
                }

                GameInfoBuilder.GameInfo.TransMode = 1;
                GameInfoBuilder.GameInfo.HookCode = lstData[HookFunListView.SelectedIndex].HookCode;
                GameInfoBuilder.GameInfo.MisakaHookCode = lstData[HookFunListView.SelectedIndex].MisakaHookCode;

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
                        GameInfoBuilder.GameInfo.HookCodeCustom = LastCustomHookCode;
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        //返回界面，否则会自动进入下一个界面
                        return;
                    }
                    else
                    {
                        //不记录特殊码，但也要写NULL
                        GameInfoBuilder.GameInfo.HookCodeCustom = null;
                    }
                }
                else
                {
                    GameInfoBuilder.GameInfo.HookCodeCustom = null;
                }

                //使用路由事件机制通知窗口来完成下一步操作
                PageChangeRoutedEventArgs args = new(PageChange.PageChangeRoutedEvent, this)
                {
                    XamlPath = "GuidePages/Hook/ChooseTextRepairFuncPage.xaml",
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
            if (PIDTextBox.Text != "" && HookCodeTextBox.Text != "" && int.TryParse(PIDTextBox.Text, out int pid))
            {
                _ = Common.TextHooker!.AttachProcessByHookCode(pid, HookCodeTextBox.Text);
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
                Common.TextHooker!.HookMessageReceived -= FilterAndDisplayData;
                HandyControl.Controls.Growl.Warning(Application.Current.Resources["ChooseHookFuncPage_PauseHint"].ToString());
            }

        }

        private void CannotfindHookBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/hanmin0822/MisakaHookFinder") { UseShellExecute = true });
        }

        [GeneratedRegex("【F+:F+:F+】")]
        private static partial Regex InvalidMisakaCodeRegex();

    }
}
