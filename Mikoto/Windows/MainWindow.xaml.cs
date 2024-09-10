using Config.Net;
using DataAccessLibrary;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Mikoto.Enums;
using Mikoto.Helpers.File;
using Mikoto.Helpers.Graphics;
using Mikoto.RegionOverride;
using Mikoto.Translators;
using Mikoto.Windows;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TextHookLibrary;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mikoto
{
    public partial class MainWindow
    {
        public List<GameInfo> GameInfoList { get; set; } = new();
        private int _gid; //当前选中的顺序，并非游戏ID
        private IntPtr _hwnd;

        public static MainWindow Instance { get; set; } = default!;

        private MainViewModel _viewModel = new();
        public MainWindow()
        {
            TextRepair.InitCustomScripts();
            DataContext = _viewModel;
            Instance = this;
            Common.AppSettings = new ConfigurationBuilder<IAppSettings>().UseIniFile(Path.Combine(Common.DataFolder, "settings", "settings.ini")).Build();
            Common.RepairSettings = new ConfigurationBuilder<IRepeatRepairSettings>().UseIniFile(Path.Combine(Common.DataFolder, "settings", "RepairSettings.ini")).Build();

            InitializeLanguage();
            TranslatorCommon.Refresh();
            InitializeComponent();

            Refresh();
            GrowlDisableSwitch();

            if (Environment.IsPrivilegedProcess)
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


        public void Refresh()
        {
            GameInfoList = GameHelper.GetAllCompletedGames();
            InitGameLibraryPanel();
        }

        private void SetRandomBlurredBackground()
        {
            if (GameInfoList.Count > 5)
            {
                Task.Run(() =>
                  {
                      BitmapSource? image = GetRandomBlurredImage();
                      if (image != null)
                      {
                          Dispatcher.BeginInvoke(() => Background = new ImageBrush(image));
                      }
                  });
            }
        }

        private BitmapSource? GetRandomBlurredImage()
        {
            int randomId = new Random().Next(GameInfoList.Count);
            BitmapSource? ico = ImageHelper.GetGameIconSource(GameInfoList[randomId].FilePath);
            if (ico is null)
            {
                return null;
            }
            else
            {
                return ImageHelper.GetBlurImage(ico);
            }
        }

        /// <summary>
        /// 游戏库瀑布流初始化
        /// </summary>
        private void InitGameLibraryPanel()
        {
            _viewModel.GamePanelCollection.Clear();
            InitAddGamePanel();
            for (var i = 0; i < GameInfoList.Count; i++)
            {
                AddGame(i);
            }
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
            gd.Children.Add(new Border()
            {
                Background = ImageHelper.GetMajorBrush(ico.Source as BitmapSource),
                CornerRadius = new CornerRadius(4),
            }
);
            gd.Children.Add(ico);
            gd.Children.Add(tb);
            var back = new Border()
            {
                CornerRadius = new CornerRadius(4),
                Name = "game" + gid,
                Width = 150,
                Child = gd,
                Margin = new Thickness(3),
            };

            back.MouseEnter += Border_MouseEnter;
            back.MouseLeave += Border_MouseLeave;
            back.MouseLeftButtonDown += Back_MouseLeftButtonDown;
            _viewModel.GamePanelCollection.Add(back);
        }

        private void InitAddGamePanel()
        {
            var textBlock = new TextBlock()
            {
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                FontSize = (double)Application.Current.Resources["SubHeadFontSize"]
            };
            textBlock.SetResourceReference(TextBlock.TextProperty, "MainWindow_ScrollViewer_AddNewGame");
            var grid = new Grid();
            grid.Children.Add(new Border()
            {
                Background = (SolidColorBrush)Application.Current.Resources["BoxBtnColor"],
                CornerRadius = new CornerRadius(4),
            });
            grid.Children.Add(textBlock);
            var border = new Border()
            {
                Name = "AddNewName",
                Width = 150,
                Child = grid,
                Margin = new Thickness(3),
                CornerRadius = new CornerRadius(4),
            };
            border.MouseEnter += Border_MouseEnter;
            border.MouseLeave += Border_MouseLeave;
            border.MouseLeftButtonDown += Border_MouseLeftButtonDown;
            _viewModel.GamePanelCollection.Add(border);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddNewGameDrawer.IsOpen = true;
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            _hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(_hwnd)?.AddHook(WndProc);
            //解决UAC选择No后窗口会被Explorer覆盖 并通过TopMost调整窗口Z Order
            base.Activate();
            base.Topmost = true;
            base.Topmost = false;
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
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
            var ggw = new GameGuideWindow(TransMode.Hook);
            ggw.Show();
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            var b = (Border)sender;
            b.BorderBrush = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["PrimaryForeground"]).Color with { A = 10 });
            ThicknessAnimation doubleAnimation = new(new Thickness(3), TimeSpan.FromSeconds(0.3))
            {
                AccelerationRatio = 0.8,
                DecelerationRatio = 0.2,
            };
            StartAnimation(b, doubleAnimation);
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            var b = (Border)sender;
            ThicknessAnimation doubleAnimation = new(new Thickness(0), TimeSpan.FromSeconds(0.3))
            {
                AccelerationRatio = 0.8,
                DecelerationRatio = 0.2,
            };
            StartAnimation(b, doubleAnimation);
        }

        private void StartAnimation(Border b, ThicknessAnimation doubleAnimation)
        {
            Storyboard.SetTarget(doubleAnimation, b);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Border.BorderThicknessProperty));
            Storyboard ellipseStoryboard = new();
            ellipseStoryboard.Children.Add(doubleAnimation);
            doubleAnimation.Freeze();
            ellipseStoryboard.Freeze();
            ellipseStoryboard.Begin(this);
        }

        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var b = (Border)sender;
            var str = b.Name;
            var temp = str.Remove(0, 4);
            _gid = int.Parse(temp);
            DrawGameImage.Source = ((b.Child as Grid)?.Children.Cast<UIElement>().First(p => p is Image) as Image)?.Source;
            RenderOptions.SetBitmapScalingMode(DrawGameImage, BitmapScalingMode.HighQuality);

            GameNameTag.Tag = _gid;
            GameNameTag.Text = GameInfoList[_gid].GameName;


            _viewModel.LastStartTime = GameInfoList[_gid].LastPlayAt.ToString();

            GameInfoDrawer.IsOpen = true;
            e.Handled = true;
        }

        private void GameNameTag_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string? gameFileDirectory = Path.GetDirectoryName(GameInfoList[(int)((TextBlock)sender).Tag].FilePath);
            if (Directory.Exists(gameFileDirectory))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = gameFileDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }

        private async Task StartTranslateByGid(int gid)
        {
            string name;
            //不以exe结尾的ProcessName不会自动把后缀去掉，因此对exe后缀特殊处理
            if (Path.GetExtension(GameInfoList[gid].FilePath).Equals(".exe", StringComparison.CurrentCultureIgnoreCase))
            {
                name = Path.GetFileNameWithoutExtension(GameInfoList[gid].FilePath);
            }
            else
            {
                name = Path.GetFileName(GameInfoList[gid].FilePath);
            }
            List<Process> gameProcessList = Process.GetProcessesByName(name).ToList();

            if (gameProcessList.Count == 0)
            {
                MessageBox.Show(Application.Current.Resources["MainWindow_StartError_Hint"].ToString(), Application.Current.Resources["MessageBox_Hint"].ToString());
                return;
            }
            GlobalWorkingData.Instance.GameID = GameInfoList[gid].GameID;
            GlobalWorkingData.Instance.TransMode = TransMode.Hook;
            GlobalWorkingData.Instance.UsingDstLang = GameInfoList[gid].DstLang;
            GlobalWorkingData.Instance.UsingSrcLang = GameInfoList[gid].SrcLang;
            GlobalWorkingData.Instance.UsingRepairFunc = GameInfoList[gid].RepairFunc;

            switch (GlobalWorkingData.Instance.UsingRepairFunc)
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
            TextRepair.RepairFuncInit();
            GlobalWorkingData.Instance.TextHooker = gameProcessList.Count == 1 ? new TextHookHandle(gameProcessList[0].Id) : new TextHookHandle(gameProcessList);

            if (!GlobalWorkingData.Instance.TextHooker.Init(GameInfoList[gid].Isx64 ? Common.AppSettings.Textractor_Path64 : Common.AppSettings.Textractor_Path32))
            {
                MessageBox.Show(Application.Current.Resources["MainWindow_TextractorError_Hint"].ToString());
                return;
            }

            await GlobalWorkingData.Instance.TextHooker.StartHook(GameInfoList[gid], Convert.ToBoolean(Common.AppSettings.AutoHook));

            if (!await GlobalWorkingData.Instance.TextHooker.AutoAddCustomHookToGameAsync())
            {
                MessageBox.Show(Application.Current.Resources["MainWindow_AutoCustomHookError"].ToString(), Application.Current.Resources["MessageBox_Error"].ToString());
            }
            NotifyIconContextContent.Collapse();
            OpenTranslateWindow();
        }

        private void OpenTranslateWindow()
        {
            try
            {
                new TranslateWindow().Show();
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                MessageBox.Show(Application.Current.Resources["MainWindow_StartError_Hint"].ToString(), Application.Current.Resources["MessageBox_Hint"].ToString());
            }
            catch (Win32Exception)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(Application.Current.Resources["MainWindow_NoAdmin_Hint"].ToString(), Application.Current.Resources["MessageBox_Ask"].ToString(), MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    RestartAsAdmin();
                }
            }
        }

        private void Restart()
        {
            ProcessStartInfo processStartInfo = new()
            {
                FileName = Environment.ProcessPath,
                UseShellExecute = true,
            };
            Process.Start(processStartInfo);
            ShutDownApp();
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
            GameHelper.UpdateGameInfoByID(GameInfoList[_gid].GameID, nameof(GameInfo.LastPlayAt), DateTime.Now);

            string path = GetEntranceFilePath(GameInfoList[_gid].FilePath);
            if (!File.Exists(path))
            {
                MessageBox.Show(messageBoxText: $"{Application.Current.Resources["GameFileNotExistsCheck"]}{path}", caption: Application.Current.Resources["MessageBox_Error"].ToString(), icon: MessageBoxImage.Error);
                return;
            }
            Process.Start(path);
            GameInfoDrawer.IsOpen = false;
            await StartTranslateByGid(_gid);
            Refresh();
        }

        private static string GetEntranceFilePath(string filePath)
        {
            //hook文件可能与需要打开的文件不是同一个，需要进行转换
            filePath = HookFileHelper.ToEntranceFilePath(filePath);
            return filePath;
        }

        private async void LEStartBtn_Click(object sender, RoutedEventArgs e)
        {
            GameHelper.UpdateGameInfoByID(GameInfoList[_gid].GameID, nameof(GameInfo.LastPlayAt), DateTime.Now);

            string path = GetEntranceFilePath(GameInfoList[_gid].FilePath);
            if (!File.Exists(path))
            {
                MessageBox.Show(messageBoxText: $"{Application.Current.Resources["GameFileNotExistsCheck"]}{path}", caption: Application.Current.Resources["MessageBox_Error"].ToString(), icon: MessageBoxImage.Error);
                return;
            }
            ProcessStartInfo processStartInfo = new ProcessStartInfo(path);

            RegionOverrideLauncher.Start(processStartInfo);
            GameInfoDrawer.IsOpen = false;
            await StartTranslateByGid(_gid);
            Refresh();
        }

        /// <summary>
        /// 删除游戏按钮事件
        /// </summary>
        private void DeleteGameBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Application.Current.Resources["MainWindow_Drawer_DeleteGameConfirmBox"].ToString(), Application.Current.Resources["MessageBox_Ask"].ToString(), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                GameHelper.DeleteGameByID(GameInfoList[_gid].GameID);
                _viewModel.GamePanelCollection.Remove(_viewModel.GamePanelCollection.Where(p => p.Name == $"game{_gid}").First());
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
            CloseNotifyIcon();
            Application.Current.Shutdown();
        }

        public void CloseNotifyIcon()
        {
            Application.Current.Dispatcher.Invoke(NotifyIconContextContent.Dispose);
        }

        private void NotifyIconMainBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            this.Show();
            this.Focus();
            NotifyIconContextContent.CloseContextControl();
        }

        /// <summary>
        /// 切换语言通用事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Language_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (Common.AppSettings.AppLanguage != menuItem.Tag.ToString())
                {
                    Common.AppSettings.AppLanguage = menuItem.Tag.ToString()!;
                    MessageBox.Show(Application.Current.Resources["Language_Changed"].ToString(), Application.Current.Resources["MessageBox_Hint"].ToString());
                    Restart();
                }
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
            GlobalWorkingData.Instance.TextHooker = new TextHookHandle();
            GlobalWorkingData.Instance.GameID = Guid.Empty;
            GlobalWorkingData.Instance.TransMode = TransMode.Hook;
            GlobalWorkingData.Instance.TextHooker.AddClipBoardThread();

            var ggw = new GameGuideWindow(TransMode.Clipboard);
            ggw.Show();
        }


        private async void BlurWindow_ContentRendered(object sender, EventArgs e)
        {
            if (Common.AppSettings.UpdateCheckEnabled)
            {
                await Common.CheckUpdateAsync();
            }
        }

        /// <summary>
        /// 允许关闭全局通知。实际做法是新建了一个无关联的panel，那些通知本质上还是会生成。
        /// </summary>
        private void GrowlDisableSwitch()
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

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool newValue && newValue == true)
            {
                SetRandomBlurredBackground();
                NotifyIconContextContent.Show();
            }
        }

        private void GameNameTag_MouseEnter(object sender, MouseEventArgs e) => GameNameTag.TextDecorations = TextDecorations.Underline;

        private void GameNameTag_MouseLeave(object sender, MouseEventArgs e) => GameNameTag.TextDecorations = null;
    }
}
