using MisakaTranslator.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace MisakaTranslator
{
    public class TranslateViewModel : INotifyPropertyChanged
    {
        private Window _translateWindow;
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

        public SuppressibleObservableCollection<string> FontList { get; set; } = [];


        private bool? _srcAnimationCheckEnabled = Common.AppSettings.TF_SrcAnimationCheckEnabled;
        public bool? SrcAnimationCheckEnabled
        {
            get
            {
                return _srcAnimationCheckEnabled;
            }
            set
            {
                Common.AppSettings.TF_SrcAnimationCheckEnabled = value ?? true;
                SetProperty(ref _srcAnimationCheckEnabled, value);
            }
        }

        private bool? _transAnimationCheckEnabled = Common.AppSettings.TF_TransAnimationCheckEnabled;
        public bool? TransAnimationCheckEnabled
        {
            get
            {
                return _transAnimationCheckEnabled;
            }

            set
            {
                Common.AppSettings.TF_TransAnimationCheckEnabled = value ?? false;
                SetProperty(ref _transAnimationCheckEnabled, value);
            }
        }

        private bool? _backgroundBlurCheckEnabled = Common.AppSettings.TF_BackgroundBlurCheckEnabled;
        public bool? BackgroundBlurCheckEnabled
        {
            get
            {
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

        private bool? _srcSingleLineEnabled = Common.AppSettings.TF_SrcSingleLineDisplay;
        public bool? SrcSingleLineEnabled
        {
            get
            {
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
                return _sourceTextFontFamily;
            }
            set
            {
                SetProperty(ref _sourceTextFontFamily, value);
            }
        }

        private double _sourceTextFontSize = Common.AppSettings.TF_SrcTextSize;
        public double SourceTextFontSize
        {
            get
            {
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
                return _firstTextFontFamily;
            }
            set
            {
                SetProperty(ref _firstTextFontFamily, value);
            }
        }

        private double _firstTextFontSize = Common.AppSettings.TF_FirstTransTextSize;
        public double FirstTextFontSize
        {
            get
            {
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
                return _secondTextFontFamily;
            }
            set
            {
                SetProperty(ref _secondTextFontFamily, value);
            }
        }

        private double _secondTextFontSize = Common.AppSettings.TF_SecondTransTextSize;
        public double SecondTextFontSize
        {
            get
            {
                return _secondTextFontSize;
            }

            set
            {
                Common.AppSettings.TF_SecondTransTextSize = value;
                SetProperty(ref _secondTextFontSize, value);
            }
        }

        private string pauseButtonIconText = "\uF8AE";

        public string PauseButtonIconText { get => pauseButtonIconText; set => SetProperty(ref pauseButtonIconText, value); }

        private string showSourceButtonIconText = "\uE8C5";

        public string ShowSourceButtonIconText
        {
            get
            {
                if (Common.AppSettings.TF_ShowSourceText)
                {
                    showSourceButtonIconText = "\uE8C5";
                    SourcePanelVisibility = Visibility.Visible;
                }
                else
                {
                    showSourceButtonIconText = "\uE8C4";
                    SourcePanelVisibility = Visibility.Collapsed;
                }
                return showSourceButtonIconText;
            }

            set
            {
                if (value.ToString() == "\uE8C5")
                {
                    Common.AppSettings.TF_ShowSourceText = true;
                    SourcePanelVisibility = Visibility.Visible;
                }
                else
                {
                    Common.AppSettings.TF_ShowSourceText = false;
                    SourcePanelVisibility = Visibility.Collapsed;

                }
                SetProperty(ref showSourceButtonIconText, value);
            }
        }
    }
}