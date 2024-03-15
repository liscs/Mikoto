using ArtificialTransHelperLibrary;
using FontAwesome.WPF;
using HandyControl.Controls;
using KeyboardMouseHookLibrary;
using MecabHelperLibrary;
using MisakaTranslator.SettingsPages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using TextHookLibrary;
using TextRepairLibrary;
using TranslatorLibrary;
using TranslatorLibrary.Translator;
using TransOptimizationLibrary;
using TTSHelperLibrary;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MisakaTranslator
{
    /// <summary>
    /// TranslateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TranslateWindow
    {
        public DispatcherTimer DispatcherTimer { get; set; } = new();//定时器 定时将窗口置顶

        Process? _gameProcess;
        private ArtificialTransHelper _artificialTransHelper;

        private MecabHelper _mecabHelper;
        private BeforeTransHandle _beforeTransHandle;
        private AfterTransHandle _afterTransHandle;
        private ITranslator? _translator1; //第一翻译源
        private ITranslator? _translator2; //第二翻译源

        private string _currentsrcText = string.Empty; //当前源文本内容

        public string SourceTextFont = string.Empty; //源文本区域字体
        private int _sourceTextFontSize; //源文本区域字体大小

        private Queue<HistoryInfo> _gameTextHistory; //历史文本
        private static KeyboardMouseHook? _hook; //全局键盘鼠标钩子
        public volatile bool IsOCRingFlag; //线程锁:判断是否正在OCR线程中，保证同时只有一组在跑OCR
        public bool IsNotPausedFlag; //是否处在暂停状态（专用于OCR）,为真可以翻译

        private bool _enableShowSource; //是否显示原文

        private readonly object _saveTransResultLock = new(); // 读写数据库和_gameTextHistory的线程锁

        private ITTS? _tts;

        private HWND _winHandle;//窗口句柄，用于设置活动窗口，以达到全屏状态下总在最前的目的
        private TransWinSettingsWindow _transWinSettingsWindow = default!;

        //Effect 疑似有内存泄露 https://github.com/dotnet/wpf/issues/6782 use frozen
        private readonly DropShadowEffect _dropShadowEffect = new();

        private readonly ObservableCollection<UIElement> _sourceTextCollection1 = new();
        private readonly ObservableCollection<UIElement> _sourceTextCollection2 = new();

        PopupWindow? _historyWindow;
        public TranslateWindow()
        {
            InitializeComponent();
            UI_Init();


            _enableShowSource = Common.AppSettings.TF_ShowSourceText;

            _gameTextHistory = new Queue<HistoryInfo>();

            Topmost = true;
            IsOCRingFlag = false;


            _mecabHelper = new MecabHelper(Common.AppSettings.Mecab_DicPath);
            if (!_mecabHelper.EnableMecab && Common.AppSettings.Mecab_DicPath != string.Empty)
            {
                Growl.InfoGlobal(Application.Current.Resources["TranslateWin_NoMeCab_Hint"].ToString());
            }

            TTS_Init();

            IsNotPausedFlag = true;
            if (Common.AppSettings.HttpProxy != "")
            {
                TranslatorCommon.SetHttpProxiedClient(Common.AppSettings.HttpProxy);
            }
            _translator1 = TranslatorAuto(Common.AppSettings.FirstTranslator);
            _translator2 = TranslatorAuto(Common.AppSettings.SecondTranslator);

            _beforeTransHandle = new BeforeTransHandle(Common.GameID.ToString(), Common.UsingSrcLang, Common.UsingDstLang);
            _afterTransHandle = new AfterTransHandle(_beforeTransHandle);

            _artificialTransHelper = new ArtificialTransHelper(Common.GameID.ToString());

            switch (Common.TransMode)
            {
                case TransMode.Hook:
                    Common.TextHooker!.MeetHookAddressMessageReceived += ProcessAndDisplayTranslation;

                    _gameProcess = Process.GetProcessById(Common.TextHooker.GamePID);
                    try
                    {
                        _gameProcess.EnableRaisingEvents = true;
                        _gameProcess.Exited += (_, _) =>
                        {
                            _gameProcess.Dispose();
                            Application.Current.Dispatcher.Invoke(Close);
                        };
                    }
                    catch (Win32Exception)
                    {
                        _gameProcess.Dispose();
                        Application.Current.Dispatcher.Invoke(Close);
                        throw;
                    }

                    break;
                case TransMode.Ocr:
                    MouseKeyboardHook_Init();
                    break;
                case TransMode.Clipboard:
                    Common.TextHooker!.MeetHookAddressMessageReceived += ProcessAndDisplayTranslation;
                    break;
            }

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _transWinSettingsWindow = new TransWinSettingsWindow(this);
            });

            SourceTextPanel1.ItemsSource = _sourceTextCollection1;
            SourceTextPanel2.ItemsSource = _sourceTextCollection2;

            _sourcePanelReference1 = SourceTextPanel1;
            _sourcePanelReference2 = SourceTextPanel2;
            _sourceScrollReference1 = SourceScroll1;
            _sourceScrollReference2 = SourceScroll2;


            Application.Current.MainWindow.Hide();
        }

        private void TTS_Init()
        {
            DispatcherOperation? dispatcherOperation = null;
            switch (Common.AppSettings.SelectedTTS)
            {
                case TTSMode.Local:
                    var localTTS = new LocalTTS();
                    if (!string.IsNullOrWhiteSpace(Common.AppSettings.LocalTTSVoice))
                    {
                        dispatcherOperation = Dispatcher.BeginInvoke(() =>
                            {
                                localTTS.SetTTSVoice(Common.AppSettings.LocalTTSVoice);
                                localTTS.SetVolume(Common.AppSettings.LoaclTTSVolume);
                                localTTS.SetRate(Common.AppSettings.LocaTTSRate);
                                _tts = localTTS;
                            });
                    }
                    else
                    {
                        _tts = null;
                    }
                    break;
                case TTSMode.Azure:
                    if (!string.IsNullOrWhiteSpace(Common.AppSettings.AzureTTSVoice)
                        && !string.IsNullOrWhiteSpace(Common.AppSettings.AzureTTSSecretKey)
                        && !string.IsNullOrWhiteSpace(Common.AppSettings.AzureTTSLocation)
                        )
                    {
                        AzureTTS azureTTS = new(Common.AppSettings.AzureTTSSecretKey, Common.AppSettings.AzureTTSLocation, Common.AppSettings.AzureTTSVoice, Common.AppSettings.AzureTTSProxy);
                        _tts = azureTTS;
                    }
                    else
                    {
                        _tts = null;
                    }
                    break;
            }
            Dispatcher.BeginInvoke(() =>
            {
                dispatcherOperation?.Wait();
                if (_tts == null)
                {
                    Growl.InfoGlobal(Application.Current.Resources["TranslateWin_NoTTS_Hint"].ToString());
                }
            });
        }

        /// <summary>
        /// 键盘鼠标钩子初始化
        /// </summary>
        private void MouseKeyboardHook_Init()
        {
            if (_hook == null)
            {
                _hook = new KeyboardMouseHook();
                bool r = false;

                if (Common.UsingHotKey.IsMouse)
                {
                    _hook.OnMouseActivity += Hook_OnMouseActivity;
                    if (Common.UsingHotKey.MouseButton == System.Windows.Forms.MouseButtons.Left)
                    {
                        r = _hook.Start(true, 1);
                    }
                    else if (Common.UsingHotKey.MouseButton == System.Windows.Forms.MouseButtons.Right)
                    {
                        r = _hook.Start(true, 2);
                    }
                }
                else
                {
                    _hook.OnKeyboardActivity += Hook_OnKeyBoardActivity;
                    int keycode = (int)Common.UsingHotKey.KeyCode;
                    r = _hook.Start(false, keycode);
                }

                if (!r)
                {
                    Growl.ErrorGlobal(Application.Current.Resources["Hook_Error_Hint"].ToString());
                }
            }


        }

        /// <summary>
        /// UI方面的初始化
        /// </summary>
        private void UI_Init()
        {

            _enableShowSource = Common.AppSettings.TF_ShowSourceText;
            if (_enableShowSource)
            {
                ShowSourceButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.Eye);
            }
            else
            {
                ShowSourceButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.EyeSlash);
            }

            SourceTextFontSize = (int)Common.AppSettings.TF_SrcTextSize;
            FirstTransText.FontSize = Common.AppSettings.TF_FirstTransTextSize;
            SecondTransText.FontSize = Common.AppSettings.TF_SecondTransTextSize;

            SourceTextFont = Common.AppSettings.TF_SrcTextFont;
            FirstTransText.FontFamily = new FontFamily(Common.AppSettings.TF_FirstTransTextFont);
            SecondTransText.FontFamily = new FontFamily(Common.AppSettings.TF_SecondTransTextFont);

            FirstTransText.Stroke = Common.AppSettings.TF_FirstWhiteStrokeIsChecked ? Brushes.White : Brushes.Black;
            SecondTransText.Stroke = Common.AppSettings.TF_SecondWhiteStrokeIsChecked ? Brushes.White : Brushes.Black;

            BrushConverter brushConverter = new();
            FirstTransText.Fill = brushConverter.ConvertFromString(Common.AppSettings.TF_FirstTransTextColor) as Brush;
            SecondTransText.Fill = brushConverter.ConvertFromString(Common.AppSettings.TF_SecondTransTextColor) as Brush;

            this.Background = brushConverter.ConvertFromString(Common.AppSettings.TF_BackColor) as Brush;

            if (int.Parse(Common.AppSettings.TF_LocX) != -1 && int.Parse(Common.AppSettings.TF_SizeW) != 0)
            {
                this.Left = int.Parse(Common.AppSettings.TF_LocX);
                this.Top = int.Parse(Common.AppSettings.TF_LocY);
                this.Width = int.Parse(Common.AppSettings.TF_SizeW);
                this.Height = int.Parse(Common.AppSettings.TF_SizeH);
            }

            _dropShadowEffect.Opacity = 1;
            _dropShadowEffect.ShadowDepth = 0;
            _dropShadowEffect.BlurRadius = 6;

            InitTranslateAnimation(FirstTransText);
            InitTranslateAnimation(SecondTransText);

        }

        /// <summary>
        /// 根据翻译器名称自动返回翻译器类实例(包括初始化)
        /// </summary>
        /// <param name="translatorName"></param>
        /// <returns></returns>
        public static ITranslator? TranslatorAuto(string translatorName)
        {
            ITranslator translator;
            switch (translatorName)
            {
                case "BaiduTranslator":
                    translator = new BaiduTranslator();
                    translator.TranslatorInit(Common.AppSettings.BDappID, Common.AppSettings.BDsecretKey);
                    return translator;
                case "TencentOldTranslator":
                    translator = new TencentOldTranslator();
                    translator.TranslatorInit(Common.AppSettings.TXOSecretId, Common.AppSettings.TXOSecretKey);
                    return translator;
                case "CaiyunTranslator":
                    translator = new CaiyunTranslator();
                    translator.TranslatorInit(Common.AppSettings.CaiyunToken);
                    return translator;
                case "XiaoniuTranslator":
                    translator = new XiaoniuTranslator();
                    translator.TranslatorInit(Common.AppSettings.xiaoniuApiKey);
                    return translator;
                case "IBMTranslator":
                    translator = new IBMTranslator();
                    translator.TranslatorInit(Common.AppSettings.IBMApiKey, Common.AppSettings.IBMURL);
                    return translator;
                case "YandexTranslator":
                    translator = new YandexTranslator();
                    translator.TranslatorInit(Common.AppSettings.YandexApiKey);
                    return translator;
                case "YoudaoZhiyun":
                    translator = new YoudaoZhiyun();
                    translator.TranslatorInit(Common.AppSettings.YDZYAppId, Common.AppSettings.YDZYAppSecret);
                    return translator;
                case "GoogleCNTranslator":
                    translator = new GoogleCNTranslator();
                    translator.TranslatorInit();
                    return translator;
                case "JBeijingTranslator":
                    translator = new JBeijingTranslator();
                    translator.TranslatorInit(Common.AppSettings.JBJCTDllPath);
                    return translator;
                case "KingsoftFastAITTranslator":
                    translator = new KingsoftFastAITTranslator();
                    translator.TranslatorInit(Common.AppSettings.KingsoftFastAITPath);
                    return translator;
                case "DreyeTranslator":
                    translator = new DreyeTranslator();
                    translator.TranslatorInit(Common.AppSettings.DreyePath);
                    return translator;
                case "DeepLTranslator":
                    translator = new DeepLTranslator();
                    translator.TranslatorInit(Common.AppSettings.DeepLsecretKey, Common.AppSettings.DeepLsecretKey);
                    return translator;
                case "ChatGPTTranslator":
                    translator = new ChatGPTTranslator();
                    translator.TranslatorInit(Common.AppSettings.ChatGPTapiKey, Common.AppSettings.ChatGPTapiUrl);
                    return translator;
                case "AzureTranslator":
                    translator = new AzureTranslator();
                    translator.TranslatorInit(Common.AppSettings.AzureSecretKey, Common.AppSettings.AzureLocation);
                    return translator;
                case "ArtificialTranslator":
                    translator = new ArtificialTranslator();
                    translator.TranslatorInit(Common.AppSettings.ArtificialPatchPath);
                    return translator;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 键盘点击事件
        /// </summary>
        void Hook_OnKeyBoardActivity(object sender)
        {
            TranslateEventOcr();
        }

        /// <summary>
        /// 鼠标点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hook_OnMouseActivity(object sender, System.Drawing.Point e)
        {
            if (Common.IsAllWindowCap && Environment.ProcessId != FindWindowInfo.GetProcessIDByHWND(FindWindowInfo.GetWindowHWND(e))
                || Common.OCRWinHwnd == FindWindowInfo.GetWindowHWND(e))
            {
                TranslateEventOcr();
            }
        }

        /// <summary>
        /// OCR事件
        /// </summary>
        /// <param name="isRenew">是否是重新获取翻译</param>
        private async void TranslateEventOcr(bool isRenew = false)
        {
            if (!IsNotPausedFlag && IsOCRingFlag)
                return;

            IsOCRingFlag = true;

            string? srcText = null;
            for (int i = 0; i < 3; i++)
            {
                // 重新OCR不需要等待
                if (!isRenew)
                    await Task.Delay(Common.UsingOCRDelay);

                srcText = await Common.Ocr!.OCRProcessAsync()!;

                if (!string.IsNullOrEmpty(srcText))
                    break;
            }

            if (!string.IsNullOrEmpty(srcText))
            {
                if (Common.AppSettings.OCRsource == "BaiduFanyiOCR" || Common.AppSettings.OCRsource == "TencentOCR")
                    Application.Current.Dispatcher.Invoke(() => { FirstTransText.Text = srcText; });
                else
                {
                    if (!Common.AppSettings.EachRowTrans) // 不启用分行翻译
                        if (IsJaOrZh(Common.UsingSrcLang))
                            srcText = srcText.Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
                        else
                            srcText = new string(srcText.Where(p => !char.IsWhiteSpace(p)).ToArray());

                    TranslateText(srcText, isRenew);
                }
            }
            else if (!string.IsNullOrEmpty(Common.Ocr!.GetLastError()))
                Growl.WarningGlobal(Common.AppSettings.OCRsource + " Error: " + Common.Ocr.GetLastError());
            else
                Growl.WarningGlobal(Application.Current.Resources["TranslateWindow_OCREmpty"].ToString());

            IsOCRingFlag = false;
        }
        DictResWindow? _dictResWindow;

        public int SourceTextFontSize { get => _sourceTextFontSize; set => _sourceTextFontSize = value; }

        private void DictArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.TextBox? textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) { return; }
            if (!string.IsNullOrWhiteSpace(textBox.SelectedText))
            {
                DispatcherTimer.Stop();
                _dictResWindow ??= new DictResWindow(_tts);
                _dictResWindow.Search(textBox.SelectedText);
                _dictResWindow.Show();
                DispatcherTimer.Start();
            }
        }

        SolvedDataReceivedEventArgs _lastSolvedDataReceivedEventArgs = new();
        string? _tempData;

        /// <summary>
        /// Hook/Clipboard模式下调用的事件
        /// </summary>
        public async void ProcessAndDisplayTranslation(object sender, SolvedDataReceivedEventArgs e)
        {
            //1.得到原句
            _tempData = e.Data.Data;

            //延迟极短的一段时间，针对Escu:de hook多段返回的特殊处理
            //延迟会导致收到两个内容相同的e
            await Task.Delay(10);
            if (_tempData == null || e.Data == _lastSolvedDataReceivedEventArgs.Data || _tempData != e.Data.Data)
            {
                return;
            }
            _lastSolvedDataReceivedEventArgs = e;

            //2.进行去重
            string repairedText = TextRepair.RepairFun_Auto(Common.UsingRepairFunc, _tempData);

            if (!Common.AppSettings.EachRowTrans) // 不启用分行翻译
            {
                if (IsJaOrZh(Common.UsingSrcLang))
                {
                    repairedText = new string(repairedText.Where(p => !char.IsWhiteSpace(p)).ToArray()).Replace("<br>", "").Replace("</br>", "").Trim();
                }
                else
                {
                    repairedText = repairedText.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("<br>", " ").Replace("</br>", " ").Trim();
                }
            }
            else
            {
                repairedText = repairedText.Trim();
            }
            TranslateText(repairedText);
        }

        /// <summary>
        /// 翻译
        /// </summary>
        /// <param name="repairedText">原文</param>
        /// <param name="isRenew">是否是重新获取翻译</param>
        private void TranslateText(string repairedText, bool isRenew = false)
        {
            //补充:如果去重之后的文本长度超过指定值（默认100），直接不翻译、不显示
            //补充2：如果去重后文本长度为0，则不翻译不显示
            if (repairedText.Length != 0
                && ((repairedText.Length <= Common.AppSettings.TransLimitNums && IsJaOrZh(Common.UsingSrcLang))
                || (repairedText.Split(' ').Length <= Common.AppSettings.TransLimitNums && !IsJaOrZh(Common.UsingSrcLang)))
                )
            {

                _currentsrcText = repairedText;

                // 3. 更新原文
                UpdateSourceAsync(repairedText);

                // 分别获取两个翻译结果
                TranslateApiSubmitASync(repairedText, 1, isRenew);
                TranslateApiSubmitASync(repairedText, 2, isRenew);
            }
        }

        private static bool IsJaOrZh(string s)
        {
            return s == "ja" || s == "zh";
        }


        private ItemsControl _sourcePanelReference1 = new();
        private ItemsControl _sourcePanelReference2 = new();
        private HandyControl.Controls.ScrollViewer _sourceScrollReference1 = new();
        private HandyControl.Controls.ScrollViewer _sourceScrollReference2 = new();

        /// <summary>
        /// 更新原文
        /// 注意执行过程中调用了StackPanel等UI组件，必须交回主线程才能执行。
        /// </summary>
        /// <param name="repairedText">原文</param>
        private async void UpdateSourceAsync(string repairedText)
        {
            if (_sourcePanelReference2.ItemsSource is not ObservableCollection<UIElement> sourceCollection) return;
            if (!_enableShowSource)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _sourceTextCollection1.Clear();
                    _sourceTextCollection2.Clear();
                });
                return;
            }
            await Task.Run(async () =>
            {
                //3.分词
                if (Common.UsingSrcLang == "ja"
                    && _mecabHelper.EnableMecab
                    && (Common.AppSettings.TF_EnablePhoneticNotation || Common.AppSettings.TF_EnableColorful))
                {
                    var mwi = _mecabHelper.SentenceHandle(repairedText);
                    _ = Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        sourceCollection.Clear();
                        //分词后结果显示
                        foreach (MecabWordInfo v in mwi)
                        {
                            StackPanel stackPanel = new()
                            {
                                Orientation = Orientation.Vertical,
                                Margin = new Thickness(5, 0, 0, 5)
                            };

                            System.Windows.Controls.TextBox textBox = new()
                            {
                                IsReadOnly = true,
                                BorderBrush = new SolidColorBrush(Colors.Transparent),
                                Padding = new Thickness(0),
                                Text = v.Word,
                                Margin = new Thickness(0, 0, 0, 0),
                                FontSize = SourceTextFontSize,
                                Background = Brushes.Transparent,
                                HorizontalAlignment = HorizontalAlignment.Center,
                            };
                            textBox.PreviewMouseLeftButtonUp += DictArea_MouseLeftButtonUp;
                            if (!string.IsNullOrEmpty(SourceTextFont))
                            {
                                FontFamily fontFamily = new(SourceTextFont);
                                textBox.FontFamily = fontFamily;
                            }
                            if (Common.AppSettings.TF_EnableDropShadow)
                            {
                                //加入原文的阴影
                                textBox.Effect = (Effect)_dropShadowEffect.GetCurrentValueAsFrozen();
                            }
                            if (Common.AppSettings.TF_EnableColorful)
                            {
                                textBox.TextAlignment = TextAlignment.Center;
                                //根据不同词性跟字体上色
                                textBox.Foreground = v.PartOfSpeech switch
                                {
                                    "補助記号" or "空白" => Brushes.White,
                                    "動詞" => Brushes.YellowGreen,
                                    "形容詞" => Brushes.Orange,
                                    "判定詞" => Brushes.Yellow,
                                    "助動詞" => Brushes.LightGreen,
                                    "名詞" => Brushes.SkyBlue,
                                    "副詞" => Brushes.BlueViolet,
                                    "助詞" => Brushes.Wheat,
                                    "連体詞" => Brushes.Pink,
                                    "接続詞" => Brushes.Brown,
                                    "感動詞" => Brushes.Red,
                                    "指示詞" => Brushes.Plum,
                                    "代名詞" => Brushes.Olive,
                                    "接頭辞" => Brushes.LightGreen,
                                    "接尾辞" => Brushes.LightGoldenrodYellow,
                                    "形状詞" => Brushes.IndianRed,
                                    _ => Brushes.White,
                                };
                            }
                            else
                            {
                                textBox.Foreground = Brushes.White;
                            }

                            if (Common.AppSettings.TF_EnablePhoneticNotation)
                            {
                                // 假名或注释等的上标标签
                                TextBlock NotationTextBlock = new();
                                if (!string.IsNullOrEmpty(SourceTextFont))
                                {
                                    FontFamily fontFamily = new(SourceTextFont);
                                    NotationTextBlock.FontFamily = fontFamily;
                                }
                                //选择平假名或者片假名
                                NotationTextBlock.Text = Common.AppSettings.TF_PhoneticNotationType switch
                                {
                                    PhoneticNotationType.Hiragana => v.Hiragana,
                                    PhoneticNotationType.Katakana => v.Katakana,
                                    PhoneticNotationType.Romaji => v.Romaji,
                                    _ => v.Hiragana,
                                };
                                NotationTextBlock.Margin = new Thickness(0, 0, 0, 2);
                                NotationTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                                if (Common.AppSettings.TF_EnableDropShadow)
                                {
                                    //加入注音的阴影
                                    NotationTextBlock.Effect = (Effect)_dropShadowEffect.GetCurrentValueAsFrozen();
                                }
                                if (SourceTextFontSize - 6.5 > 0)
                                {
                                    NotationTextBlock.FontSize = SourceTextFontSize - 6.5;
                                    if (Common.AppSettings.TF_EnableSuperBold)
                                    {
                                        //注音加粗
                                        NotationTextBlock.FontWeight = FontWeights.Bold;
                                    }
                                }
                                else
                                {
                                    NotationTextBlock.FontSize = 1;
                                }
                                NotationTextBlock.Background = Brushes.Transparent;
                                NotationTextBlock.Foreground = Brushes.White;
                                stackPanel.Children.Add(NotationTextBlock);
                                stackPanel.Children.Add(textBox);
                                sourceCollection.Add(stackPanel);
                            }
                            else
                            {
                                sourceCollection.Add(textBox);
                            }
                        }
                    });
                }
                else
                {
                    _ = Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        sourceCollection.Clear();
                        System.Windows.Controls.TextBox textBox = new()
                        {
                            TextAlignment = TextAlignment.Left,
                            IsReadOnly = true,
                            Background = new SolidColorBrush(Colors.Transparent),
                            BorderBrush = new SolidColorBrush(Colors.Transparent),
                            Padding = new Thickness(0),
                            Margin = new Thickness(10, 0, 0, 10),
                            HorizontalAlignment = HorizontalAlignment.Left,
                        };
                        if (!string.IsNullOrEmpty(SourceTextFont))
                        {
                            FontFamily fontFamily = new(SourceTextFont);
                            textBox.FontFamily = fontFamily;
                        }
                        textBox.Text = repairedText;
                        textBox.TextWrapping = TextWrapping.Wrap;
                        textBox.FontSize = SourceTextFontSize;
                        textBox.Background = Brushes.Transparent;
                        textBox.PreviewMouseLeftButtonUp += DictArea_MouseLeftButtonUp;
                        if (Common.AppSettings.TF_EnableDropShadow)
                        {
                            textBox.Effect = (Effect)_dropShadowEffect.GetCurrentValueAsFrozen(); ;
                        }
                        textBox.Foreground = Brushes.White;
                        sourceCollection.Add(textBox);
                    });
                }
                if (Common.AppSettings.TF_SrcAnimationCheckEnabled)
                {
                    _ = FadeInAsync(_sourcePanelReference2, _sourceScrollReference2);
                    await FadeOutAsync(_sourcePanelReference1, _sourceScrollReference1);
                }
                (_sourcePanelReference1, _sourcePanelReference2) = (_sourcePanelReference2, _sourcePanelReference1);
                (_sourceScrollReference1, _sourceScrollReference2) = (_sourceScrollReference2, _sourceScrollReference1);
            });
        }

        private static async Task FadeInAsync(UIElement uiElement, HandyControl.Controls.ScrollViewer scrollViewer)
        {
            await Application.Current.Dispatcher.BeginInvoke(() => FadeIn(uiElement, scrollViewer));
        }

        private static async Task FadeOutAsync(UIElement uiElement, HandyControl.Controls.ScrollViewer scrollViewer)
        {
            await Application.Current.Dispatcher.BeginInvoke(() => FadeOut(uiElement, scrollViewer));
        }

        private const double FADE_DURATION = 0.3;
        private static void FadeIn(UIElement uiElement, HandyControl.Controls.ScrollViewer scrollViewer)
        {
            uiElement.Opacity = 0;
            scrollViewer.Visibility = Visibility.Visible;
            scrollViewer.ScrollToHome();
            uiElement.Visibility = Visibility.Visible;
            uiElement.BeginAnimation(OpacityProperty, _fadeinAnimation);
        }

        readonly static DoubleAnimation _fadeinAnimation = InitFadeinAnimation();
        private static DoubleAnimation InitFadeinAnimation()
        {
            DoubleAnimation fadeinAnimation = new()
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(FADE_DURATION))
            };
            fadeinAnimation.Freeze();
            return fadeinAnimation;
        }

        private static void FadeOut(UIElement uiElement, HandyControl.Controls.ScrollViewer scrollViewer)
        {
            uiElement.Opacity = 1;

            DoubleAnimation fadeoutAnimation = new()
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(FADE_DURATION))
            };
            fadeoutAnimation.Completed += (_, _) =>
            {
                scrollViewer.Visibility = Visibility.Collapsed;
                uiElement.Visibility = Visibility.Collapsed;
            };
            fadeoutAnimation.Freeze();
            uiElement.BeginAnimation(OpacityProperty, fadeoutAnimation);
        }

        /// <summary>
        /// 提交原文到翻译器，获取翻译结果并显示
        /// </summary>
        /// <param name="repairedText">原文</param>
        /// <param name="tranResultIndex">翻译框序号</param>
        /// <param name="isRenew">是否是重新获取翻译</param>
        private async void TranslateApiSubmitASync(string repairedText, int tranResultIndex, bool isRenew = false)
        {
            //4.翻译前预处理 
            string beforeString = _beforeTransHandle.AutoHandle(repairedText);

            //5.提交翻译 
            string? transRes = string.Empty;
            switch (tranResultIndex)
            {
                case 1:
                    if (_translator1 != null)
                    {
                        transRes = await _translator1.TranslateAsync(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                        if (transRes == null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Growl.WarningGlobal(_translator1.GetType().Name + ": " + _translator1.GetLastError());
                            });
                            return;
                        }
                    }
                    break;
                case 2:
                    if (_translator2 != null)
                    {
                        transRes = await _translator2.TranslateAsync(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                        if (transRes == null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Growl.WarningGlobal(_translator2.GetType().Name + ": " + _translator2.GetLastError());
                            });
                            return;
                        }
                    }
                    break;
            }

            //6.翻译后处理 
            string afterString = _afterTransHandle.AutoHandle(transRes);

            //7.翻译结果显示到窗口上 
            switch (tranResultIndex)
            {
                case 1:
                    _ = Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        FirstTransText.Text = afterString;
                        if (Common.AppSettings.TF_EnableDropShadow)
                        {
                            FirstTransText.Effect = (Effect)_dropShadowEffect.GetCurrentValueAsFrozen(); ;
                        }
                        else
                        {
                            FirstTransText.Effect = null;
                        }
                        if (Common.AppSettings.TF_TransAnimationCheckEnabled)
                        {
                            StartFadeInAnimation(FirstTransText);
                        }

                    }, DispatcherPriority.Send);
                    break;
                case 2:
                    _ = Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        SecondTransText.Text = afterString;
                        if (Common.AppSettings.TF_EnableDropShadow)
                        {
                            SecondTransText.Effect = (Effect)_dropShadowEffect.GetCurrentValueAsFrozen(); ;
                        }
                        else
                        {
                            SecondTransText.Effect = null;
                        }
                        if (Common.AppSettings.TF_TransAnimationCheckEnabled)
                        {
                            StartFadeInAnimation(SecondTransText);
                        }
                    }, DispatcherPriority.Send);
                    break;
            }

            if (!isRenew)
            {
                lock (_saveTransResultLock)
                {
                    //8.翻译结果记录到队列
                    // todo: 这是比较粗暴地添加历史记录，可以优化（时间排序等）
                    if (_gameTextHistory.Count > 1000)
                    {
                        _gameTextHistory.Dequeue();
                    }
                    HistoryInfo historyInfo = new();
                    historyInfo.DateTime = DateTime.Now;
                    historyInfo.Message = repairedText + Environment.NewLine + afterString;
                    historyInfo.TranslatorName = tranResultIndex switch
                    {
                        1 => _translator1?.TranslatorDisplayName ?? string.Empty,
                        2 => _translator2?.TranslatorDisplayName ?? string.Empty,
                        _ => throw new UnreachableException(),
                    };
                    _gameTextHistory.Enqueue(historyInfo);
                    UpdateHistoryWindow();
                    //9.翻译原句和结果记录到数据库 
                    if (Common.AppSettings.ATon)
                    {
                        bool addRes = _artificialTransHelper.AddTrans(repairedText, afterString);
                        if (addRes == false)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                HandyControl.Data.GrowlInfo growlInfo = new()
                                {
                                    Message = Application.Current.Resources["ArtificialTransAdd_Error_Hint"].ToString(),
                                    WaitTime = 2
                                };
                                Growl.InfoGlobal(growlInfo);
                            });
                        }
                    }
                }
            }
        }



        private void StartFadeInAnimation(OutlineText outlineText)
        {
            outlineText.BeginAnimation(OpacityProperty, _fadeinAnimation);
        }

        private void InitTranslateAnimation(OutlineText outlineText)
        {
            LinearGradientBrush opacityBrush = new()
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(0, 0)
            };
            outlineText.OpacityMask = opacityBrush;
            opacityBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 255, 255), 0.0));
            opacityBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 255, 255), 0.99999));
            opacityBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 1));
        }

        private void UpdateHistoryWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_historyWindow != null && _historyWindow.IsLoaded)
                {
                    if (_historyWindow.PopupElement is HandyControl.Controls.TextBox textBox)
                    {
                        textBox.Text = GetHistoryText();
                    }
                }
            }
            );
        }

        private void ChangeSize_Item_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton)?.IsChecked ?? false)
            {
                this.BorderThickness = new(3);
                this.ResizeMode = ResizeMode.CanResizeWithGrip;
                Growl.InfoGlobal(Application.Current.Resources["TranslateWin_DragBox_Hint"].ToString());
            }
            else
            {
                this.BorderThickness = new(0);
                this.ResizeMode = ResizeMode.CanResize;
            }
        }

        private void Exit_Item_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void Pause_Item_Click(object sender, RoutedEventArgs e)
        {
            if (Common.TransMode == TransMode.Hook)
            {
                if (Common.TextHooker!.Pause)
                {
                    PauseButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.Pause);
                }
                else
                {
                    PauseButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.Play);
                }
                Common.TextHooker.Pause = !Common.TextHooker.Pause;
            }
            else
            {
                if (IsNotPausedFlag)
                {
                    PauseButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.Play);
                }
                else
                {
                    PauseButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.Pause);
                }
                IsNotPausedFlag = !IsNotPausedFlag;
            }

        }

        private void ShowSource_Item_Click(object sender, RoutedEventArgs e)
        {
            _enableShowSource = !_enableShowSource;
            if (_enableShowSource)
            {
                ShowSourceButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.Eye);
            }
            else
            {
                ShowSourceButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.EyeSlash);
            }
            Common.AppSettings.TF_ShowSourceText = _enableShowSource;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Common.AppSettings.TF_LocX = Convert.ToString((int)this.Left);
            Common.AppSettings.TF_LocY = Convert.ToString((int)this.Top);
            Common.AppSettings.TF_SizeW = Convert.ToString((int)this.Width);
            Common.AppSettings.TF_SizeH = Convert.ToString((int)this.Height);

            if (_hook != null)
            {
                _hook.Stop();
                _hook = null;
            }

            if (Common.TextHooker != null)
            {
                Common.TextHooker.MeetHookAddressMessageReceived -= ProcessAndDisplayTranslation;
                Common.TextHooker.StopHook();
                Common.TextHooker = null;
            }

            DispatcherTimer.Stop();

            _mecabHelper.Dispose();

            try
            {
                if (Application.Current.MainWindow != null)
                {
                    //System.InvalidOperationException:“关闭 Window 之后，无法设置 Visibility，也无法调用 Show、ShowDialogor 或 WindowInteropHelper.EnsureHandle。”
                    Application.Current.MainWindow.Show();
                    Application.Current.MainWindow.Topmost = true;
                    Application.Current.MainWindow.Topmost = false;
                }
            }
            catch (InvalidOperationException)
            { }
        }

        private void Settings_Item_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer.Stop();
            _transWinSettingsWindow.WindowState = WindowState.Normal;
            _transWinSettingsWindow.Show();
        }

        private void History_Item_Click(object sender, RoutedEventArgs e)
        {
            if (_historyWindow == null || !_historyWindow.IsLoaded)
            {
                var textbox = new HandyControl.Controls.TextBox
                {
                    Text = GetHistoryText(),
                    FontSize = 15,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Left,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    BorderThickness = new Thickness(0),
                };
                _historyWindow = new PopupWindow
                {
                    PopupElement = textbox,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    BorderThickness = new Thickness(0, 0, 0, 0),
                    MaxWidth = 600,
                    MaxHeight = 300,
                    MinWidth = 600,
                    MinHeight = 300,
                    Owner = this,
                    Title = Application.Current.Resources["TranslateWin_History_Title"].ToString()
                };
            }
            _historyWindow.Topmost = true;
            _historyWindow.Show();
        }

        private enum HistoryOrderOption
        {
            //按时间降序
            Default, LatestFirst,
            //按时间升序
            OldestFirst,
        }
        private enum HistoryFilterOption
        {
            //显示所有
            Default, All,
            //只显示第一个
            OnlyFirstTranslator,
            //只显示第二个
            OnlySecondTranslator
        }
        private string GetHistoryText(HistoryOrderOption historyOrderOption = HistoryOrderOption.Default, HistoryFilterOption historyFilterOption = HistoryFilterOption.Default)
        {
            HistoryInfo[] historyList = _gameTextHistory.ToArray();
            switch (historyFilterOption)
            {
                case HistoryFilterOption.Default:
                case HistoryFilterOption.All:
                    break;
                case HistoryFilterOption.OnlyFirstTranslator:
                    if (_translator1 != null)
                    {
                        historyList = historyList.Where(p => p.TranslatorName == _translator1.TranslatorDisplayName).ToArray();
                    }
                    break;
                case HistoryFilterOption.OnlySecondTranslator:
                    if (_translator1 != null)
                    {
                        historyList = historyList.Where(p => p.TranslatorName == _translator2!.TranslatorDisplayName).ToArray();
                    }
                    break;
                default:
                    break;
            }
            StringBuilder historyStringBuilder = new();
            switch (historyOrderOption)
            {
                case HistoryOrderOption.Default:
                case HistoryOrderOption.LatestFirst:
                    for (int i = historyList.Length - 1; i > 0; i--)
                    {
                        historyStringBuilder.AppendLine(historyList[i].ToString());
                    }
                    break;
                case HistoryOrderOption.OldestFirst:
                    for (int i = 0; i < historyList.Length; i++)
                    {
                        historyStringBuilder.AppendLine(historyList[i].ToString());
                    }
                    break;
                default:
                    break;
            }


            return historyStringBuilder.ToString();
        }

        private void AddNoun_Item_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer.Stop();
            AddOptWindow win = new(_currentsrcText);
            win.ShowDialog();
            DispatcherTimer.Start();
        }

        private void RenewOCR_Item_Click(object sender, RoutedEventArgs e)
        {
            if (Common.TransMode == TransMode.Ocr)
            {
                TranslateEventOcr(true);
            }
            else
            {
                TranslateApiSubmitASync(_currentsrcText, 1, true);
                TranslateApiSubmitASync(_currentsrcText, 2, true);
            }
        }

        private void Min_Item_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Lock_Item_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as ToggleButton)?.IsChecked ?? false))
            {
                this.Background = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                BrushConverter brushConverter = new();
                this.Background = brushConverter.ConvertFromString(Common.AppSettings.TF_BackColor) as Brush;
            }
        }

        private void TTS_Item_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_currentsrcText))
                _tts?.SpeakAsync(_currentsrcText);
        }

        void TickWindowTopMost(object? sender, EventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                //定时刷新窗口到顶层
                PInvoke.SetWindowPos(_winHandle, HWND.HWND_TOPMOST, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE);
            }
        }

        private void ArtificialTransAdd_Item_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer.Stop();
            ArtificialTransAddWindow win = new(_currentsrcText, FirstTransText.Text, SecondTransText.Text);
            win.ShowDialog();
            DispatcherTimer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _winHandle = (HWND)new WindowInteropHelper(this).Handle;//记录翻译窗口句柄

            DispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            DispatcherTimer.Tick += TickWindowTopMost;
            DispatcherTimer.Tick += TickClock;
            DispatcherTimer.Start();
        }

        private void TickClock(object? sender, EventArgs e)
        {
            TimeTextBlock.Text = DateTime.Now.ToShortTimeString();
        }
    }
}
