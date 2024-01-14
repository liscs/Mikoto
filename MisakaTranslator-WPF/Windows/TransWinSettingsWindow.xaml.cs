using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MisakaTranslator
{
    /// <summary>
    /// TransWinSettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TransWinSettingsWindow : Window
    {
        private TranslateWindow _translateWin;
        private List<string> _fontList;

        public TransWinSettingsWindow(TranslateWindow Win)
        {
            _translateWin = Win;

            InitializeComponent();

            _fontList = new List<string>();

            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily family in fonts.Families)
            {
                _fontList.Add(family.Name);
            }

            sourceFont.ItemsSource = _fontList;
            firstFont.ItemsSource = _fontList;
            secondFont.ItemsSource = _fontList;

            EventInit();

            UI_Init();

            this.Topmost = true;
        }

        /// <summary>
        /// 事件初始化
        /// </summary>
        private void EventInit()
        {
            sourceFont.SelectionChanged += delegate
            {
                _translateWin.SourceTextFont = _fontList[sourceFont.SelectedIndex];
                Common.AppSettings.TF_SrcTextFont = _fontList[sourceFont.SelectedIndex];
            };

            firstFont.SelectionChanged += delegate
            {
                _translateWin.FirstTransText.FontFamily = new FontFamily(_fontList[firstFont.SelectedIndex]);
                Common.AppSettings.TF_FirstTransTextFont = _fontList[firstFont.SelectedIndex];
            };

            secondFont.SelectionChanged += delegate
            {
                _translateWin.SecondTransText.FontFamily = new FontFamily(_fontList[secondFont.SelectedIndex]);
                Common.AppSettings.TF_SecondTransTextFont = _fontList[secondFont.SelectedIndex];
            };

            sourceFontSize.ValueChanged += delegate
            {
                _translateWin.SourceTextFontSize = (int)sourceFontSize.Value;
                Common.AppSettings.TF_SrcTextSize = sourceFontSize.Value;
            };

            firstFontSize.ValueChanged += delegate
            {
                _translateWin.FirstTransText.FontSize = firstFontSize.Value;
                Common.AppSettings.TF_FirstTransTextSize = firstFontSize.Value;
            };

            secondFontSize.ValueChanged += delegate
            {
                _translateWin.SecondTransText.FontSize = secondFontSize.Value;
                Common.AppSettings.TF_SecondTransTextSize = secondFontSize.Value;
            };

            firstWhiteStrokeCheckBox.Click += delegate
            {
                _translateWin.FirstTransText.Stroke = firstWhiteStrokeCheckBox.IsChecked switch
                {
                    true => Brushes.White,
                    null or false => Brushes.Black
                };
                Common.AppSettings.TF_FirstWhiteStrokeIsChecked = firstWhiteStrokeCheckBox.IsChecked ?? false;
            };

            secondWhiteStrokeCheckBox.Click += delegate
            {
                _translateWin.SecondTransText.Stroke = secondWhiteStrokeCheckBox.IsChecked switch
                {
                    true => _translateWin.SecondTransText.Stroke = Brushes.White,
                    null or false => _translateWin.SecondTransText.Stroke = Brushes.Black
                };

                Common.AppSettings.TF_SecondWhiteStrokeIsChecked = secondWhiteStrokeCheckBox.IsChecked ?? false;
            };

            DropShadowCheckBox.Click += delegate
            {
                Common.AppSettings.TF_EnableDropShadow = DropShadowCheckBox.IsChecked ?? false;
            };

            PhoneticNotationCheckBox.Click += delegate
            {
                Common.AppSettings.TF_EnablePhoneticNotation = PhoneticNotationCheckBox.IsChecked ?? false;
                if (PhoneticNotationCheckBox.IsChecked == true)
                {
                    HiraganaRadioButton.IsEnabled = true;
                    KatakanaRadioButton.IsEnabled = true;
                    RomajiRadioButton.IsEnabled = true;
                }
                else
                {
                    HiraganaRadioButton.IsEnabled = false;
                    KatakanaRadioButton.IsEnabled = false;
                    RomajiRadioButton.IsEnabled = false;
                }
            };

            HiraganaRadioButton.Click += delegate
            {
                Common.AppSettings.TF_PhoneticNotationType = PhoneticNotationType.Hiragana;
            };

            KatakanaRadioButton.Click += delegate
            {
                Common.AppSettings.TF_PhoneticNotationType = PhoneticNotationType.Katakana;
            };

            RomajiRadioButton.Click += delegate
            {
                Common.AppSettings.TF_PhoneticNotationType = PhoneticNotationType.Romaji;
            };

            KanaBoldCheckBox.Click += delegate
            {
                Common.AppSettings.TF_EnableSuperBold = KanaBoldCheckBox.IsChecked ?? false;
            };

            ColorfulCheckBox.Click += delegate
            {
                Common.AppSettings.TF_EnableColorful = ColorfulCheckBox.IsChecked ?? false;
            };

            ZenModeCheckBox.Click += delegate (object sender, RoutedEventArgs e)
            {
                if ((sender as CheckBox)?.IsChecked ?? false)
                {
                    _translateWin.TitleBar.Visibility = Visibility.Collapsed;
                    _translateWin.Top += _translateWin.TitleBar.Height;
                    _translateWin.Height -= _translateWin.TitleBar.Height;
                }
                else
                {
                    _translateWin.TitleBar.Visibility = Visibility.Visible;
                    _translateWin.Top -= _translateWin.TitleBar.Height;
                    _translateWin.Height += _translateWin.TitleBar.Height;
                }
            };
        }

        /// <summary>
        /// UI初始化
        /// </summary>
        private void UI_Init()
        {
            BrushConverter brushConverter = new BrushConverter();
            BgColorBlock.Background = brushConverter.ConvertFromString(Common.AppSettings.TF_BackColor) as Brush;
            firstColorBlock.Background = brushConverter.ConvertFromString(Common.AppSettings.TF_FirstTransTextColor) as Brush;
            secondColorBlock.Background = brushConverter.ConvertFromString(Common.AppSettings.TF_SecondTransTextColor) as Brush;

            for (int i = 0; i < _fontList.Count; i++)
            {
                if (Common.AppSettings.TF_SrcTextFont == _fontList[i])
                {
                    sourceFont.SelectedIndex = i;
                }

                if (Common.AppSettings.TF_FirstTransTextFont == _fontList[i])
                {
                    firstFont.SelectedIndex = i;
                }

                if (Common.AppSettings.TF_SecondTransTextFont == _fontList[i])
                {
                    secondFont.SelectedIndex = i;
                }
            }

            sourceFontSize.Value = Common.AppSettings.TF_SrcTextSize;
            firstFontSize.Value = Common.AppSettings.TF_FirstTransTextSize;
            secondFontSize.Value = Common.AppSettings.TF_SecondTransTextSize;

            firstWhiteStrokeCheckBox.IsChecked = Common.AppSettings.TF_FirstWhiteStrokeIsChecked;

            DropShadowCheckBox.IsChecked = Common.AppSettings.TF_EnableDropShadow;

            PhoneticNotationCheckBox.IsChecked = Common.AppSettings.TF_EnablePhoneticNotation;

            Common.AppSettings.TF_EnablePhoneticNotation = PhoneticNotationCheckBox.IsChecked ?? false;
            if (PhoneticNotationCheckBox.IsChecked == true)
            {
                HiraganaRadioButton.IsEnabled = true;
                KatakanaRadioButton.IsEnabled = true;
                RomajiRadioButton.IsEnabled = true;
            }
            else
            {
                HiraganaRadioButton.IsEnabled = false;
                KatakanaRadioButton.IsEnabled = false;
                RomajiRadioButton.IsEnabled = false;
            }
            switch (Common.AppSettings.TF_PhoneticNotationType)
            {
                case PhoneticNotationType.Hiragana:
                    HiraganaRadioButton.IsChecked = true;
                    break;
                case PhoneticNotationType.Katakana:
                    KatakanaRadioButton.IsChecked = true;
                    break;
                case PhoneticNotationType.Romaji:
                    RomajiRadioButton.IsChecked = true;
                    break;
                default:
                    break;
            }

            KanaBoldCheckBox.IsChecked = Common.AppSettings.TF_EnableSuperBold;
            ColorfulCheckBox.IsChecked = Common.AppSettings.TF_EnableColorful;
        }

        private void ChooseColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var picker = HandyControl.Tools.SingleOpenHelper.CreateControl<HandyControl.Controls.ColorPicker>();
            var window = new HandyControl.Controls.PopupWindow
            {
                PopupElement = picker,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None,
                MinWidth = 0,
                MinHeight = 0,
                Title = Application.Current.Resources["TransWinSettingsWin_BtnChooseColor"].ToString(),
                Owner = this
            };
            picker.Confirmed += delegate
            {
                if (sender == BgColorBtn)
                {
                    _translateWin.LockButton.IsChecked = true;
                    BgColorBlock.Background = picker.SelectedBrush;
                    _translateWin.Background = picker.SelectedBrush;
                    Common.AppSettings.TF_BackColor = picker.SelectedBrush.ToString();
                }
                else if (sender == firstColorBtn)
                {
                    firstColorBlock.Background = picker.SelectedBrush;
                    _translateWin.FirstTransText.Fill = picker.SelectedBrush;
                    Common.AppSettings.TF_FirstTransTextColor = picker.SelectedBrush.ToString();
                }
                else if (sender == secondColorBtn)
                {
                    secondColorBlock.Background = picker.SelectedBrush;
                    _translateWin.SecondTransText.Fill = picker.SelectedBrush;
                    Common.AppSettings.TF_SecondTransTextColor = picker.SelectedBrush.ToString();
                }
                window.Close();
            };
            picker.Canceled += delegate
            {
                window.Close();
            };
            window.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            _translateWin.DispatcherTimer.Start();
            Hide();
        }
    }
}