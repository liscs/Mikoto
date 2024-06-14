using DataAccessLibrary;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TextHookLibrary;
using Windows.Win32;

namespace MisakaTranslator.GuidePages.Hook
{
    /// <summary>
    /// ChooseGamePage.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseGamePage : Page
    {
        private readonly Dictionary<string, int> _processList = ProcessHelper.GetProcessList_Name_PID();
        private int _gamePid = -1;
        private List<Process> _sameNameGameProcessList = new();
        private static ChooseGameViewModel _viewModel = new();


        public ChooseGamePage()
        {
            InitializeComponent();
            DataContext = _viewModel;
            if (Common.IsAdmin)
            {
                NoAdminPrivilegesTextBlock.Visibility = Visibility.Collapsed;
            }

            GameProcessComboBox.ItemsSource = _processList.Keys.OrderBy(p => p);
        }

        private void GameProcessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _gamePid = _processList[(string)GameProcessComboBox.SelectedValue];
            _sameNameGameProcessList = ProcessHelper.FindSameNameProcess(_gamePid);
            AutoHookTag.Text = Application.Current.Resources["ChooseGamePage_AutoHookTag_Begin"].ToString() + _sameNameGameProcessList.Count + Application.Current.Resources["ChooseGamePage_AutoHookTag_End"].ToString();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gamePid != -1 && GameProcessComboBox.SelectedValue is string selectValueString)
            {
                GenerateHookerAndGotoNextStep(_processList[selectValueString]);
            }
            else
            {
                HandyControl.Controls.Growl.Info(Application.Current.Resources["ChooseGamePage_NextErrorHint"].ToString());
            }
        }

        private void GenerateHookerAndGotoNextStep(int pid)
        {
            if (_sameNameGameProcessList.Count == 1)
            {
                Common.TextHooker = new TextHookHandle(pid);
            }
            else
            {
                Common.TextHooker = new TextHookHandle(_sameNameGameProcessList);
            }

            bool isx64 = Is64BitProcess(_gamePid);
            if (Common.TextHooker.Init(isx64 ? Common.AppSettings.Textractor_Path64 : Common.AppSettings.Textractor_Path32))
            {
                Common.GameID = Guid.Empty;
                string filepath = ProcessHelper.FindProcessPath(_gamePid, isx64);
                if (!string.IsNullOrEmpty(filepath))
                {
                    GameInfoBuilder.Reset();
                    GameInfoBuilder.GameProcessId = _gamePid;
                    GameInfoBuilder.GameInfo = GameHelper.GetGameByPath(filepath);
                    Common.GameID = GameInfoBuilder.GameInfo.GameID;
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
            PInvoke.IsWow64Process((Windows.Win32.Foundation.HANDLE)Process.GetProcessById(pid).Handle, out Windows.Win32.Foundation.BOOL result);
            return !result;
        }

        private static unsafe int GetProcessIdFromFocus()
        {
            uint thisPid;
            PInvoke.GetWindowThreadProcessId(PInvoke.GetForegroundWindow(), &thisPid);
            while (true)
            {
                uint pid;
                if (PInvoke.GetWindowThreadProcessId(PInvoke.GetForegroundWindow(), &pid) != 0 && pid != thisPid)
                {
                    _viewModel.EnableSelectFocusButton = true;
                    return (int)pid;
                }
            }
        }
    }
}
