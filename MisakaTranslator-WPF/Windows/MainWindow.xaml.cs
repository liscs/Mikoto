using Config.Net;
using DataAccessLibrary;
using HandyControl.Controls;
using MisakaTranslator.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TextHookLibrary;
using TextRepairLibrary;
using TranslatorLibrary;
using MessageBox = HandyControl.Controls.MessageBox;

namespace MisakaTranslator
{
    public partial class MainWindow
    {
        public List<GameInfo> GameInfoList { get; set; } = new();
        private int _gid; //当前选中的顺序，并非游戏ID
        private IntPtr _hwnd;
        public ObservableCollection<Border> GamePanelCollection { get; set; } = new();

        public static MainWindow Instance { get; set; } = default!;

        public MainWindow()
        {
            DataContext = GamePanelCollection;
            Instance = this;
            Common.AppSettings = new ConfigurationBuilder<IAppSettings>().UseIniFile($"{Environment.CurrentDirectory}\\data\\settings\\settings.ini").Build();
            InitializeLanguage();
            TranslatorCommon.Refresh();
            InitializeComponent();
            Refresh();
            GrowlDisableSwitch();

            //注册全局OCR热键
            this.SourceInitialized += MainWindow_SourceInitialized;
            if (Common.IsAdmin)
            {
                RestartAsAdminBtn.Visibility = Visibility.Collapsed;
            }
        }

        private static void InitializeLanguage()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Common.AppSettings.AppLanguage);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Common.AppSettings.AppLanguage);
            ResourceDictionary languageResource = new ResourceDictionary();
            languageResource.Source = new Uri($"lang/{Common.AppSettings.AppLanguage}.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries[1] = languageResource;
        }

        //按下快捷键时被调用的方法
        public static void GlobalOcrHotKey_Pressed()
        {
            Common.GlobalOCR();
        }

        public void Refresh()
        {
            GameInfoList = GameHelper.GetAllCompletedGames();
            Common.RepairSettings = new ConfigurationBuilder<IRepeatRepairSettings>().UseIniFile(Environment.CurrentDirectory + "\\data\\settings\\RepairSettings.ini").Build();
            InitGameLibraryPanel();
        }

        /// <summary>
        /// 游戏库瀑布流初始化
        /// </summary>
        private void InitGameLibraryPanel()
        {
            GamePanelCollection.Clear();
            for (var i = 0; i < GameInfoList.Count; i++)
            {
                AddGame(i);
            }
            InitAddGamePanel();
        }

        private void AddGame(int gid)
        {
            TextBlock tb = new()
            {
                Text = GameInfoList[gid].GameName,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(3),
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.6,
                FontWeight = FontWeights.SemiBold,
            };
            tb.Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryForeground"];
            Image ico = ImageHelper.GetGameIcon(GameInfoList[gid].FilePath);
            RenderOptions.SetBitmapScalingMode(ico, BitmapScalingMode.HighQuality);
            var gd = new Grid();
            gd.Children.Add(ico);
            gd.Children.Add(tb);
            var back = new Border()
            {
                CornerRadius = new CornerRadius(1),
                Name = "game" + gid,
                Width = 150,
                Child = gd,
                Margin = new Thickness(3),
                Background = ImageHelper.GetMajorBrush(ico.Source as BitmapSource)
            };

            back.MouseEnter += Border_MouseEnter;
            back.MouseLeave += Border_MouseLeave;
            back.MouseLeftButtonDown += Back_MouseLeftButtonDown;
            GamePanelCollection.Add(back);
        }

        private void InitAddGamePanel()
        {
            var textBlock = new TextBlock()
            {
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(3)
            };
            textBlock.SetResourceReference(TextBlock.TextProperty, "MainWindow_ScrollViewer_AddNewGame");
            var grid = new Grid();
            grid.Children.Add(textBlock);
            var border = new Border()
            {
                Name = "AddNewName",
                Width = 150,
                Child = grid,
                Margin = new Thickness(3),
                Background = (SolidColorBrush)Application.Current.Resources["BoxBtnColor"]
            };
            border.MouseEnter += Border_MouseEnter;
            border.MouseLeave += Border_MouseLeave;
            border.MouseLeftButtonDown += Border_MouseLeftButtonDown;
            GamePanelCollection.Add(border);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddNewGameDrawer.IsOpen = true;
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            _hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(_hwnd)?.AddHook(WndProc);
            //注册热键
            if (Common.GlobalOCRHotKey.RegisterHotKeyByStr(Common.AppSettings.GlobalOCRHotkey, _hwnd, GlobalOcrHotKey_Pressed) == false)
            {
                Growl.ErrorGlobal(Application.Current.Resources["MainWindow_GlobalOCRError_Hint"].ToString());
            }
            //解决UAC选择No后窗口会被Explorer覆盖 并通过TopMost调整窗口Z Order
            base.Activate();
            base.Topmost = true;
            base.Topmost = false;
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            Common.GlobalOCRHotKey.ProcessHotKey(System.Windows.Forms.Message.Create(hwnd, msg, wParam, lParam));
            return IntPtr.Zero;
        }

        private WeakReference<SettingsWindow>? _settingsWindow;

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsWindow != null && _settingsWindow.TryGetTarget(out var sw))
            {
                sw.Show();
                sw.WindowState = WindowState.Normal;
                sw.Activate();
            }
            else
            {
                sw = new SettingsWindow();
                _settingsWindow = new WeakReference<SettingsWindow>(sw);
                sw.Show();
            }
        }

        private void HookGuideBtn_Click(object sender, RoutedEventArgs e)
        {
            var ggw = new GameGuideWindow(GuideMode.Hook);
            ggw.Show();
        }

        private void OCRGuideBtn_Click(object sender, RoutedEventArgs e)
        {
            var ggw = new GameGuideWindow(GuideMode.Ocr);
            ggw.Show();
        }

        private static void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            var b = (Border)sender;
            if (b.Child is Grid g)
            {
                foreach (var item in g.Children)
                {
                    if (item is Image image)
                    {
                        image.Width -= 4;
                        image.Height -= 4;
                    }
                }
            }
            b.BorderBrush = Brushes.Transparent;
            b.BorderThickness = new Thickness(2);
        }

        private static void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            var b = (Border)sender;
            if (b.Child is Grid g)
            {
                foreach (var item in g.Children)
                {
                    if (item is Image image)
                    {
                        image.Width += 4;
                        image.Height += 4;
                    }
                }
            }
            b.BorderThickness = new Thickness(0);
        }

        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var b = (Border)sender;
            var str = b.Name;
            var temp = str.Remove(0, 4);
            _gid = int.Parse(temp);
            DrawGameImage.Source = ImageHelper.GetGameIcon(GameInfoList[_gid].FilePath).Source;
            RenderOptions.SetBitmapScalingMode(DrawGameImage, BitmapScalingMode.HighQuality);

            GameNameTag.Tag = _gid;
            GameNameTag.Text = GameInfoList[_gid].GameName;
            GameNameTag.MouseEnter += (_, _) => GameNameTag.TextDecorations = TextDecorations.Underline;
            GameNameTag.MouseLeave += (_, _) => GameNameTag.TextDecorations = null;
            if (GameInfoList[_gid].TransMode == 1)
            {
                TransModeTag.Text = Application.Current.Resources["MainWindow_Drawer_Tag_TransMode"].ToString() + "Hook";
            }
            else
            {
                TransModeTag.Text = Application.Current.Resources["MainWindow_Drawer_Tag_TransMode"].ToString() + "OCR";
            }

            GameInfoDrawer.IsOpen = true;
            e.Handled = true;
        }

        private void GameNameTag_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string? gameFileDirectory = Path.GetDirectoryName(GameInfoList[(int)((TextBlock)sender).Tag].FilePath);
            if (Directory.Exists(gameFileDirectory))
            {
                _ = Process.Start("explorer.exe", gameFileDirectory);
            }
        }

        private async Task StartTranslateByGid(int gid)
        {
            List<Process> gameProcessList = new();
            Stopwatch s = new();
            s.Start();
            while (s.Elapsed < TimeSpan.FromSeconds(5))
            {
                //不以exe结尾的ProcessName不会自动把后缀去掉，因此对exe后缀特殊处理
                gameProcessList = Process.GetProcessesByName(Regex.Split(Path.GetFileName(GameInfoList[gid].FilePath), @"\.exe", RegexOptions.IgnoreCase)[0]).ToList();
                if (gameProcessList.Count > 0)
                {
                    break;
                }
            }
            s.Stop();

            if (gameProcessList.Count == 0)
            {
                MessageBox.Show(Application.Current.Resources["MainWindow_StartError_Hint"].ToString(), Application.Current.Resources["MessageBox_Hint"].ToString());
                return;
            }

            Common.GameID = GameInfoList[gid].GameID;
            Common.TransMode = TransMode.Hook;
            Common.UsingDstLang = GameInfoList[gid].DstLang;
            Common.UsingSrcLang = GameInfoList[gid].SrcLang;
            Common.UsingRepairFunc = GameInfoList[gid].RepairFunc;

            switch (Common.UsingRepairFunc)
            {
                case "RepairFun_RemoveSingleWordRepeat":
                    Common.RepairSettings.SingleWordRepeatTimes = int.Parse(GameInfoList[gid].RepairParamA ?? "0");
                    break;
                case "RepairFun_RemoveSentenceRepeat":
                    Common.RepairSettings.SentenceRepeatFindCharNum = int.Parse(GameInfoList[gid].RepairParamA ?? "0");
                    break;
                case "RepairFun_RegexReplace":
                    Common.RepairSettings.Regex = GameInfoList[gid].RepairParamA ?? string.Empty;
                    Common.RepairSettings.Regex_Replace = GameInfoList[gid].RepairParamB ?? string.Empty;
                    break;
                default:
                    break;
            }

            Common.RepairFuncInit();

            Common.TextHooker = gameProcessList.Count == 1 ? new TextHookHandle(gameProcessList[0].Id) : new TextHookHandle(gameProcessList);

            if (!Common.TextHooker.Init(GameInfoList[gid].Isx64 ? Common.AppSettings.Textractor_Path64 : Common.AppSettings.Textractor_Path32))
            {
                MessageBox.Show(Application.Current.Resources["MainWindow_TextractorError_Hint"].ToString());
                return;
            }
            Common.TextHooker.HookCodeList.Add(GameInfoList[gid].HookCode);
            Common.TextHooker.HookCode_Custom = GameInfoList[gid].HookCodeCustom;

            Common.TextHooker.MisakaCodeList.Add(GameInfoList[gid].MisakaHookCode);
            await Common.TextHooker.StartHook(Convert.ToBoolean(Common.AppSettings.AutoHook));

            if (!await Common.TextHooker.AutoAddCustomHookToGameAsync())
            {
                MessageBox.Show(Application.Current.Resources["MainWindow_AutoCustomHookError"].ToString(), Application.Current.Resources["MessageBox_Error"].ToString());
            }

            try
            {
                new TranslateWindow().Show();
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                MessageBox.Show(Application.Current.Resources["MainWindow_StartError_Hint"].ToString(), Application.Current.Resources["MessageBox_Hint"].ToString());
                return;
            }
            catch (Win32Exception)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(Application.Current.Resources["MainWindow_NoAdmin_Hint"].ToString(), Application.Current.Resources["MessageBox_Ask"].ToString(), MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    RestartAsAdmin();
                }
                return;
            }
        }

        private void RestartAsAdmin()
        {
            if (StartProcessAsAdmin(Environment.ProcessPath!))
            {
                ShutDownApp();
            }
        }

        public static bool StartProcessAsAdmin(string fileName)
        {
            ProcessStartInfo processStartInfo = new()
            {
                FileName = fileName,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                var p = Process.Start(processStartInfo);
            }
            catch (Win32Exception)
            {
                return false;
            }
            return true;
        }
        private void CloseDrawerBtn_Click(object sender, RoutedEventArgs e)
        {
            GameInfoDrawer.IsOpen = false;
        }

        private async void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(GameInfoList[_gid].FilePath))
            {
                MessageBox.Show(messageBoxText: $"{Application.Current.Resources["GameFileNotExistsCheck"]}{GameInfoList[_gid].FilePath}", caption: Application.Current.Resources["MessageBox_Error"].ToString(), icon: MessageBoxImage.Error);
                return;
            }
            Process.Start(GameInfoList[_gid].FilePath);
            GameInfoDrawer.IsOpen = false;
            await StartTranslateByGid(_gid);
        }
        private async void LEStartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(GameInfoList[_gid].FilePath))
            {
                MessageBox.Show(messageBoxText: $"{Application.Current.Resources["GameFileNotExistsCheck"]}{GameInfoList[_gid].FilePath}", caption: Application.Current.Resources["MessageBox_Error"].ToString(), icon: MessageBoxImage.Error);
                return;
            }
            var filepath = GameInfoList[_gid].FilePath;
            var p = new ProcessStartInfo();
            var lePath = Common.AppSettings.LEPath;
            p.FileName = lePath + "\\LEProc.exe";
            // 记住加上引号，否则可能会因为路径带空格而无法启动
            p.Arguments = $"-run \"{filepath}\"";
            p.UseShellExecute = false;
            p.WorkingDirectory = lePath;
            Process.Start(p);
            GameInfoDrawer.IsOpen = false;
            await StartTranslateByGid(_gid);
        }

        /// <summary>
        /// 删除游戏按钮事件
        /// </summary>
        private void DeleteGameBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Application.Current.Resources["MainWindow_Drawer_DeleteGameConfirmBox"].ToString(), Application.Current.Resources["MessageBox_Ask"].ToString(), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                GameHelper.DeleteGameByID(GameInfoList[_gid].GameID);
                GamePanelCollection.RemoveAt(_gid);
                GameInfoDrawer.IsOpen = false;
            }

        }

        private void UpdateNameBtn_Click(object sender, RoutedEventArgs e)
        {
            Dialog.Show(new GameNameDialog(this, GameInfoList, _gid));
        }

        private void BlurWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            switch (Common.AppSettings.OnClickCloseButton)
            {
                case "Minimization":
                    Visibility = Visibility.Collapsed;
                    break;
                case "Exit":
                    ShutDownApp();
                    break;
            }
        }

        private void ShutDownApp()
        {
            Common.GlobalOCRHotKey.UnRegisterGlobalHotKey(_hwnd, GlobalOcrHotKey_Pressed);
            CloseNotifyIcon();
            Application.Current.Shutdown();
        }

        public void CloseNotifyIcon()
        {
            Application.Current.Dispatcher.Invoke(NotifyIconContextContent.Dispose);
        }

        private void NotifyIconMainBtn_Click(object sender, RoutedEventArgs e) => NotifyIconContextContent.CloseContextControl();

        /// <summary>
        /// 切换语言通用事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Language_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ResourceDictionary languageResource = new ResourceDictionary();
            if (sender is MenuItem menuItem)
            {
                Common.AppSettings.AppLanguage = menuItem.Tag.ToString() ?? "zh";
                Thread.CurrentThread.CurrentCulture = new CultureInfo(Common.AppSettings.AppLanguage);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(Common.AppSettings.AppLanguage);
                TranslatorCommon.Refresh();
                TextRepair.Refresh();
                languageResource.Source = new Uri($"lang/{Common.AppSettings.AppLanguage}.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries[1] = languageResource;
                MessageBox.Show(Application.Current.Resources["Language_Changed"].ToString(), Application.Current.Resources["MessageBox_Hint"].ToString());
            }
        }

        private async void AutoStartBtn_Click(object sender, RoutedEventArgs e)
        {
            int gid = GetRunningGameGid();
            if (gid == -1)
            {
                Growl.WarningGlobal(Application.Current.Resources["MainWindow_AutoStartError_Hint"].ToString());
            }
            else
            {
                await StartTranslateByGid(gid);
            }
        }

        /// <summary>
        /// 寻找任何正在运行中的之前已保存过的游戏
        /// </summary>
        /// <returns>数组索引（非GameID），-1代表未找到</returns>
        private int GetRunningGameGid()
        {
            GameInfoList = GameHelper.GetAllCompletedGames();

            foreach ((_, string path) in ProcessHelper.GetProcessesData())
            {
                for (int j = 0; j < GameInfoList.Count; j++)
                {
                    if (path == GameInfoList[j].FilePath)
                        return j;
                }
            }

            return -1;
        }

        private void ClipboardGuideBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.TextHooker = new TextHookHandle();
            Common.GameID = Guid.Empty;
            Common.TransMode = TransMode.Hook;
            Common.TextHooker.AddClipBoardThread();

            //剪贴板方式读取的特殊码和misakacode
            Common.TextHooker.HookCodeList.Add("HB0@0");
            Common.TextHooker.MisakaCodeList.Add("【0:-1:-1】");

            var ggw = new GameGuideWindow(GuideMode.Clipboard);
            ggw.Show();
        }


        private async void BlurWindow_ContentRendered(object sender, EventArgs e)
        {
            if (Common.AppSettings.UpdateCheckEnabled)
            {
                await Common.CheckUpdateAsync();
            }
        }

        private void ComicTransBtn_Click(object sender, RoutedEventArgs e)
        {
            var ctmw = new ComicTranslator.ComicTransMainWindow();
            ctmw.Show();
        }

        /// <summary>
        /// 允许关闭全局通知。实际做法是新建了一个无关联的panel，那些通知本质上还是会生成。
        /// </summary>
        void GrowlDisableSwitch()
        {
            if (!Common.AppSettings.GrowlEnabled)
            {
                Growl.InfoGlobal(Application.Current.Resources["MainWindow_NoGlobalNotice"].ToString()); // 必须先显示一句否则GrowlWindow是null
                var gw = typeof(Growl)?.GetField("GrowlWindow", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)?.GetValue(null);
                var panel = gw?.GetType().GetProperty("GrowlPanel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var sp = new StackPanel();
                sp.Children.Add(new UIElement()); // 必须添加一个成员否则HC检测到成员为空时就把GrowlWindow设为null了
                panel?.SetValue(gw, sp);
                this.Closing += (o, e) => gw?.GetType()?.GetMethod("Close")?.Invoke(gw, null); // 关闭主窗口时关闭GrowlWindow否则程序无法退出
            }
        }

        private void LanguageBtn_Click(object sender, RoutedEventArgs e)
        {
            LanguageContextMenu.IsOpen = true;
        }
        private void RestartAsAdminBtn_Clicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(Application.Current.Resources["MainWindow_RestartAdmin_Hint"].ToString(), Application.Current.Resources["MessageBox_Ask"].ToString(), MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                RestartAsAdmin();
            }
        }
    }
}
