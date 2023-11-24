using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MisakaTranslator_WPF
{
    /// <summary>
    /// TransWinSettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TransWinSettingsWindow : Window
    {
        TranslateWindow translateWin;

        List<string> FontList;

        public TransWinSettingsWindow(TranslateWindow Win)
        {
            translateWin = Win;

            InitializeComponent();

            FontList = new List<string>();

            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily family in fonts.Families)
            {
                FontList.Add(family.Name);
            }

            sourceFont.ItemsSource = FontList;
            firstFont.ItemsSource = FontList;
            secondFont.ItemsSource = FontList;

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
                translateWin.SourceTextFont = FontList[sourceFont.SelectedIndex];
                Common.appSettings.TF_SrcTextFont = FontList[sourceFont.SelectedIndex];
            };

            firstFont.SelectionChanged += delegate
            {
                translateWin.FirstTransText.FontFamily = new FontFamily(FontList[firstFont.SelectedIndex]);
                Common.appSettings.TF_FirstTransTextFont = FontList[firstFont.SelectedIndex];
            };

            secondFont.SelectionChanged += delegate
            {
                translateWin.SecondTransText.FontFamily = new FontFamily(FontList[secondFont.SelectedIndex]);
                Common.appSettings.TF_SecondTransTextFont = FontList[secondFont.SelectedIndex];
            };

            sourceFontSize.ValueChanged += delegate
            {
                translateWin.SourceTextFontSize = (int)sourceFontSize.Value;
                Common.appSettings.TF_SrcTextSize = sourceFontSize.Value;
            };

            firstFontSize.ValueChanged += delegate
            {
                translateWin.FirstTransText.FontSize = firstFontSize.Value;
                Common.appSettings.TF_FirstTransTextSize = firstFontSize.Value;
            };

            secondFontSize.ValueChanged += delegate
            {
                translateWin.SecondTransText.FontSize = secondFontSize.Value;
                Common.appSettings.TF_SecondTransTextSize = secondFontSize.Value;
            };

            firstWhiteStrokeCheckBox.Click += delegate
            {
                translateWin.FirstTransText.Stroke = (bool)firstWhiteStrokeCheckBox.IsChecked ? Brushes.White : Brushes.Black;
                Common.appSettings.TF_FirstWhiteStrokeIsChecked = (bool)firstWhiteStrokeCheckBox.IsChecked;
            };

            secondWhiteStrokeCheckBox.Click += delegate
            {
                translateWin.SecondTransText.Stroke = (bool)secondWhiteStrokeCheckBox.IsChecked ? Brushes.White : Brushes.Black;
                Common.appSettings.TF_SecondWhiteStrokeIsChecked = (bool)secondWhiteStrokeCheckBox.IsChecked;
            };

            DropShadowCheckBox.Click += delegate
            {
                Common.appSettings.TF_EnableDropShadow = (bool)DropShadowCheckBox.IsChecked;
            };

            PhoneticNotationCheckBox.Click += delegate
            {
                Common.appSettings.TF_EnablePhoneticNotation = (bool)PhoneticNotationCheckBox.IsChecked;
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
                Common.appSettings.TF_PhoneticNotationType = PhoneticNotationType.Hiragana;
            };

            KatakanaRadioButton.Click += delegate
            {
                Common.appSettings.TF_PhoneticNotationType = PhoneticNotationType.Katakana;
            };

            RomajiRadioButton.Click += delegate
            {
                Common.appSettings.TF_PhoneticNotationType = PhoneticNotationType.Romaji;
            };

            KanaBoldCheckBox.Click += delegate
            {
                Common.appSettings.TF_EnableSuperBold = (bool)KanaBoldCheckBox.IsChecked;
            };

            ColorfulCheckBox.Click += delegate
            {
                Common.appSettings.TF_EnableColorful = (bool)ColorfulCheckBox.IsChecked;
            };

            ZenModeCheckBox.Click += delegate (object sender, RoutedEventArgs e)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    translateWin.TitleBar.Visibility = Visibility.Collapsed;
                    translateWin.Top += translateWin.TitleBar.Height;
                    translateWin.Height -= translateWin.TitleBar.Height;
                }
                else
                {
                    translateWin.TitleBar.Visibility = Visibility.Visible;
                    translateWin.Top -= translateWin.TitleBar.Height;
                    translateWin.Height += translateWin.TitleBar.Height;
                }
            };
        }

        /// <summary>
        /// UI初始化
        /// </summary>
        private void UI_Init()
        {
            BrushConverter brushConverter = new BrushConverter();
            BgColorBlock.Background = (Brush)brushConverter.ConvertFromString(Common.appSettings.TF_BackColor);
            firstColorBlock.Background = (Brush)brushConverter.ConvertFromString(Common.appSettings.TF_FirstTransTextColor);
            secondColorBlock.Background = (Brush)brushConverter.ConvertFromString(Common.appSettings.TF_SecondTransTextColor);

            for (int i = 0; i < FontList.Count; i++)
            {
                if (Common.appSettings.TF_SrcTextFont == FontList[i])
                {
                    sourceFont.SelectedIndex = i;
                }

                if (Common.appSettings.TF_FirstTransTextFont == FontList[i])
                {
                    firstFont.SelectedIndex = i;
                }

                if (Common.appSettings.TF_SecondTransTextFont == FontList[i])
                {
                    secondFont.SelectedIndex = i;
                }
            }

            sourceFontSize.Value = Common.appSettings.TF_SrcTextSize;
            firstFontSize.Value = Common.appSettings.TF_FirstTransTextSize;
            secondFontSize.Value = Common.appSettings.TF_SecondTransTextSize;

            firstWhiteStrokeCheckBox.IsChecked = Common.appSettings.TF_FirstWhiteStrokeIsChecked;

            DropShadowCheckBox.IsChecked = Common.appSettings.TF_EnableDropShadow;

            PhoneticNotationCheckBox.IsChecked = Common.appSettings.TF_EnablePhoneticNotation;

            Common.appSettings.TF_EnablePhoneticNotation = (bool)PhoneticNotationCheckBox.IsChecked;
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
            switch (Common.appSettings.TF_PhoneticNotationType)
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

            KanaBoldCheckBox.IsChecked = Common.appSettings.TF_EnableSuperBold;
            ColorfulCheckBox.IsChecked = Common.appSettings.TF_EnableColorful;
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
                    translateWin.LockButton.IsChecked = true;
                    BgColorBlock.Background = picker.SelectedBrush;
                    translateWin.Background = picker.SelectedBrush;
                    Common.appSettings.TF_BackColor = picker.SelectedBrush.ToString();
                }
                else if (sender == firstColorBtn)
                {
                    firstColorBlock.Background = picker.SelectedBrush;
                    translateWin.FirstTransText.Fill = picker.SelectedBrush;
                    Common.appSettings.TF_FirstTransTextColor = picker.SelectedBrush.ToString();
                }
                else if (sender == secondColorBtn)
                {
                    secondColorBlock.Background = picker.SelectedBrush;
                    translateWin.SecondTransText.Fill = picker.SelectedBrush;
                    Common.appSettings.TF_SecondTransTextColor = picker.SelectedBrush.ToString();
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
            translateWin.dtimer.Start();
            Hide();
        }
    }
}