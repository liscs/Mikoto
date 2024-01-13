using DataAccessLibrary;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using TextHookLibrary;

namespace MisakaTranslator.GuidePages.Hook
{
    /// <summary>
    /// ChooseGamePage.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseGamePage : Page
    {
        private readonly Dictionary<string, int> _processList = ProcessHelper.GetProcessList_Name_PID();
        private int _gamePid = -1;
        private List<System.Diagnostics.Process> _sameNameGameProcessList = new();

        public ChooseGamePage()
        {
            InitializeComponent();

            if (IsAdmin())
            {
                NoAdminPrivilegesTextBlock.Visibility = Visibility.Collapsed;
            }

            GameProcessCombox.ItemsSource = _processList.Keys;
        }

        private static bool IsAdmin()
        {
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            return isElevated;
        }

        private void GameProcessCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _gamePid = _processList[(string)GameProcessCombox.SelectedValue];
            _sameNameGameProcessList = ProcessHelper.FindSameNameProcess(_gamePid);
            AutoHookTag.Text = Application.Current.Resources["ChooseGamePage_AutoHookTag_Begin"].ToString() + _sameNameGameProcessList.Count + Application.Current.Resources["ChooseGamePage_AutoHookTag_End"].ToString();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gamePid != -1)
            {
                if (_sameNameGameProcessList.Count == 1)
                {
                    Common.TextHooker = new TextHookHandle(_processList[(string)GameProcessCombox.SelectedValue]);
                }
                else
                {
                    Common.TextHooker = new TextHookHandle(_sameNameGameProcessList);
                }

                if (!Common.TextHooker.Init(x64GameCheckBox.IsChecked ?? false ? Common.AppSettings.Textractor_Path64 : Common.AppSettings.Textractor_Path32))
                {
                    HandyControl.Controls.MessageBox.Show(Application.Current.Resources["MainWindow_TextractorError_Hint"].ToString());
                    return;
                }

                Common.GameID = Guid.Empty;
                GameInfo targetGame;
                string filepath = ProcessHelper.FindProcessPath(_gamePid, x64GameCheckBox.IsChecked ?? false);
                if (filepath != "")
                {
                    targetGame = GameHelper.GetGameByPath(filepath);
                    Common.GameID = targetGame.GameID;
                    targetGame.Isx64 = x64GameCheckBox.IsChecked ?? false;
                    GameHelper.SaveGameInfo(targetGame);
                }

                //使用路由事件机制通知窗口来完成下一步操作
                PageChangeRoutedEventArgs args = new(PageChange.PageChangeRoutedEvent, this)
                {
                    XamlPath = "GuidePages/Hook/ChooseHookFuncPage.xaml"
                };
                this.RaiseEvent(args);
            }
            else
            {
                HandyControl.Controls.Growl.Error(Application.Current.Resources["ChooseGamePage_NextErrorHint"].ToString());
            }

        }
    }
}
