using Microsoft.Scripting.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Text;
using System.Linq;
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
        private TranslateViewModel _viewModel;
        private TranslateWindow _translateWin;

        public TransWinSettingsWindow(TranslateWindow Win)
        {
            InitializeComponent();
            _translateWin = Win;
            _viewModel = Win.ViewModel;
            DataContext = _viewModel;

            EventInit();

            UI_Init();

            this.Topmost = true;
        }

        private void FontListInit()
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
               {
                   InstalledFontCollection fonts = new();
                   _viewModel.FontList.SuppressNotification = true;
                   _viewModel.FontList.AddRange(fonts.Families.Select(p => p.Name));
                   _viewModel.FontList.SuppressNotification = false;
                   _viewModel.SourceTextFontFamilyString = Common.AppSettings.TF_SrcTextFont;
                   _viewModel.FirstTextFontFamilyString = Common.AppSettings.TF_FirstTransTextFont;
                   _viewModel.SecondTextFontFamilyString = Common.AppSettings.TF_SecondTransTextFont;
               });
        }

        /// <summary>
        /// 事件初始化
        /// </summary>
        private void EventInit()
        {
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
            Hide();
        }


        bool _fontInited = false;

        private void Font_ContextMenuOpening(object sender, System.EventArgs e)
        {
            if (_fontInited == false)
            {
                FontListInit();
                _fontInited = true;
            }
        }
    }
}