using ArtificialTransHelperLibrary;
using FontAwesome.WPF;
using HandyControl.Controls;
using KeyboardMouseHookLibrary;
using MecabHelperLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
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

namespace MisakaTranslator_WPF
{
    /// <summary>
    /// TranslateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TranslateWindow
    {
        public DispatcherTimer dtimer;//定时器 定时将窗口置顶

        private ArtificialTransHelper _artificialTransHelper;

        private MecabHelper _mecabHelper;
        private BeforeTransHandle _beforeTransHandle;
        private AfterTransHandle _afterTransHandle;
        private ITranslator _translator1; //第一翻译源
        private ITranslator _translator2; //第二翻译源

        private string _currentsrcText; //当前源文本内容

        public string SourceTextFont; //源文本区域字体
        public int SourceTextFontSize; //源文本区域字体大小

        private Queue<string> _gameTextHistory; //历史文本
        public static KeyboardMouseHook hook; //全局键盘鼠标钩子
        public volatile bool IsOCRingFlag; //线程锁:判断是否正在OCR线程中，保证同时只有一组在跑OCR
        public bool IsNotPausedFlag; //是否处在暂停状态（专用于OCR）,为真可以翻译

        private bool _isShowSource; //是否显示原文

        private readonly object _saveTransResultLock = new object(); // 读写数据库和_gameTextHistory的线程锁

        private ITTS _TTS;

        private HWND winHandle;//窗口句柄，用于设置活动窗口，以达到全屏状态下总在最前的目的
        private TransWinSettingsWindow transWinSettingsWindow;

        //Effect 疑似有内存泄露 https://github.com/dotnet/wpf/issues/6782 use frozen
        private DropShadowEffect DropShadow = new DropShadowEffect();

        public TranslateWindow()
        {
            InitializeComponent();

            _isShowSource = true;

            _gameTextHistory = new Queue<string>();

            Topmost = true;
            UI_Init();
            IsOCRingFlag = false;


            _mecabHelper = new MecabHelper(Common.appSettings.Mecab_DicPath);
            if (!_mecabHelper.EnableMecab && Common.appSettings.Mecab_DicPath != string.Empty)
            {
                Growl.InfoGlobal(Application.Current.Resources["TranslateWin_NoMeCab_Hint"].ToString());
            }

            TTS_Init();

            IsNotPausedFlag = true;
            if (Common.appSettings.HttpProxy != "")
            {
                TranslatorCommon.SetHttpProxiedClient(Common.appSettings.HttpProxy);
            }
            _translator1 = TranslatorAuto(Common.appSettings.FirstTranslator);
            _translator2 = TranslatorAuto(Common.appSettings.SecondTranslator);

            _beforeTransHandle = new BeforeTransHandle(Convert.ToString(Common.GameID), Common.UsingSrcLang, Common.UsingDstLang);
            _afterTransHandle = new AfterTransHandle(_beforeTransHandle);

            _artificialTransHelper = new ArtificialTransHelper(Convert.ToString(Common.GameID));

            if (Common.transMode == TransMode.Hook)
            {
                Common.textHooker.MeetHookAddressMessageReceived += ProcessAndDisplayTranslation;
            }
            else if (Common.transMode == TransMode.Ocr)
            {
                MouseKeyboardHook_Init();
            }

            Dispatcher.BeginInvoke(() =>
            {
                transWinSettingsWindow = new TransWinSettingsWindow(this);
            });

        }

        private void TTS_Init()
        {
            DispatcherOperation dispatcherOperation = null;
            switch (Common.appSettings.SelectedTTS)
            {
                case TTSMode.Local:
                    var localTTS = new LocalTTS();
                    if (!string.IsNullOrWhiteSpace(Common.appSettings.LocalTTSVoice))
                    {
                        dispatcherOperation = Dispatcher.BeginInvoke(() =>
                            {
                                localTTS.SetTTSVoice(Common.appSettings.LocalTTSVoice);
                                localTTS.SetVolume(Common.appSettings.LoaclTTSVolume);
                                localTTS.SetRate(Common.appSettings.LocaTTSRate);
                                _TTS = localTTS;
                            });
                    }
                    else
                    {
                        _TTS = null;
                    }
                    break;
                case TTSMode.Azure:
                    if (!string.IsNullOrWhiteSpace(Common.appSettings.AzureTTSVoice) &&
                        !string.IsNullOrWhiteSpace(Common.appSettings.AzureTTSSecretKey) &&
                        !string.IsNullOrWhiteSpace(Common.appSettings.AzureTTSLocation)
                        )
                    {
                        var azureTTS = new AzureTTS();
                        azureTTS.ProxyString = Common.appSettings.AzureTTSProxy;
                        azureTTS.TTSInit(Common.appSettings.AzureTTSSecretKey, Common.appSettings.AzureTTSLocation, Common.appSettings.AzureTTSVoice);
                        _TTS = azureTTS;
                    }
                    else
                    {
                        _TTS = null;
                    }
                    break;
            }
            Dispatcher.BeginInvoke(() =>
            {
                if (dispatcherOperation != null)
                {
                    dispatcherOperation.Wait();
                }
                if (_TTS == null)
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
            if (hook == null)
            {
                hook = new KeyboardMouseHook();
                bool r = false;

                if (Common.UsingHotKey.IsMouse)
                {
                    hook.OnMouseActivity += Hook_OnMouseActivity;
                    if (Common.UsingHotKey.MouseButton == System.Windows.Forms.MouseButtons.Left)
                    {
                        r = hook.Start(true, 1);
                    }
                    else if (Common.UsingHotKey.MouseButton == System.Windows.Forms.MouseButtons.Right)
                    {
                        r = hook.Start(true, 2);
                    }
                }
                else
                {
                    hook.onKeyboardActivity += Hook_OnKeyBoardActivity;
                    int keycode = (int)Common.UsingHotKey.KeyCode;
                    r = hook.Start(false, keycode);
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

            _isShowSource = Common.appSettings.TF_ShowSourceText;
            if (_isShowSource)
            {
                ShowSourceButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.Eye);
            }
            else
            {
                ShowSourceButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.EyeSlash);
            }

            SourceTextFontSize = (int)Common.appSettings.TF_SrcTextSize;
            FirstTransText.FontSize = Common.appSettings.TF_FirstTransTextSize;
            SecondTransText.FontSize = Common.appSettings.TF_SecondTransTextSize;

            SourceTextFont = Common.appSettings.TF_SrcTextFont;
            FirstTransText.FontFamily = new FontFamily(Common.appSettings.TF_FirstTransTextFont);
            SecondTransText.FontFamily = new FontFamily(Common.appSettings.TF_SecondTransTextFont);

            FirstTransText.Stroke = Common.appSettings.TF_FirstWhiteStrokeIsChecked ? Brushes.White : Brushes.Black;
            SecondTransText.Stroke = Common.appSettings.TF_SecondWhiteStrokeIsChecked ? Brushes.White : Brushes.Black;

            BrushConverter brushConverter = new BrushConverter();
            FirstTransText.Fill = (Brush)brushConverter.ConvertFromString(Common.appSettings.TF_FirstTransTextColor);
            SecondTransText.Fill = (Brush)brushConverter.ConvertFromString(Common.appSettings.TF_SecondTransTextColor);

            this.Background = (Brush)brushConverter.ConvertFromString(Common.appSettings.TF_BackColor);

            if (int.Parse(Common.appSettings.TF_LocX) != -1 && int.Parse(Common.appSettings.TF_SizeW) != 0)
            {
                this.Left = int.Parse(Common.appSettings.TF_LocX);
                this.Top = int.Parse(Common.appSettings.TF_LocY);
                this.Width = int.Parse(Common.appSettings.TF_SizeW);
                this.Height = int.Parse(Common.appSettings.TF_SizeH);
            }

            DropShadow.Opacity = 1;
            DropShadow.ShadowDepth = 0;
            DropShadow.BlurRadius = 6;
        }

        /// <summary>
        /// 根据翻译器名称自动返回翻译器类实例(包括初始化)
        /// </summary>
        /// <param name="translator"></param>
        /// <returns></returns>
        public static ITranslator TranslatorAuto(string translator)
        {
            switch (translator)
            {
                case "BaiduTranslator":
                    BaiduTranslator bd = new BaiduTranslator();
                    bd.TranslatorInit(Common.appSettings.BDappID, Common.appSettings.BDsecretKey);
                    return bd;
                case "TencentOldTranslator":
                    TencentOldTranslator txo = new TencentOldTranslator();
                    txo.TranslatorInit(Common.appSettings.TXOSecretId, Common.appSettings.TXOSecretKey);
                    return txo;
                case "CaiyunTranslator":
                    CaiyunTranslator cy = new CaiyunTranslator();
                    cy.TranslatorInit(Common.appSettings.CaiyunToken);
                    return cy;
                case "XiaoniuTranslator":
                    XiaoniuTranslator xt = new XiaoniuTranslator();
                    xt.TranslatorInit(Common.appSettings.xiaoniuApiKey);
                    return xt;
                case "IBMTranslator":
                    IBMTranslator it = new IBMTranslator();
                    it.TranslatorInit(Common.appSettings.IBMApiKey, Common.appSettings.IBMURL);
                    return it;
                case "YandexTranslator":
                    YandexTranslator yt = new YandexTranslator();
                    yt.TranslatorInit(Common.appSettings.YandexApiKey);
                    return yt;
                case "YoudaoZhiyun":
                    YoudaoZhiyun ydzy = new YoudaoZhiyun();
                    ydzy.TranslatorInit(Common.appSettings.YDZYAppId, Common.appSettings.YDZYAppSecret);
                    return ydzy;
                case "YoudaoTranslator":
                    YoudaoTranslator yd = new YoudaoTranslator();
                    yd.TranslatorInit();
                    return yd;
                case "GoogleCNTranslator":
                    GoogleCNTranslator gct = new GoogleCNTranslator();
                    gct.TranslatorInit();
                    return gct;
                case "JBeijingTranslator":
                    JBeijingTranslator bj = new JBeijingTranslator();
                    bj.TranslatorInit(Common.appSettings.JBJCTDllPath);
                    return bj;
                case "KingsoftFastAITTranslator":
                    KingsoftFastAITTranslator kfat = new KingsoftFastAITTranslator();
                    kfat.TranslatorInit(Common.appSettings.KingsoftFastAITPath);
                    return kfat;
                case "DreyeTranslator":
                    DreyeTranslator drt = new DreyeTranslator();
                    drt.TranslatorInit(Common.appSettings.DreyePath);
                    return drt;
                case "DeepLTranslator":
                    DeepLTranslator deepl = new DeepLTranslator();
                    deepl.TranslatorInit(Common.appSettings.DeepLsecretKey, Common.appSettings.DeepLsecretKey);
                    return deepl;
                case "ChatGPTTranslator":
                    ChatGPTTranslator chatgpt = new ChatGPTTranslator();
                    chatgpt.TranslatorInit(Common.appSettings.ChatGPTapiKey, Common.appSettings.ChatGPTapiUrl);
                    return chatgpt;
                case "AzureTranslator":
                    AzureTranslator azure = new AzureTranslator();
                    azure.TranslatorInit(Common.appSettings.AzureSecretKey, Common.appSettings.AzureLocation);
                    return azure;
                case "ArtificialTranslator":
                    ArtificialTranslator at = new ArtificialTranslator();
                    at.TranslatorInit(Common.appSettings.ArtificialPatchPath);
                    return at;
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
            if (Common.isAllWindowCap && Process.GetCurrentProcess().Id != FindWindowInfo.GetProcessIDByHWND(FindWindowInfo.GetWindowHWND(e))
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

            string srcText = null;
            for (int i = 0; i < 3; i++)
            {
                // 重新OCR不需要等待
                if (!isRenew)
                    await Task.Delay(Common.UsingOCRDelay);

                srcText = await Common.ocr.OCRProcessAsync();

                if (!string.IsNullOrEmpty(srcText))
                    break;
            }

            if (!string.IsNullOrEmpty(srcText))
            {
                if (Common.appSettings.OCRsource == "BaiduFanyiOCR" || Common.appSettings.OCRsource == "TencentOCR")
                    Application.Current.Dispatcher.Invoke(() => { FirstTransText.Text = srcText; });
                else
                {
                    if (!Common.appSettings.EachRowTrans) // 不启用分行翻译
                        if (Common.UsingSrcLang != "zh" && Common.UsingSrcLang != "ja")
                            srcText = srcText.Replace(Environment.NewLine, " ").Replace("\n", " ");
                        else
                            srcText = new string(srcText.Where(p => !char.IsWhiteSpace(p)).ToArray());

                    TranslateText(srcText, isRenew);
                }
            }
            else if (!string.IsNullOrEmpty(Common.ocr.GetLastError()))
                Growl.WarningGlobal(Common.appSettings.OCRsource + " Error: " + Common.ocr.GetLastError());
            else
                Growl.WarningGlobal(Application.Current.Resources["TranslateWindow_OCREmpty"].ToString());

            IsOCRingFlag = false;
        }
        DictResWindow _dictResWindow;
        private void DictArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            if (!string.IsNullOrWhiteSpace(textBox.SelectedText))
            {
                dtimer.Stop();
                _dictResWindow ??= new DictResWindow(_TTS);
                _dictResWindow.Search(textBox.SelectedText);
                _dictResWindow.Show();
                dtimer.Start();
            }
        }

        /// <summary>
        /// Hook模式下调用的事件
        /// </summary>
        public void ProcessAndDisplayTranslation(object sender, SolvedDataReceivedEventArgs e)
        {
            //1.得到原句
            string source = e.Data.Data;
            if (source == null && e.Data.HookFunc == "Clipboard")
                return;
            //2.进行去重
            string repairedText = TextRepair.RepairFun_Auto(Common.UsingRepairFunc, source);

            if (!Common.appSettings.EachRowTrans) // 不启用分行翻译
                if (Common.UsingSrcLang != "zh" && Common.UsingSrcLang != "ja")
                    repairedText = repairedText.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("<br>", " ").Replace("</br>", " ");
                else
                    repairedText = new string(repairedText.Where(p => !char.IsWhiteSpace(p)).ToArray()).Replace("<br>", "").Replace("</br>", "");

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
            if (repairedText.Length != 0 && repairedText.Length <= Common.appSettings.TransLimitNums)
            {

                _currentsrcText = repairedText;

                // 3. 更新原文
                UpdateSource(repairedText);

                // 分别获取两个翻译结果
                _ = TranslateApiSubmitASync(repairedText, 1, isRenew);
                _ = TranslateApiSubmitASync(repairedText, 2, isRenew);
            }
        }

        /// <summary>
        /// 更新原文
        /// 注意执行过程中调用了StackPanel等UI组件，必须交回主线程才能执行。
        /// </summary>
        /// <param name="repairedText">原文</param>
        private void UpdateSource(string repairedText)
        {
            // 使用BeginInvoke，在更新原文时可以去获取翻译
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                SourceTextPanel.Children.Clear();
                if (_isShowSource)
                {
                    //3.分词
                    if (Common.UsingSrcLang == "ja")
                    {
                        var mwi = _mecabHelper.SentenceHandle(repairedText);
                        //分词后结果显示
                        for (int i = 0; i < mwi.Count; i++)
                        {
                            StackPanel stackPanel = new StackPanel
                            {
                                Orientation = Orientation.Vertical,
                                Margin = new Thickness(5, 0, 0, 5)
                            };

                            System.Windows.Controls.TextBox textBox = new()
                            {
                                TextAlignment = TextAlignment.Center,
                                IsReadOnly = true,
                                Background = new SolidColorBrush(Colors.Transparent),
                                BorderBrush = new SolidColorBrush(Colors.Transparent),
                                Padding = new Thickness(0),
                                Margin = new Thickness(0)
                            };
                            if (!string.IsNullOrEmpty(SourceTextFont))
                            {
                                FontFamily fontFamily = new FontFamily(SourceTextFont);
                                textBox.FontFamily = fontFamily;
                            }
                            textBox.Text = mwi[i].Word;


                            //选择平假名或者片假名
                            switch (Common.appSettings.TF_PhoneticNotationType)
                            {
                                case PhoneticNotationType.Hiragana:
                                    textBox.Tag = mwi[i].Hiragana;
                                    break;
                                case PhoneticNotationType.Katakana:
                                    textBox.Tag = mwi[i].Katakana;
                                    break;
                                case PhoneticNotationType.Romaji:
                                    textBox.Tag = mwi[i].Romaji;
                                    break;
                                default:
                                    textBox.Tag = mwi[i].Hiragana;
                                    break;
                            }

                            textBox.Margin = new Thickness(0, 0, 0, 0);
                            textBox.FontSize = SourceTextFontSize;
                            textBox.Background = Brushes.Transparent;
                            textBox.PreviewMouseLeftButtonUp += DictArea_MouseLeftButtonUp;

                            if (Common.appSettings.TF_EnableDropShadow)
                            {
                                //加入原文的阴影
                                textBox.Effect = (Effect)DropShadow.GetCurrentValueAsFrozen();
                            }

                            if (Common.appSettings.TF_EnableColorful)
                            {
                                //根据不同词性跟字体上色
                                switch (mwi[i].PartOfSpeech)
                                {
                                    case "補助記号":
                                    case "空白":
                                        textBox.Foreground = Brushes.White;
                                        break;
                                    case "動詞":
                                        textBox.Foreground = Brushes.YellowGreen;
                                        break;
                                    case "形容詞":
                                        textBox.Foreground = Brushes.Orange;
                                        break;
                                    case "判定詞":
                                        textBox.Foreground = Brushes.Yellow;
                                        break;
                                    case "助動詞":
                                        textBox.Foreground = Brushes.LightGreen;
                                        break;
                                    case "名詞":
                                        textBox.Foreground = Brushes.SkyBlue;
                                        break;
                                    case "副詞":
                                        textBox.Foreground = Brushes.BlueViolet;
                                        break;
                                    case "助詞":
                                        textBox.Foreground = Brushes.Wheat;
                                        break;
                                    case "連体詞":
                                        textBox.Foreground = Brushes.Pink;
                                        break;
                                    case "接続詞":
                                        textBox.Foreground = Brushes.Brown;
                                        break;
                                    case "感動詞":
                                        textBox.Foreground = Brushes.Red;
                                        break;
                                    case "指示詞":
                                        textBox.Foreground = Brushes.Plum;
                                        break;
                                    case "代名詞":
                                        textBox.Foreground = Brushes.Olive;
                                        break;
                                    case "接頭辞":
                                        textBox.Foreground = Brushes.LightGreen;
                                        break;
                                    case "接尾辞":
                                        textBox.Foreground = Brushes.LightGoldenrodYellow;
                                        break;
                                    case "形状詞":
                                        textBox.Foreground = Brushes.IndianRed;
                                        break;
                                    default:
                                        textBox.Foreground = Brushes.White;
                                        break;
                                }
                            }
                            else
                            {
                                textBox.Foreground = Brushes.White;
                            }

                            // 假名或注释等的上标标签
                            TextBlock superScript = new TextBlock();
                            if (!string.IsNullOrEmpty(SourceTextFont))
                            {
                                FontFamily fontFamily = new FontFamily(SourceTextFont);
                                superScript.FontFamily = fontFamily;
                            }
                            //选择平假名或者片假名
                            switch (Common.appSettings.TF_PhoneticNotationType)
                            {
                                case PhoneticNotationType.Hiragana:
                                    superScript.Text = mwi[i].Hiragana;
                                    break;
                                case PhoneticNotationType.Katakana:
                                    superScript.Text = mwi[i].Katakana;
                                    break;
                                case PhoneticNotationType.Romaji:
                                    superScript.Text = mwi[i].Romaji;
                                    break;
                                default:
                                    superScript.Text = mwi[i].Hiragana;
                                    break;
                            }

                            superScript.Margin = new Thickness(0, 0, 0, 2);
                            superScript.HorizontalAlignment = HorizontalAlignment.Center;

                            if (Common.appSettings.TF_EnableDropShadow)
                            {
                                //加入注音的阴影
                                superScript.Effect = (Effect)DropShadow.GetCurrentValueAsFrozen();
                            }

                            if (SourceTextFontSize - 6.5 > 0)
                            {
                                superScript.FontSize = SourceTextFontSize - 6.5;
                                if (Common.appSettings.TF_EnableSuperBold)
                                {
                                    superScript.FontWeight = FontWeights.Bold;
                                    //注音加粗
                                }
                            }
                            else
                            {
                                superScript.FontSize = 1;
                            }
                            superScript.Background = Brushes.Transparent;
                            superScript.Foreground = Brushes.White;
                            stackPanel.Children.Add(superScript);

                            //是否打开假名标注
                            if (Common.appSettings.TF_EnablePhoneticNotation)
                            {
                                stackPanel.Children.Add(textBox);
                                SourceTextPanel.Children.Add(stackPanel);
                            }
                            else
                            {
                                textBox.Margin = new Thickness(10, 0, 0, 10);
                                SourceTextPanel.Children.Add(textBox);
                            }
                        }
                    }
                    else
                    {
                        System.Windows.Controls.TextBox textBox = new()
                        {
                            TextAlignment = TextAlignment.Left,
                            IsReadOnly = true,
                            Background = new SolidColorBrush(Colors.Transparent),
                            BorderBrush = new SolidColorBrush(Colors.Transparent),
                            Padding = new Thickness(0),
                            Margin = new Thickness(10, 0, 0, 10)
                        };
                        if (!string.IsNullOrEmpty(SourceTextFont))
                        {
                            FontFamily fontFamily = new FontFamily(SourceTextFont);
                            textBox.FontFamily = fontFamily;
                        }
                        textBox.Text = repairedText;
                        textBox.TextWrapping = TextWrapping.Wrap;
                        textBox.FontSize = SourceTextFontSize;
                        textBox.Background = Brushes.Transparent;
                        textBox.PreviewMouseLeftButtonUp += DictArea_MouseLeftButtonUp;
                        if (Common.appSettings.TF_EnableDropShadow)
                        {
                            textBox.Effect = (Effect)DropShadow.GetCurrentValueAsFrozen(); ;
                        }
                        textBox.Foreground = Brushes.White;
                        SourceTextPanel.Children.Add(textBox);
                    }

                }
            });
        }

        /// <summary>
        /// 提交原文到翻译器，获取翻译结果并显示
        /// </summary>
        /// <param name="repairedText">原文</param>
        /// <param name="tranResultIndex">翻译框序号</param>
        /// <param name="isRenew">是否是重新获取翻译</param>
        private async Task TranslateApiSubmitASync(string repairedText, int tranResultIndex, bool isRenew = false)
        {
            //4.翻译前预处理 
            string beforeString = _beforeTransHandle.AutoHandle(repairedText);

            //5.提交翻译 
            string transRes = string.Empty;
            if (tranResultIndex == 1)
            {
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
            }
            else if (tranResultIndex == 2)
            {
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
            }

            //6.翻译后处理 
            string afterString = _afterTransHandle.AutoHandle(transRes);

            //7.翻译结果显示到窗口上 
            switch (tranResultIndex)
            {
                case 1:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        FirstTransText.Text = afterString;
                        if (Common.appSettings.TF_EnableDropShadow)
                        {
                            FirstTransText.Effect = (Effect)DropShadow.GetCurrentValueAsFrozen(); ;
                        }
                        else
                        {
                            FirstTransText.Effect = null;
                        }
                        //添加第一翻译源的阴影
                    });
                    break;
                case 2:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SecondTransText.Text = afterString;
                        if (Common.appSettings.TF_EnableDropShadow)
                        {
                            SecondTransText.Effect = (Effect)DropShadow.GetCurrentValueAsFrozen(); ;
                        }
                        else
                        {
                            SecondTransText.Effect = null;
                        }
                        //添加第二翻译源的阴影
                    });
                    break;
            }

            if (!isRenew)
            {
                lock (_saveTransResultLock)
                {
                    //8.翻译结果记录到队列
                    // todo: 这是比较粗暴地添加历史记录，可以优化（时间排序等）
                    if (_gameTextHistory.Count > 10)
                    {
                        _gameTextHistory.Dequeue();
                    }
                    _gameTextHistory.Enqueue(repairedText + "\n" + afterString);

                    //9.翻译原句和结果记录到数据库 
                    if (Common.appSettings.ATon)
                    {
                        bool addRes = _artificialTransHelper.AddTrans(repairedText, afterString);
                        if (addRes == false)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                HandyControl.Data.GrowlInfo growlInfo = new HandyControl.Data.GrowlInfo();
                                growlInfo.Message = Application.Current.Resources["ArtificialTransAdd_Error_Hint"].ToString();
                                growlInfo.WaitTime = 2;
                                Growl.InfoGlobal(growlInfo);
                            });
                        }
                    }
                }
            }
        }


        private void ChangeSize_Item_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)(sender as ToggleButton).IsChecked)
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
            if (Common.transMode == TransMode.Hook)
            {
                if (Common.textHooker.Pause)
                {
                    PauseButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.Pause);
                }
                else
                {
                    PauseButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.Play);
                }
                Common.textHooker.Pause = !Common.textHooker.Pause;
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
            _isShowSource = !_isShowSource;
            if (_isShowSource)
            {
                ShowSourceButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.Eye);
            }
            else
            {
                ShowSourceButton.SetValue(Awesome.ContentProperty, FontAwesomeIcon.EyeSlash);
            }
            Common.appSettings.TF_ShowSourceText = _isShowSource;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Common.appSettings.TF_LocX = Convert.ToString((int)this.Left);
            Common.appSettings.TF_LocY = Convert.ToString((int)this.Top);
            Common.appSettings.TF_SizeW = Convert.ToString((int)this.ActualWidth);
            Common.appSettings.TF_SizeH = Convert.ToString((int)this.ActualHeight);

            if (hook != null)
            {
                hook.Stop();
                hook = null;
            }

            if (Common.textHooker != null)
            {
                Common.textHooker.MeetHookAddressMessageReceived -= ProcessAndDisplayTranslation;
                Common.textHooker.StopHook();
                Common.textHooker = null;
            }

            dtimer.Stop();

            _mecabHelper.Dispose();

            try
            {
                //System.InvalidOperationException:“关闭 Window 之后，无法设置 Visibility，也无法调用 Show、ShowDialogor 或 WindowInteropHelper.EnsureHandle。”
                Common.mainWin.Visibility = Visibility.Visible;
            }
            catch (InvalidOperationException)
            { }
        }

        private void Settings_Item_Click(object sender, RoutedEventArgs e)
        {
            dtimer.Stop();
            transWinSettingsWindow.WindowState = WindowState.Normal;
            transWinSettingsWindow.Show();
        }

        private void History_Item_Click(object sender, RoutedEventArgs e)
        {
            var textbox = new HandyControl.Controls.TextBox();
            string his = string.Empty;
            string[] history = _gameTextHistory.ToArray();
            for (int i = history.Length - 1; i > 0; i--)
            {
                his += history[i] + "\n";
                his += "==================================\n";
            }
            textbox.Text = his;
            textbox.FontSize = 15;
            textbox.TextWrapping = TextWrapping.Wrap;
            textbox.TextAlignment = TextAlignment.Left;
            textbox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            var window = new PopupWindow
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
            dtimer.Stop();
            window.Topmost = true;
            window.ShowDialog();
            dtimer.Start();
        }

        private void AddNoun_Item_Click(object sender, RoutedEventArgs e)
        {
            dtimer.Stop();
            AddOptWindow win = new AddOptWindow(_currentsrcText);
            win.ShowDialog();
            dtimer.Start();
        }

        private void RenewOCR_Item_Click(object sender, RoutedEventArgs e)
        {
            if (Common.transMode == TransMode.Ocr)
            {
                TranslateEventOcr(true);
            }
            else
            {
                _ = TranslateApiSubmitASync(_currentsrcText, 1, true);
                _ = TranslateApiSubmitASync(_currentsrcText, 2, true);
            }
        }

        private void Min_Item_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Lock_Item_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)(sender as ToggleButton).IsChecked)
            {
                this.Background = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                BrushConverter brushConverter = new();
                this.Background = (Brush)brushConverter.ConvertFromString(Common.appSettings.TF_BackColor);
            }
        }

        private void TTS_Item_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_currentsrcText))
                _TTS.SpeakAsync(_currentsrcText);
        }

        private void TransWin_Loaded(object sender, RoutedEventArgs e)
        {
            winHandle = (HWND)new WindowInteropHelper(this).Handle;//记录翻译窗口句柄

            dtimer = new System.Windows.Threading.DispatcherTimer();
            dtimer.Interval = TimeSpan.FromSeconds(10);
            dtimer.Tick += dtimer_Tick;
            dtimer.Start();

            Common.mainWin.Visibility = Visibility.Collapsed;
        }

        void dtimer_Tick(object sender, EventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                //定时刷新窗口到顶层
                PInvoke.SetWindowPos(winHandle, HWND.HWND_TOPMOST, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE);
            }
        }

        private void ArtificialTransAdd_Item_Click(object sender, RoutedEventArgs e)
        {
            dtimer.Stop();
            ArtificialTransAddWindow win = new ArtificialTransAddWindow(_currentsrcText, FirstTransText.Text, SecondTransText.Text);
            win.ShowDialog();
            dtimer.Start();
        }
    }
}
