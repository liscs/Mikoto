using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TextHookLibrary;

namespace MisakaTranslator.GuidePages.Hook
{
    /// <summary>
    /// ReChooseHookFuncPage.xaml 的交互逻辑
    /// </summary>
    public partial class ReChooseHookFuncPage : Page
    {
        private BindingList<TextHookData> lstData = new BindingList<TextHookData>();
        private int sum = 0;

        public ReChooseHookFuncPage()
        {
            InitializeComponent();
            HookFunListView.ItemsSource = lstData;
            sum = 0;
            Common.TextHooker!.MeetHookCodeMessageReceived += FilterAndDisplayData;
            _ = Common.TextHooker.StartHook(Common.AppSettings.AutoHook);
            var task_1 = System.Threading.Tasks.Task.Run(async delegate
            {
                await System.Threading.Tasks.Task.Delay(3000);
                _ = Common.TextHooker.AutoAddCustomHookToGameAsync();
            });

        }

        public void FilterAndDisplayData(object sender, HookReceivedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (e.Index < sum)
                {
                    lstData[e.Index] = e.Data;
                }
                else
                {
                    lstData.Add(e.Data);
                    sum++;
                }
            });
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (HookFunListView.SelectedIndex != -1)
            {
                //先关闭对本窗口的输出
                Common.TextHooker!.MeetHookCodeMessageReceived -= FilterAndDisplayData;

                Common.TextHooker.MisakaCodeList.Add(lstData[HookFunListView.SelectedIndex].MisakaHookCode);

                //用户开启了自动卸载
                if (Common.AppSettings.AutoDetach)
                {
                    List<string> usedHook = new List<string>();
                    usedHook.Add(lstData[HookFunListView.SelectedIndex].HookAddress);
                    Common.TextHooker.DetachUnrelatedHooks(lstData[HookFunListView.SelectedIndex].GamePID, usedHook);
                }


                //使用路由事件机制通知窗口来完成下一步操作
                PageChangeRoutedEventArgs args = new PageChangeRoutedEventArgs(PageChange.PageChangeRoutedEvent, this);
                args.XamlPath = "GuidePages/CompletationPage.xaml";
                this.RaiseEvent(args);
            }

        }
    }
}
