using Config.Net;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Mikoto.Config;
using Mikoto.DataAccess;
using Mikoto.Enums;
using Mikoto.Helpers;
using Mikoto.Helpers.File;
using Mikoto.Helpers.Graphics;
using Mikoto.TextHook;
using Mikoto.Translators;
using Mikoto.Windows;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mikoto
{
    public partial class MainWindow
    {
        public static MainWindow Instance { get; set; } = default!;

        private readonly MainWindowViewModel _viewModel = new();
        public MainWindow()
        {
            TextRepair.InitCustomScripts();
            DataContext = _viewModel;
            Instance = this;
            Common.AppSettings = new ConfigurationBuilder<IAppSettings>().UseIniFile(Path.Combine(DataFolder.Path, "settings", "settings.ini")).Build();
            Common.RepairSettings = new ConfigurationBuilder<IRepeatRepairSettings>().UseIniFile(Path.Combine(DataFolder.Path, "settings", "RepairSettings.ini")).Build();

            InitializeLanguage();
            TranslatorCommon.Refresh(App.Env.ResourceService);
            InitializeComponent();
            // 异步获取游戏列表
            _gameInfoInitTask = RefreshGameInfoAsync();
            RefreshUIAsync().FireAndForget();
            SetRandomBlurredBackgroundAsync().FireAndForget();
            GrowlDisableSwitch();

            if (Environment.IsPrivilegedProcess)
            {
                RestartAsAdminBtn.Visibility = Visibility.Collapsed;
            }
        }

        public async Task RefreshAsync()
        {
            await RefreshGameInfoAsync();
            await RefreshUIAsync();
        }

        private CancellationTokenSource? _refreshGameCts;

        private async Task RefreshGameInfoAsync()
        {
            var cts = new CancellationTokenSource();
            var oldCts = Interlocked.Exchange(ref _refreshGameCts, cts);
            oldCts?.Cancel();

            var token = cts.Token;

            try
            {
                await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    App.Env.GameInfoService.GetAllCompletedGames();
                }, token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                Interlocked.CompareExchange(ref _refreshGameCts, null, cts);
                cts.Dispose();
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

        private CancellationTokenSource? _refreshUICts;
        private async Task RefreshUIAsync()
        {
#if DEBUG
            _viewModel.GameInfoFileButtonVisibility = Visibility.Visible;
#endif
            _viewModel.LEStartVisibility= Path.Exists(Common.AppSettings.LEPath) ? Visibility.Visible : Visibility.Collapsed;
            var cts = new CancellationTokenSource();
            var oldCts = Interlocked.Exchange(ref _refreshUICts, cts);
            oldCts?.Cancel();

            var token = cts.Token;

            try
            {
                await _gameInfoInitTask;

                token.ThrowIfCancellationRequested();

                var dict = App.Env.GameInfoService.AllCompletedGamesIdDict;

                if (dict.Count == 0)
                {
                    return;
                }

                var currentId = _viewModel.GameInfo.GameID;

                if (dict.TryGetValue(currentId, out var gameInfo))
                {
                    _viewModel.GameInfo = gameInfo;
                }
                else
                {
                    // 当前不存在，退回任意一个
                    _viewModel.GameInfo = dict.Values.First();
                }


                await InitGameLibraryPanelAsync(token);
            }
            catch (OperationCanceledException)
            {
                // 正常取消，啥都不用做
            }
            finally
            {
                Interlocked.CompareExchange(ref _refreshUICts, null, cts);
                cts.Dispose();
            }
        }

        private async Task InitGameLibraryPanelAsync(CancellationToken token = default)
        {
            var itemsToAdd = App.Env.GameInfoService.AllCompletedGamesIdDict.Values
                .OrderByDescending(info => info.LastPlayAt)
                .ToList();
            token.ThrowIfCancellationRequested();

            // 清空集合（在 UI 线程）
            await Dispatcher.InvokeAsync(_viewModel.GamePanelCollection.Clear, DispatcherPriority.Background, CancellationToken.None);
            const int batchSize = 30;
            for (int i = 0; i < itemsToAdd.Count; i += batchSize)
            {
                token.ThrowIfCancellationRequested();
                var batch = itemsToAdd.Skip(i).Take(batchSize);
                // 切回 UI 线程一次性添加一批
                await Dispatcher.InvokeAsync(() =>
                {
                    var elements = batch.Select(x => CreateGameElement(x)).ToList();

                    foreach (var element in elements)
                    {
                        _viewModel.GamePanelCollection.Add(element);
                    }
                });
                // 让出 UI 线程，让界面有时间渲染
                await Task.Yield();
            }
        }

        private async Task SetRandomBlurredBackgroundAsync()
        {
            await _gameInfoInitTask;

            if (App.Env.GameInfoService.AllCompletedGamesIdDict.Count <= 5) return;
            BitmapSource? image = await Task.Run(GetRandomBlurredImage);
            bool isDark = await Task.Run(() => ImageHelper.IsDarkImage(image));
            ApplySearchBarBrushes(isDark);
            if (image != null)
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    Background = new ImageBrush(image);
                    PlayFadeInAnimation(this);
                });
            }
        }


        private BitmapSource? GetRandomBlurredImage()
        {
            int randomId = new Random().Next(App.Env.GameInfoService.AllCompletedGamesIdDict.Count);
            BitmapSource? ico = ImageHelper.GetGameIconSource(App.Env.GameInfoService.AllCompletedGamesIdDict.ElementAt(randomId).Value.FilePath);
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
        /// 播放淡入动画
        /// </summary>
        private static void PlayFadeInAnimation(UIElement element)
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            element.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        /// <summary>
        /// 创建单个游戏 UI 元素
        /// </summary>
        private Border CreateGameElement(GameInfo game)
        {
            var gameInfo = game;
            string gameName = gameInfo.GameName;
            string filePath = gameInfo.FilePath;

            // 获取游戏图标
            Image ico = ImageHelper.GetGameIcon(filePath);
            RenderOptions.SetBitmapScalingMode(ico, BitmapScalingMode.HighQuality);

            // 计算背景色（可考虑缓存）
            var bgBrush = ImageHelper.GetMajorBrush(ico.Source as BitmapSource);

            var textBlock = new TextBlock
            {
                Text = gameName,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(3),
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.6,
                FontWeight = FontWeights.SemiBold,
                Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryForeground"]
            };

            var grid = new Grid();
            grid.Children.Add(new Border
            {
                Background = bgBrush,
                CornerRadius = new CornerRadius(4),
            });
            grid.Children.Add(ico);
            grid.Children.Add(textBlock);

            var border = new Border
            {
                CornerRadius = new CornerRadius(4),
                Width = 150,
                Height = 120,
                Child = grid,
                Margin = new Thickness(3),
                Tag = gameInfo,
                Focusable = true,
            };

            border.MouseEnter += Border_MouseEnter;
            border.MouseLeave += Border_MouseLeave;
            border.MouseLeftButtonDown += Back_MouseLeftButtonDown;
            border.KeyDown += Border_KeyDown;

            return border;
        }

        private void Border_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Back_MouseLeftButtonDown(sender, null);
            }
        }

        private WeakReference<SettingsWindow>? _settingsWindow;
        private readonly Task _gameInfoInitTask;

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

        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs? e)
        {
            var b = (Border)sender;
            b.Focus();
            var str = b.Name;
            DrawGameImage.Source = ((b.Child as Grid)?.Children.Cast<UIElement>().First(p => p is Image) as Image)?.Source;
            RenderOptions.SetBitmapScalingMode(DrawGameImage, BitmapScalingMode.HighQuality);

            _viewModel.GameInfo = (GameInfo)b.Tag;

            _viewModel.GameInfoDrawerIsOpen = true;
            e?.Handled = true;
        }

        private void GameNameTag_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string? gameFileDirectory = Path.GetDirectoryName(_viewModel.GameInfo?.FilePath);
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

        private async Task StartTranslateByGidAsync()
        {
            List<Process> gameProcessList = new();
            Stopwatch s = Stopwatch.StartNew();

            while (s.Elapsed < TimeSpan.FromSeconds(5))
            {
                string name;
                //不以exe结尾的ProcessName不会自动把后缀去掉，因此对exe后缀特殊处理
                if (Path.GetExtension(_viewModel.GameInfo.FilePath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    name = Path.GetFileNameWithoutExtension(_viewModel.GameInfo.FilePath);
                }
                else
                {
                    name = Path.GetFileName(_viewModel.GameInfo.FilePath);
                }

                gameProcessList = Process.GetProcessesByName(name).ToList();
                if (gameProcessList.Count > 0)
                {
                    break;
                }

                await Task.Delay(100);
            }

            if (gameProcessList.Count == 0)
            {
                Log.Warning("未检测到游戏进程，游戏：{Gid}", _viewModel.GameInfo.GameName);
                MessageBox.Show(
                    Application.Current.Resources["MainWindow_StartError_Hint"].ToString(),
                    Application.Current.Resources["MessageBox_Hint"].ToString());
                return;
            }

            App.Env.Context.GameID = _viewModel.GameInfo.GameID;
            App.Env.Context.TransMode = TransMode.Hook;
            App.Env.Context.UsingDstLang = _viewModel.GameInfo.DstLang;
            App.Env.Context.UsingSrcLang = _viewModel.GameInfo.SrcLang;
            App.Env.Context.UsingRepairFunc = _viewModel.GameInfo.RepairFunc;

            switch (App.Env.Context.UsingRepairFunc)
            {
                case "RepairFun_RemoveSingleWordRepeat":
                    Common.RepairSettings.SingleWordRepeatTimes = int.Parse(_viewModel.GameInfo.RepairParamA ?? "0");
                    break;
                case "RepairFun_RemoveSentenceRepeat":
                    Common.RepairSettings.SentenceRepeatFindCharNum = int.Parse(_viewModel.GameInfo.RepairParamA ?? "0");
                    break;
                case "RepairFun_RegexReplace":
                    Common.RepairSettings.Regex = _viewModel.GameInfo.RepairParamA ?? string.Empty;
                    Common.RepairSettings.Regex_Replace = _viewModel.GameInfo.RepairParamB ?? string.Empty;
                    break;
            }

            TextRepair.RepairFuncInit();

            App.Env.TextHookService =
                gameProcessList.Count == 1
                    ? new TextHookService(gameProcessList[0].Id)
                    : new TextHookService(gameProcessList, new MaxMemoryProcessSelector());

            if (!App.Env.TextHookService.Init(
                    _viewModel.GameInfo.Isx64
                        ? Common.AppSettings.Textractor_Path64
                        : Common.AppSettings.Textractor_Path32))
            {
                Log.Error("TextHookService 初始化失败");
                MessageBox.Show(Application.Current.Resources["MainWindow_TextractorError_Hint"].ToString());
                return;
            }

            await App.Env.TextHookService.StartHookAsync(
                _viewModel.GameInfo,
                Convert.ToBoolean(Common.AppSettings.AutoHook));
            Log.Information(
                "进程注入完成，游戏名={GameName}，进程数={ProcessCount}",
                 _viewModel.GameInfo.GameName,
                gameProcessList.Count);

            if (!await App.Env.TextHookService.AutoAddCustomHookToGameAsync())
            {
                Log.Warning("自动添加自定义 Hook 失败");
                MessageBox.Show(
                    Application.Current.Resources["MainWindow_AutoCustomHookError"].ToString(),
                    Application.Current.Resources["MessageBox_Error"].ToString());
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
                // 游戏没有启动成功，或者启动时间过长
                Log.Warning(ex, "未找到游戏进程");
                MessageBox.Show(Application.Current.Resources["MainWindow_StartError_Hint"].ToString(), Application.Current.Resources["MessageBox_Hint"].ToString());
            }
            catch (Win32Exception ex)
            {
                // 权限不足
                Log.Warning(ex, "权限不足，无法附加到游戏进程");
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
            _viewModel.GameInfoDrawerIsOpen = false;
        }

        private async void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            App.Env.GameInfoService.UpdateGameInfoByID(_viewModel.GameInfo.GameID, nameof(GameInfo.LastPlayAt), DateTime.Now);

            string path = GetEntranceFilePath(_viewModel.GameInfo.FilePath);
            if (!File.Exists(path))
            {
                MessageBox.Show(messageBoxText: $"{Application.Current.Resources["GameFileNotExistsCheck"]}{path}", caption: Application.Current.Resources["MessageBox_Error"].ToString(), icon: MessageBoxImage.Error);
                return;
            }
            Process.Start(path);
            _viewModel.GameInfoDrawerIsOpen = false;
            Log.Information("启动游戏，路径：{Path}", path);
            await StartTranslateByGidAsync();
            await RefreshAsync();
        }

        private static string GetEntranceFilePath(string filePath)
        {
            //hook文件可能与需要打开的文件不是同一个，需要进行转换
            filePath = HookFileHelper.ToEntranceFilePath(filePath);
            return filePath;
        }

        private async void LEStartBtn_Click(object sender, RoutedEventArgs e)
        {
            App.Env.GameInfoService.UpdateGameInfoByID(_viewModel.GameInfo.GameID, nameof(GameInfo.LastPlayAt), DateTime.Now);

            string path = GetEntranceFilePath(_viewModel.GameInfo.FilePath);
            if (!File.Exists(path))
            {
                MessageBox.Show(messageBoxText: $"{Application.Current.Resources["GameFileNotExistsCheck"]}{path}", caption: Application.Current.Resources["MessageBox_Error"].ToString(), icon: MessageBoxImage.Error);
                return;
            }

            var p = new ProcessStartInfo();
            var lePath = Common.AppSettings.LEPath;
            p.FileName = lePath + "\\LEProc.exe";
            if (!File.Exists(p.FileName))
            {
                MessageBox.Show(messageBoxText: $"{p.FileName}{Application.Current.Resources["MainWindow_LEProcNotExistError"]}", caption: Application.Current.Resources["MessageBox_Error"].ToString(), icon: MessageBoxImage.Error);
                return;
            }
            // 记住加上引号，否则可能会因为路径带空格而无法启动
            p.Arguments = $"-run \"{path}\"";
            p.UseShellExecute = false;
            p.WorkingDirectory = lePath;
            Process.Start(p);
            _viewModel.GameInfoDrawerIsOpen = false;
            Log.Information("通过 LE 启动游戏，路径：{Path}", path);
            await StartTranslateByGidAsync();
            RefreshAsync().FireAndForget();
        }

        /// <summary>
        /// 删除游戏按钮事件
        /// </summary>
        private void DeleteGameBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Application.Current.Resources["MainWindow_Drawer_DeleteGameConfirmBox"].ToString(), Application.Current.Resources["MessageBox_Ask"].ToString(), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                App.Env.GameInfoService.DeleteGameByID(_viewModel.GameInfo.GameID);
                RefreshAsync().FireAndForget();
                _viewModel.GameInfoDrawerIsOpen = false;
            }

        }

        private void UpdateNameBtn_Click(object sender, RoutedEventArgs e)
        {
            Dialog.Show(new GameNameDialog(_viewModel.GameInfo));
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
            var running = App.Env.GameInfoService.GetRunningGame();
            if (running == null)
            {
                Growl.WarningGlobal(Application.Current.Resources["MainWindow_AutoStartError_Hint"].ToString());
            }
            else
            {
                _viewModel.GameInfo=running;
                Log.Information("检测到正在运行的游戏：{GameName}", _viewModel.GameInfo.GameName);
                await StartTranslateByGidAsync();
            }
        }



        private void ClipboardGuideBtn_Click(object sender, RoutedEventArgs e)
        {
            App.Env.TextHookService = new TextHook.TextHookService();
            App.Env.Context.GameID = Guid.Empty;
            App.Env.Context.TransMode = TransMode.Hook;
            App.Env.TextHookService.AddClipBoardWatcher();

            var ggw = new GameGuideWindow(TransMode.Clipboard);
            ggw.Show();
        }


        private void BlurWindow_ContentRendered(object sender, EventArgs e)
        {
            if (Common.AppSettings.UpdateCheckEnabled)
            {
                _ = Common.CheckUpdateAsync();
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

        private async void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool visible)
            {
                if (visible)
                {
                    NotifyIconContextContent.Show();
                }
                else
                {
                    SetRandomBlurredBackgroundAsync().FireAndForget();
                }
            }
        }

        private void DrawGameImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (((Image)sender).Source is BitmapSource bitmap)
                {
                    var file = Path.ChangeExtension(Path.GetTempFileName(), ".png");
                    using (var fileStream = new FileStream(file, FileMode.Create))
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        encoder.Save(fileStream);
                    }
                    Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
                }
            }
        }

        private void OpenGameInfoFileBtn_Click(object sender, RoutedEventArgs e)
        {
            App.Env.GameInfoService.OpenGameInfoFile(_viewModel.GameInfo);
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewGameDrawer.IsOpen = true;
        }

        private static void ApplySearchBarBrushes(bool backgroundIsDark)
        {
            var dict = Application.Current.Resources;

            if (backgroundIsDark)
            {
                dict["SearchBarBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1AFFFFFF"));
                dict["SearchBarBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#44FFFFFF"));
                dict["SearchBarBackgroundHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30FFFFFF"));
                dict["SearchBarBorderHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#77FFFFFF"));
                dict["SearchBarBackgroundFocus"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#50FFFFFF"));
                dict["SearchBarBorderFocus"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AFFFFFFF"));
            }
            else
            {
                dict["SearchBarBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A000000"));
                dict["SearchBarBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#33000000"));
                dict["SearchBarBackgroundHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30000000"));
                dict["SearchBarBorderHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#55000000"));
                dict["SearchBarBackgroundFocus"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#50000000"));
                dict["SearchBarBorderFocus"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88000000"));
            }
        }

        internal void SetGameInfoModel(GameInfo game)
        {
            _viewModel.GameInfo=game;
            // 同一个引用，不会自动刷新视图模型，需要手动触发
            _viewModel.RefreshGameInfo();
            UpdateBorder(game);
        }

        private void UpdateBorder(GameInfo game)
        {
            for (int i = 0; i < _viewModel.GamePanelCollection.Count; i++)
            {
                var border = _viewModel.GamePanelCollection[i];
                if (((GameInfo)border.Tag).GameID == game.GameID)
                {
                    // 创建新的 Border 元素
                    var newBorder = CreateGameElement(game);

                    // 替换集合里的元素，这样 ObservableCollection 会通知 UI 刷新
                    _viewModel.GamePanelCollection[i] = newBorder;
                }
            }
        }

        internal void SetLeStartVisibility(Visibility visible)
        {
            _viewModel.LEStartVisibility = visible;
        }
    }
}

