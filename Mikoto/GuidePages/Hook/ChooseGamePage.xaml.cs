using HandyControl.Controls;
using Mikoto.DataAccess;
using Mikoto.Helpers.Graphics;
using Mikoto.TextHook;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Windows.Win32;



namespace Mikoto.GuidePages.Hook
{
    /// <summary>
    /// ChooseGamePage.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseGamePage : Page
    {
        private readonly Dictionary<string, int> _appNamePidDict = ProcessHelper.GetAppNamePidDict();
        private List<Process> _sameNameGameProcessList = new();
        private readonly ChooseGamePageViewModel _viewModel = new();


        public ChooseGamePage()
        {
            InitializeComponent();
            DataContext = _viewModel;
            if (Environment.IsPrivilegedProcess)
            {
                NoAdminPrivilegesTextBlock.Visibility = Visibility.Collapsed;
            }
            var list = _appNamePidDict.Keys.OrderBy(p => p)
                                       .Select(p =>
                                               new ProcessItem
                                               {
                                                   DisplayName = p,
                                                   Icon=ImageHelper.GetGameIconSource(_appNamePidDict[p]),
                                                   PID = _appNamePidDict[p],
                                               })
                                       .ToList();
            _viewModel.ProcessList = new(list);
        }

        private void GameProcessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _sameNameGameProcessList = ProcessHelper.FindSameNameProcess(_viewModel.SelectedProcess.PID);
            AutoHookTag.Text = Application.Current.Resources["ChooseGamePage_AutoHookTag_Begin"].ToString() + _sameNameGameProcessList.Count + Application.Current.Resources["ChooseGamePage_AutoHookTag_End"].ToString();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedProcess.PID != -1)
            {
                try
                {
                    GenerateHookerAndGotoNextStep(_viewModel.SelectedProcess.PID);
                }
                catch (Win32Exception ex)
                {
                    Growl.Warning(ex.Message.ToString());
                }
            }
            else
            {
                Growl.Info(Application.Current.Resources["ChooseGamePage_NextErrorHint"].ToString());
            }
        }

        private void GenerateHookerAndGotoNextStep(int pid)
        {
            if (_sameNameGameProcessList.Count == 1)
            {
                App.Env.TextHookService = new TextHook.TextHookService(pid);
            }
            else
            {
                App.Env.TextHookService = new TextHook.TextHookService(_sameNameGameProcessList, new MaxMemoryProcessSelector());
            }

            bool isx64 = Is64BitProcess(pid);
            if (App.Env.TextHookService.Init(isx64 ? Common.AppSettings.Textractor_Path64 : Common.AppSettings.Textractor_Path32))
            {
                App.Env.Context.GameID = Guid.Empty;
                string filepath = ProcessHelper.FindProcessPath(pid);
                if (!string.IsNullOrEmpty(filepath))
                {
                    GameInfoBuilder.Reset();
                    GameInfoBuilder.GameProcessId = pid;
                    GameInfoBuilder.GameInfo = GameHelper.GetGameByPath(filepath);
                    App.Env.Context.GameID = GameInfoBuilder.GameInfo.GameID;
                    GameInfoBuilder.GameInfo.Isx64 = isx64;

                    //使用路由事件机制通知窗口来完成下一步操作
                    PageChangeRoutedEventArgs args = new(PageChange.PageChangeRoutedEvent, this)
                    {
                        Page = new ChooseHookFuncPage(),
                    };
                    this.RaiseEvent(args);
                }
            }
            else
            {
                HandyControl.Controls.MessageBox.Show(Application.Current.Resources["MainWindow_TextractorError_Hint"].ToString());
            }
        }

        private static bool Is64BitProcess(int pid)
        {
            PInvoke.IsWow64Process((global::Windows.Win32.Foundation.HANDLE)Process.GetProcessById(pid).Handle, out global::Windows.Win32.Foundation.BOOL result);
            return !result;
        }

    }
}
