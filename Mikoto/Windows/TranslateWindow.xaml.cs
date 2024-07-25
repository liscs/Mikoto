using ArtificialTransHelperLibrary;
using HandyControl.Controls;
using MecabHelperLibrary;
using Mikoto.Helpers;
using Mikoto.Translators;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using TextHookLibrary;
using TransOptimizationLibrary;
using TTSHelperLibrary;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace Mikoto
{
    /// <summary>
    /// _translateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TranslateWindow
    {
        public TranslateViewModel ViewModel { get; set; }

        public DispatcherTimer DispatcherTimer { get; set; } = new();//定时器

        private Process? _gameProcess;
        private ArtificialTransHelper _artificialTransHelper;

        private MecabHelper _mecabHelper;
        private BeforeTransHandle _beforeTransHandle;
        private AfterTransHandle _afterTransHandle;
        private ITranslator? _translator1; //第一翻译源
        private ITranslator? _translator2; //第二翻译源

        private string _currentSrcText = string.Empty; //当前源文本内容

        private Queue<HistoryInfo> _gameTextHistory; //历史文本

        private readonly object _saveTransResultLock = new(); // 读写数据库和_gameTextHistory的线程锁

        private ITTS? _tts;
        private IVoiceDetector? _voiceDetector;

        private HWND _winHandle;//窗口句柄，用于设置活动窗口，以达到全屏状态下总在最前的目的
        private TransWinSettingsWindow? _transWinSettingsWindow;

        private readonly DropShadowEffect _dropShadowEffect = new();
        private PopupWindow? _historyWindow;
        public TranslateWindow()
        {
            InitializeComponent();
            ViewModel = new(this);
            DataObject.AddCopyingHandler(SourceRichTextBox1, OnCopy);
            DataObject.AddCopyingHandler(SourceRichTextBox2, OnCopy);

            DataContext = ViewModel;
            UI_Init();
            _richTextBoxes = [SourceRichTextBox1, SourceRichTextBox2];

            _gameTextHistory = new Queue<HistoryInfo>();

            Topmost = true;

            _mecabHelper = new MecabHelper(Common.AppSettings.Mecab_DicPath);
            if (!_mecabHelper.EnableMecab && Common.AppSettings.Mecab_DicPath != string.Empty)
            {
                Growl.InfoGlobal(Application.Current.Resources["TranslateWin_NoMeCab_Hint"].ToString());
            }

            TTS_Init();

            if (Common.AppSettings.HttpProxy != "")
            {
                TranslatorCommon.SetHttpProxiedClient(Common.AppSettings.HttpProxy);
            }
            _translator1 = TranslatorCommon.GetTranslator(Common.AppSettings.FirstTranslator);
            _translator2 = TranslatorCommon.GetTranslator(Common.AppSettings.SecondTranslator);

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
                case TransMode.Clipboard:
                    Common.TextHooker!.MeetHookAddressMessageReceived += ProcessAndDisplayTranslation;
                    break;
            }
            Application.Current.MainWindow.Hide();

            StoryBoardInit();
        }

        private void StoryBoardInit()
        {
            NameScope.SetNameScope(this, new NameScope());
            (var i1, var o1) = InitSrcFadeStoryboard(SourceRichTextBox1);
            (var i2, var o2) = InitSrcFadeStoryboard(SourceRichTextBox2);
            _srcFadeInStoryBoard = [i1, i2];
            _srcFadeOutStoryBoard = [o1, o2];
        }

        private Storyboard[] _srcFadeInStoryBoard = [];
        private Storyboard[] _srcFadeOutStoryBoard = [];

        /// <summary>
        /// 重写复制有两行内容的富文本框
        /// </summary>
        private void OnCopy(object sender, DataObjectCopyingEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.DataObject.GetData("UnicodeText").ToString()))
            {
                //如果是正常的复制就直接返回不重写覆盖
                return;
            }

            e.CancelCommand();
            string result = GetRubyAndText(_richTextBoxes[_updatingSouceNumber]).Item2;
            if (!string.IsNullOrEmpty(result))
            {
                Clipboard.SetText(result);
            }
        }

        private static (string, string) GetRubyAndText(RichTextBox richTextBox)
        {
            FlowDocument flowDocument = richTextBox.Document;
            if (flowDocument.Blocks.FirstBlock is Paragraph paragraph)
            {
                System.Collections.IList list = paragraph.Inlines;

                GetSelectionRangeIndex(richTextBox, out int start, out int end);

                StringBuilder copyRubyStringBuilder = new();
                StringBuilder copyStringBuilder = new();
                for (int i = start; i < end; i++)
                {
                    if (list.Count <= i || list[i] is not InlineUIContainer item)
                    {
                        return (string.Empty, string.Empty);
                    }
                    if (item.Child is StackPanel stackPanel)
                    {
                        if (stackPanel.Children[0] is TextBlock rubyTextBlock)
                        {
                            copyRubyStringBuilder.Append(rubyTextBlock.Text);
                        }
                        if (stackPanel.Children[1] is TextBlock textBlock)
                        {
                            copyStringBuilder.Append(textBlock.Text);
                        }
                    }
                }
                (string ruby, string text) = (copyRubyStringBuilder.ToString(), copyStringBuilder.ToString());
                return (ruby, text);
            }
            return (string.Empty, string.Empty);
        }

        private static void GetSelectionRangeIndex(RichTextBox richTextBox, out int start, out int end)
        {
            TextPointer docStart = richTextBox.Document.ContentStart;
            TextPointer selectionStart = richTextBox.Selection.Start;
            TextPointer selectionEnd = richTextBox.Selection.End;

            start = new TextRange(docStart, selectionStart).Text.Length;
            end = new TextRange(docStart, selectionEnd).Text.Length;
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
                        _tts = new AzureTTS(Common.AppSettings.AzureTTSSecretKey, Common.AppSettings.AzureTTSLocation, Common.AppSettings.AzureTTSVoice, Common.AppSettings.AzureTTSVoiceVolume, Common.AppSettings.AzureTTSVoiceStyle, Common.AppSettings.AzureTTSProxy);
                        _voiceDetector = new AzureVoiceDetector(Common.AppSettings.AzureTTSSecretKey, Common.AppSettings.AzureTTSLocation, Common.UsingSrcLang);
                    }
                    else
                    {
                        _tts = null;
                        _voiceDetector?.Dispose();
                        _voiceDetector = null;
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
        /// UI方面的初始化
        /// </summary>
        private void UI_Init()
        {
            //TODO MVVM重写
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
            _dropShadowEffect.Freeze();

            InitTranslateAnimation(FirstTransText);
            InitTranslateAnimation(SecondTransText);

            ViewModelInit();
        }

        private void ViewModelInit()
        {
            BrushConverter brushConverter = new();
            ViewModel.SourceTextColor = brushConverter.ConvertFromString(Common.AppSettings.TF_SrcTextColor) as SolidColorBrush ?? Brushes.White;

            ViewModel.FirstTextStrokeThickness = Common.AppSettings.TF_FirstTextStrokeThickness;
            ViewModel.FirstTextFontWeight = FontWeight.FromOpenTypeWeight(Common.AppSettings.TF_FirstTextFontWeight);
            ViewModel.SecondTextStrokeThickness = Common.AppSettings.TF_SecondTextStrokeThickness;
            ViewModel.SecondTextFontWeight = FontWeight.FromOpenTypeWeight(Common.AppSettings.TF_SecondTextFontWeight);
        }

        private DictResWindow? _dictResWindow;


        private static string GetSelectedText(RichTextBox textBox)
        {
            string result = GetRubyAndText(textBox).Item2;
            if (string.IsNullOrWhiteSpace(result))
            {
                result = textBox.Selection.Text;
            }
            return result;
        }

        private SolvedDataReceivedEventArgs _lastSolvedDataReceivedEventArgs = new();
        private string? _tempData;

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

            Dispatcher.Invoke(SetWindowTopMost);

            if (Common.AppSettings.AzureEnableAutoSpeak)
            {
                _ = SpeakIfNoGameVoiceAsync(repairedText);
            }
            TranslateText(repairedText);
        }

        private async Task SpeakIfNoGameVoiceAsync(string text)
        {
            if (_voiceDetector == null || _tts == null)
            {
                return;
            }

            try
            {
                (bool playing, string info) = await _voiceDetector.IsVoicePlaying();
                Application.Current.Dispatcher.Invoke(() => Logger.Info(info));
                if (!playing)
                {
                    await _tts.SpeakAsync(text);
                }
                else
                {
                    //如果有人说话就停止
                    await _tts.StopSpeakAsync();
                }

            }
            catch (TaskCanceledException ex)
            {
                Application.Current.Dispatcher.Invoke(() => Logger.Warn(ex.Message));
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => Logger.Error(ex));
            }

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

                _currentSrcText = repairedText;

                // 3. 更新原文
                UpdateSourceAsync(repairedText);

                // 分别获取两个翻译结果
                TranslateApiSubmit(repairedText, 1, isRenew);
                TranslateApiSubmit(repairedText, 2, isRenew);
            }
        }

        private static bool IsJaOrZh(string s)
        {
            return s == "ja" || s == "zh";
        }


        /// <summary>
        /// 更新原文
        /// 注意执行过程中调用了StackPanel等UI组件，必须交回主线程才能执行。
        /// </summary>
        /// <param name="repairedText">原文</param>
        private async void UpdateSourceAsync(string repairedText)
        {
            _maxOffset = 0;//重置scrollbar最大偏移
            if (!Common.AppSettings.TF_ShowSourceText)
            {
                return;
            }
            await Task.Run(() =>
            {
                //3.分词
                if (Common.UsingSrcLang == "ja"
                    && _mecabHelper.EnableMecab
                    && (Common.AppSettings.TF_EnablePhoneticNotation || Common.AppSettings.TF_EnableColorful))
                {
                    try
                    {
                        var mwi = _mecabHelper.SentenceHandle(repairedText);
                        Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        UpdateSourceRichBoxes(mwi);
                    });
                    }
                    catch (ObjectDisposedException)
                    {
                        //说明翻译窗口已经关闭，mecab资源已Dispose
                        Logger.Info("翻译窗口已经关闭，mecab资源已Dispose");
                    }

                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        UpdateSourceRichBoxes(repairedText);
                    });
                }
            });
        }

        private void UpdateSourceRichBoxes(string repairedText)
        {
            Paragraph paragraph = GenerateParagraph(repairedText);

            AnimatedUpdateFlowDocument(paragraph);
        }

        private static Paragraph GenerateParagraph(string repairedText)
        {
            Paragraph paragraph = new();
            paragraph.Inlines.Add(new Run(repairedText));
            return paragraph;
        }

        private void AnimatedUpdateFlowDocument(Paragraph paragraph)
        {
            SwitchUpdatingNumber();

            RichTextBox richTextBox = _richTextBoxes[_updatingSouceNumber];
            if (Common.AppSettings.TF_EnableDropShadow)
            {
                richTextBox.Effect = _dropShadowEffect;
            }
            else
            {
                richTextBox.Effect = null;
            }


            FlowDocument flowDocument = richTextBox.Document;
            flowDocument.Blocks.Remove(flowDocument.Blocks.LastBlock);
            flowDocument.Blocks.Add(paragraph);

            double contentWidth = TextMeasureHelper.GetContentWidth(richTextBox);
            if (Common.AppSettings.TF_SrcSingleLineDisplay)
            {
                flowDocument.PageWidth = contentWidth;
            }
            else
            {
                flowDocument.PageWidth = Width * 0.95;
            }

            if (Common.AppSettings.TF_SrcAnimationCheckEnabled)
            {
                StartSourceSwitchAnimation();
            }
            else
            {
                StopSourceAnimations();
                _richTextBoxes[_updatingSouceNumber].Visibility = Visibility.Visible;
                _richTextBoxes[_updatingSouceNumber].Opacity = 1;
                _richTextBoxes[1 - _updatingSouceNumber].Visibility = Visibility.Collapsed;

            }
        }

        private void StopSourceAnimations()
        {
            foreach (var item in _srcFadeInStoryBoard)
            {
                item.Stop(this);
            }
            foreach (var item in _srcFadeOutStoryBoard)
            {
                item.Stop(this);
            }
        }

        private void UpdateSourceRichBoxes(List<MecabWordInfo> mwi)
        {
            Paragraph paragraph = GenerateParagraph(mwi);
            AnimatedUpdateFlowDocument(paragraph);
        }

        private Paragraph GenerateParagraph(List<MecabWordInfo> mwi)
        {
            Paragraph paragraph = new();

            //分词后结果显示
            if (Common.AppSettings.TF_EnablePhoneticNotation)
            {
                //显示注音的情况
                foreach (MecabWordInfo v in mwi)
                {
                    StackPanel stackPanel = new()
                    {
                        Orientation = Orientation.Vertical,
                        Margin = new Thickness(0)
                    };
                    stackPanel.Children.Add(CreateRubyTextBlock(v));
                    stackPanel.Children.Add(CreateSourceTextBlock(v));
                    paragraph.Inlines.Add(stackPanel);
                }
            }
            else
            {
                //不显示注音的情况
                foreach (MecabWordInfo v in mwi)
                {
                    paragraph.Inlines.Add(CreateColoredRun(v));
                }
            }

            return paragraph;
        }

        private static Run CreateColoredRun(MecabWordInfo info)
        {
            Run run = new(info.Word);
            if (Common.AppSettings.TF_EnableColorful)
            {
                run.Foreground = GetWordColorBrush(info.PartOfSpeech);
            }
            return run;
        }

        private static int _updatingSouceNumber = 0;
        private List<RichTextBox> _richTextBoxes;

        private static void SwitchUpdatingNumber()
        {
            _updatingSouceNumber = 1 - _updatingSouceNumber;
        }

        private void StartSourceSwitchAnimation()
        {
            StopSourceAnimations();
            _richTextBoxes[_updatingSouceNumber].Visibility = Visibility.Visible;
            _richTextBoxes[1 - _updatingSouceNumber].Visibility = Visibility.Visible;
            _srcFadeInStoryBoard[_updatingSouceNumber].Begin(this, true);
            _srcFadeOutStoryBoard[1 - _updatingSouceNumber].Begin(this, true);
        }

        public (Storyboard, Storyboard) InitSrcFadeStoryboard(RichTextBox richTextBox)
        {
            this.RegisterName(richTextBox.Name, richTextBox);

            DoubleAnimation fadeInAnimation = InitFadeInAnimation();
            Storyboard.SetTargetName(fadeInAnimation, richTextBox.Name);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(OpacityProperty));
            Storyboard fadeInStoryBoard = new();
            fadeInStoryBoard.Children.Add(fadeInAnimation);


            DoubleAnimation fadeOutAnimation = InitFadeOutAnimation(richTextBox);
            Storyboard.SetTargetName(fadeOutAnimation, richTextBox.Name);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(OpacityProperty));
            Storyboard fadeoutStoryboard = new();
            fadeoutStoryboard.Children.Add(fadeOutAnimation);

            fadeInAnimation.Freeze();
            fadeoutStoryboard.Freeze();
            return (fadeInStoryBoard, fadeoutStoryboard);
        }


        private static TextBlock CreateSourceTextBlock(MecabWordInfo info)
        {
            TextBlock textBox = new()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            return UpdateTextBox(info, textBox);
        }

        private static TextBlock UpdateTextBox(MecabWordInfo info, TextBlock textBlock)
        {
            textBlock.Text = info.Word;
            if (Common.AppSettings.TF_EnableColorful)
            {
                textBlock.Foreground = GetWordColorBrush(info.PartOfSpeech);
            }
            return textBlock;
        }

        private TextBlock CreateRubyTextBlock(MecabWordInfo info)
        {
            TextBlock rubyTextBlock = new()
            {
                Background = Brushes.Transparent,
                Foreground = ViewModel.SourceTextColor,
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            return UpdateRubyTextBlock(info, rubyTextBlock);
        }

        private TextBlock UpdateRubyTextBlock(MecabWordInfo info, TextBlock rubyTextBlock)
        {
            if (Common.AppSettings.TF_EnableColorful)
            {
                rubyTextBlock.Foreground = GetWordColorBrush(info.PartOfSpeech);
            }

            //选择平假名或者片假名
            rubyTextBlock.Text = Common.AppSettings.TF_PhoneticNotationType switch
            {
                PhoneticNotationType.Hiragana => info.Hiragana,
                PhoneticNotationType.Katakana => info.Katakana,
                PhoneticNotationType.Romaji => info.Romaji,
                _ => info.Hiragana,
            };

            if (ViewModel.SourceTextFontSize - 6.5 > 0)
            {
                rubyTextBlock.FontSize = ViewModel.SourceTextFontSize - 6.5;
                if (Common.AppSettings.TF_EnableSuperBold)
                {
                    //注音加粗
                    rubyTextBlock.FontWeight = FontWeights.Bold;
                }
            }
            else
            {
                rubyTextBlock.FontSize = 1;
            }

            return rubyTextBlock;
        }

        private static SolidColorBrush GetWordColorBrush(string partOfSpeech)
        {
            //根据不同词性跟字体上色
            return partOfSpeech switch
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

        private const double FADE_DURATION = 0.3;
        private static DoubleAnimation InitFadeInAnimation()
        {
            DoubleAnimation fadeinAnimation = new()
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(FADE_DURATION))
            };
            return fadeinAnimation;
        }
        private static DoubleAnimation InitFadeOutAnimation(RichTextBox richTextBox)
        {
            DoubleAnimation fadeoutAnimation = new()
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(FADE_DURATION))
            };
            fadeoutAnimation.Completed += (_, _) =>
            {
                richTextBox.Visibility = Visibility.Collapsed;
            };
            return fadeoutAnimation;
        }

        /// <summary>
        /// 提交原文到翻译器，获取翻译结果并显示
        /// </summary>
        /// <param name="repairedText">原文</param>
        /// <param name="tranResultIndex">翻译框序号</param>
        /// <param name="isRenew">是否是重新获取翻译</param>
        private async void TranslateApiSubmit(string repairedText, int tranResultIndex, bool isRenew = false)
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
                            FirstTransText.Effect = _dropShadowEffect;
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
                            SecondTransText.Effect = _dropShadowEffect; ;
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

                    string? name = tranResultIndex switch
                    {
                        1 => _translator1?.TranslatorDisplayName,
                        2 => _translator2?.TranslatorDisplayName,
                        _ => throw new UnreachableException(),
                    };

                    if (name != null)
                    {
                        historyInfo.TranslatorName = name;
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
        }


        private readonly DoubleAnimation _translateFadeInAnimation = (DoubleAnimation)InitFadeInAnimation().GetAsFrozen();
        private void StartFadeInAnimation(OutlineText outlineText)
        {
            outlineText.BeginAnimation(OpacityProperty, _translateFadeInAnimation);
        }

        private static void InitTranslateAnimation(OutlineText outlineText)
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
                    ViewModel.PauseButtonIconText = "\uF8AE";
                }
                else
                {
                    ViewModel.PauseButtonIconText = "\uF5B0";
                }
                Common.TextHooker.Pause = !Common.TextHooker.Pause;
            }
        }

        private void ShowSource_Item_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ShowSourceButtonIconText.ToString() == "\uE8C4")
            {
                ViewModel.ShowSourceButtonIconText = "\uE8C5";
            }
            else
            {
                ViewModel.ShowSourceButtonIconText = "\uE8C4";
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Common.AppSettings.TF_LocX = Convert.ToString((int)this.Left);
            Common.AppSettings.TF_LocY = Convert.ToString((int)this.Top);
            Common.AppSettings.TF_SizeW = Convert.ToString((int)this.Width);
            Common.AppSettings.TF_SizeH = Convert.ToString((int)this.Height);


            if (Common.TextHooker != null)
            {
                Common.TextHooker.MeetHookAddressMessageReceived -= ProcessAndDisplayTranslation;
                Common.TextHooker.StopHook();
                Common.TextHooker = null;
            }


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

            CloseSubWindows();
        }

        private void CloseSubWindows()
        {
            _addOptWindow?.Close();
            _dictResWindow?.Close();
            _transWinSettingsWindow?.Close();
            _historyWindow?.Close();
        }

        private void Settings_Item_Click(object sender, RoutedEventArgs e)
        {
            _transWinSettingsWindow ??= new TransWinSettingsWindow(this);
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
                        historyList = historyList.Where(p => p.TranslatorName == _translator1?.GetType().GetProperty(nameof(ITranslator.TranslatorDisplayName))?.GetValue(null)?.ToString()).ToArray();
                    }
                    break;
                case HistoryFilterOption.OnlySecondTranslator:
                    if (_translator1 != null)
                    {
                        historyList = historyList.Where(p => p.TranslatorName == _translator1?.GetType().GetProperty(nameof(ITranslator.TranslatorDisplayName))?.GetValue(null)?.ToString()).ToArray();
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

        private AddOptWindow? _addOptWindow;
        private void AddNoun_Item_Click(object sender, RoutedEventArgs e)
        {
            _addOptWindow = new AddOptWindow(_currentSrcText);
            _addOptWindow.ShowDialog();
        }

        private void Repeat_Item_Click(object sender, RoutedEventArgs e)
        {
            TranslateApiSubmit(_currentSrcText, 1, true);
            TranslateApiSubmit(_currentSrcText, 2, true);
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
                BackgroundBlurHelper.DisableBlur(this);
            }
            else
            {
                this.Background = new BrushConverter().ConvertFromString(Common.AppSettings.TF_BackColor) as Brush;
                if (Common.AppSettings.TF_BackgroundBlurCheckEnabled)
                {
                    BackgroundBlurHelper.EnableBlur(this);
                }
            }
        }

        private void TTS_Item_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_currentSrcText))
                _tts?.SpeakAsync(_currentSrcText);
        }

        private void SetWindowTopMost()
        {
            if (this.WindowState != WindowState.Minimized)
            {
                PInvoke.SetWindowPos(_winHandle, HWND.HWND_TOPMOST, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE);
            }
        }

        private void ArtificialTransAdd_Item_Click(object sender, RoutedEventArgs e)
        {
            ArtificialTransAddWindow win = new(_currentSrcText, FirstTransText.Text, SecondTransText.Text);
            win.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _winHandle = (HWND)new WindowInteropHelper(this).Handle;//记录翻译窗口句柄

            StartDispatcherTimer();

            if (Common.AppSettings.TF_BackgroundBlurCheckEnabled)
            {
                BackgroundBlurHelper.EnableBlur(this);
            }
        }

        private void StartDispatcherTimer()
        {
            DispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            DispatcherTimer.Tick += TickClock;
            DispatcherTimer.Start();
        }

        private void TickClock(object? sender, EventArgs e)
        {
            string nowTime = DateTime.Now.ToShortTimeString();
            if (TimeTextBlock.Text != nowTime)
            {
                TimeTextBlock.Text = nowTime;
            }
        }

        private void SourceRichTextBoxRightClickMenu_Opening(object sender, ContextMenuEventArgs e)
        {
            if (CanCopyRuby(_richTextBoxes[_updatingSouceNumber]))
            {
                ViewModel.CopyRubyVisibility = true;
            }
            else
            {
                ViewModel.CopyRubyVisibility = false;
            }
        }

        private static bool CanCopyRuby(RichTextBox richTextBox)
        {
            return !string.IsNullOrWhiteSpace(GetRubyAndText(richTextBox).Item2);
        }

        private void CopyRubyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string ruby = GetRubyAndText(_richTextBoxes[_updatingSouceNumber]).Item1;
            if (!string.IsNullOrEmpty(ruby))
            {
                Clipboard.SetText(ruby);
                e.Handled = true;
            }
        }

        private void ConsultMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string text = GetSelectedText(_richTextBoxes[_updatingSouceNumber]);
            if (!string.IsNullOrWhiteSpace(text))
            {
                _dictResWindow ??= new DictResWindow(_tts);
                _dictResWindow.Search(text);
                _dictResWindow.Show();
            }
        }

        private double _scrollOffset;
        private double _maxOffset;
        private void SourceRichTextBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (sender is RichTextBox richTextBox)
            {
                _scrollOffset -= e.Delta;
                richTextBox.ScrollToHorizontalOffset(_scrollOffset);

                _scrollOffset = Math.Min(_maxOffset, _scrollOffset);
                _scrollOffset = Math.Max(0, _scrollOffset);
            }
        }

        private void SourceRichTextBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _maxOffset = Math.Max(e.HorizontalOffset, _maxOffset);
        }
    }
}
