using FontAwesome.WPF;
using MisakaTranslator.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace MisakaTranslator
{
    public class TranslateViewModel : INotifyPropertyChanged
    {
        public Window _translateWindow { get; set; }
        public TranslateViewModel(Window translateWindow)
        {
            _translateWindow = translateWindow;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (!(object.Equals(field, newValue)))
            {
                field = (newValue);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        private Visibility _sourcePanelVisibility;
        public Visibility SourcePanelVisibility
        {
            get => _sourcePanelVisibility;

            set => SetProperty(ref _sourcePanelVisibility, value);
        }

        private FontAwesomeIcon _showSourceIcon = FontAwesomeIcon.Eye;
        public FontAwesomeIcon ShowSourceIcon
        {
            get
            {
                if (Common.AppSettings.TF_ShowSourceText)
                {
                    _showSourceIcon = FontAwesomeIcon.Eye;
                    SourcePanelVisibility = Visibility.Visible;

                }
                else
                {
                    _showSourceIcon = FontAwesomeIcon.EyeSlash;
                    SourcePanelVisibility = Visibility.Collapsed;
                }
                return _showSourceIcon;
            }

            set
            {
                if (value == FontAwesomeIcon.Eye)
                {
                    Common.AppSettings.TF_ShowSourceText = true;
                    SourcePanelVisibility = Visibility.Visible;
                }
                else
                {
                    Common.AppSettings.TF_ShowSourceText = false;
                    SourcePanelVisibility = Visibility.Collapsed;

                }
                SetProperty(ref _showSourceIcon, value);
            }
        }

        private bool _copyRubyVisibility = true;
        public bool CopyRubyVisibility
        {
            get
            {
                return _copyRubyVisibility;
            }

            set
            {
                SetProperty(ref _copyRubyVisibility, value);
            }
        }

        public SuppressibleObservableCollection<string> FontList { get; set; } =
            [
            Common.AppSettings.TF_SrcTextFont,
            Common.AppSettings.TF_FirstTransTextFont,
            Common.AppSettings.TF_SecondTransTextFont
            ];


        private bool? _srcAnimationCheckEnabled;
        public bool? SrcAnimationCheckEnabled
        {
            get
            {
                _srcAnimationCheckEnabled = Common.AppSettings.TF_SrcAnimationCheckEnabled;
                return _srcAnimationCheckEnabled;
            }
            set
            {
                Common.AppSettings.TF_SrcAnimationCheckEnabled = value ?? true;
                SetProperty(ref _srcAnimationCheckEnabled, value);
            }
        }

        private bool? _transAnimationCheckEnabled;
        public bool? TransAnimationCheckEnabled
        {
            get
            {
                _transAnimationCheckEnabled = Common.AppSettings.TF_TransAnimationCheckEnabled;
                return _transAnimationCheckEnabled;
            }

            set
            {
                Common.AppSettings.TF_TransAnimationCheckEnabled = value ?? false;
                SetProperty(ref _transAnimationCheckEnabled, value);
            }
        }

        private bool? _backgroundBlurCheckEnabled;
        public bool? BackgroundBlurCheckEnabled
        {
            get
            {
                _backgroundBlurCheckEnabled = Common.AppSettings.TF_BackgroundBlurCheckEnabled;
                return _backgroundBlurCheckEnabled;
            }

            set
            {
                if (value ?? true)
                {
                    BackgroundBlurHelper.EnableBlur(_translateWindow);
                }
                else
                {
                    BackgroundBlurHelper.DisableBlur(_translateWindow);
                }

                Common.AppSettings.TF_BackgroundBlurCheckEnabled = value ?? true;
                SetProperty(ref _backgroundBlurCheckEnabled, value);
            }
        }

        private bool? _srcSingleLineEnabled = true;
        public bool? SrcSingleLineEnabled
        {
            get
            {
                _srcSingleLineEnabled = Common.AppSettings.TF_SrcSingleLineDisplay;
                return _srcSingleLineEnabled;
            }
            set
            {
                Common.AppSettings.TF_SrcSingleLineDisplay = value ?? true;
                SetProperty(ref _srcSingleLineEnabled, value);
            }
        }

        private string _sourceTextFontFamilyString = Common.AppSettings.TF_SrcTextFont;
        public string SourceTextFontFamilyString
        {
            get
            {
                _sourceTextFontFamilyString = Common.AppSettings.TF_SrcTextFont;
                return _sourceTextFontFamilyString;
            }
            set
            {
                Common.AppSettings.TF_SrcTextFont = value;
                SourceTextFontFamily = new FontFamily(value);
                SetProperty(ref _sourceTextFontFamilyString, value);
            }
        }

        private FontFamily _sourceTextFontFamily = new(Common.AppSettings.TF_SrcTextFont);
        public FontFamily SourceTextFontFamily
        {
            get
            {
                _sourceTextFontFamily = new FontFamily(SourceTextFontFamilyString);
                return _sourceTextFontFamily;
            }
            set
            {
                SetProperty(ref _sourceTextFontFamily, value);
            }
        }

        private double _sourceTextFontSize;
        public double SourceTextFontSize
        {
            get
            {
                _sourceTextFontSize = Common.AppSettings.TF_SrcTextSize;
                return _sourceTextFontSize;
            }

            set
            {
                Common.AppSettings.TF_SrcTextSize = value;
                SetProperty(ref _sourceTextFontSize, value);
            }
        }

        private string _firstTextFontFamilyString = Common.AppSettings.TF_FirstTransTextFont;
        public string FirstTextFontFamilyString
        {
            get
            {
                _firstTextFontFamilyString = Common.AppSettings.TF_FirstTransTextFont;
                return _firstTextFontFamilyString;
            }
            set
            {
                Common.AppSettings.TF_FirstTransTextFont = value;
                FirstTextFontFamily = new FontFamily(value);
                SetProperty(ref _firstTextFontFamilyString, value);
            }
        }

        private FontFamily _firstTextFontFamily = new(Common.AppSettings.TF_FirstTransTextFont);
        public FontFamily FirstTextFontFamily
        {
            get
            {
                _firstTextFontFamily = new FontFamily(FirstTextFontFamilyString);
                return _firstTextFontFamily;
            }
            set
            {
                SetProperty(ref _firstTextFontFamily, value);
            }
        }

        private double _firstTextFontSize;
        public double FirstTextFontSize
        {
            get
            {
                _firstTextFontSize = Common.AppSettings.TF_FirstTransTextSize;
                return _firstTextFontSize;
            }

            set
            {
                Common.AppSettings.TF_FirstTransTextSize = value;
                SetProperty(ref _firstTextFontSize, value);
            }
        }

        private string _secondTextFontFamilyString = Common.AppSettings.TF_SecondTransTextFont;
        public string SecondTextFontFamilyString
        {
            get
            {
                _secondTextFontFamilyString = Common.AppSettings.TF_SecondTransTextFont;
                return _secondTextFontFamilyString;
            }
            set
            {
                Common.AppSettings.TF_SecondTransTextFont = value;
                SecondTextFontFamily = new FontFamily(value);
                SetProperty(ref _secondTextFontFamilyString, value);
            }
        }

        private FontFamily _secondTextFontFamily = new(Common.AppSettings.TF_SecondTransTextFont);
        public FontFamily SecondTextFontFamily
        {
            get
            {
                _secondTextFontFamily = new FontFamily(SecondTextFontFamilyString);
                return _secondTextFontFamily;
            }
            set
            {
                SetProperty(ref _secondTextFontFamily, value);
            }
        }

        private double _secondTextFontSize;
        public double SecondTextFontSize
        {
            get
            {
                _secondTextFontSize = Common.AppSettings.TF_SecondTransTextSize;
                return _secondTextFontSize;
            }

            set
            {
                Common.AppSettings.TF_SecondTransTextSize = value;
                SetProperty(ref _secondTextFontSize, value);
            }
        }
    }
}