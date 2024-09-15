using HandyControl.Controls;
using Mikoto.DataAccess;
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
        private int _gamePid = -1;
        private List<Process> _sameNameGameProcessList = new();
        private static ChooseGamePageViewModel _viewModel = new();


        public ChooseGamePage()
        {
            InitializeComponent();
            DataContext = _viewModel;
            if (Environment.IsPrivilegedProcess)
            {
                NoAdminPrivilegesTextBlock.Visibility = Visibility.Collapsed;
            }

            GameProcessComboBox.ItemsSource = _appNamePidDict.Keys.OrderBy(p => p);
        }

        private void GameProcessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _gamePid = _appNamePidDict[(string)GameProcessComboBox.SelectedValue];
            _sameNameGameProcessList = ProcessHelper.FindSameNameProcess(_gamePid);
            AutoHookTag.Text = Application.Current.Resources["ChooseGamePage_AutoHookTag_Begin"].ToString() + _sameNameGameProcessList.Count + Application.Current.Resources["ChooseGamePage_AutoHookTag_End"].ToString();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gamePid != -1 && GameProcessComboBox.SelectedValue is string selectValueString)
            {
                try
                {
                    GenerateHookerAndGotoNextStep(_appNamePidDict[selectValueString]);
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
                GlobalWorkingData.Instance.TextHooker = new TextHookHandle(pid);
            }
            else
            {
                GlobalWorkingData.Instance.TextHooker = new TextHookHandle(_sameNameGameProcessList);
            }

            bool isx64 = Is64BitProcess(_gamePid);
            if (GlobalWorkingData.Instance.TextHooker.Init(isx64 ? Common.AppSettings.Textractor_Path64 : Common.AppSettings.Textractor_Path32))
            {
                GlobalWorkingData.Instance.GameID = Guid.Empty;
                string filepath = ProcessHelper.FindProcessPath(_gamePid);
                if (!string.IsNullOrEmpty(filepath))
                {
                    GameInfoBuilder.Reset();
                    GameInfoBuilder.GameProcessId = _gamePid;
                    GameInfoBuilder.GameInfo = GameHelper.GetGameByPath(filepath);
                    GlobalWorkingData.Instance.GameID = GameInfoBuilder.GameInfo.GameID;
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
