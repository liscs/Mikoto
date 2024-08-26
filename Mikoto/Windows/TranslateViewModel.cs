using Mikoto.Helpers;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace Mikoto
{
    public class TranslateViewModel : ViewModelBase
    {
        private Window _translateWindow;
        public TranslateViewModel(Window translateWindow)
        {
            _translateWindow = translateWindow;
        }

        private Visibility _sourcePanelVisibility;
        public Visibility SourcePanelVisibility
        {
            get => _sourcePanelVisibility;

            set
            {
                switch (value)
                {
                    case Visibility.Visible:
                        TextTopMargin = new(0, 0, 0, 0);
                        break;
                    case Visibility.Hidden:
                    case Visibility.Collapsed:
                        TextTopMargin = new(0, 10, 0, 0);
                        break;
                }

                SetProperty(ref _sourcePanelVisibility, value);
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

        private string _sourceTextFontFamilyString = new FontFamily(Common.AppSettings.TF_SrcTextFont).GetLocalizedName();
        public string SourceTextFontFamilyString
        {
            get
            {
                return _sourceTextFontFamilyString;
            }
            set
            {
                FontFamily font = new FontFamily(value);
                Common.AppSettings.TF_SrcTextFont = font.Source;
                SourceTextFontFamily = font;
                SetProperty(ref _sourceTextFontFamilyString, font.GetLocalizedName());
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

        private string _firstTextFontFamilyString = new FontFamily(Common.AppSettings.TF_FirstTransTextFont).GetLocalizedName();
        public string FirstTextFontFamilyString
        {
            get
            {
                return _firstTextFontFamilyString;
            }
            set
            {
                FontFamily font = new FontFamily(value);
                Common.AppSettings.TF_FirstTransTextFont = font.Source;
                FirstTextFontFamily = font;
                SetProperty(ref _firstTextFontFamilyString, font.GetLocalizedName());
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

        private string _secondTextFontFamilyString = new FontFamily(Common.AppSettings.TF_SecondTransTextFont).GetLocalizedName();
        public string SecondTextFontFamilyString
        {
            get
            {
                return _secondTextFontFamilyString;
            }
            set
            {
                FontFamily font = new FontFamily(value);
                Common.AppSettings.TF_SecondTransTextFont = font.Source;
                SecondTextFontFamily = font;
                SetProperty(ref _secondTextFontFamilyString, font.GetLocalizedName());
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

        private SolidColorBrush sourceTextColor = Brushes.White;

        public SolidColorBrush SourceTextColor
        {
            get
            {
                return sourceTextColor;
            }

            set
            {
                Common.AppSettings.TF_SrcTextColor = value.ToString();
                SetProperty(ref sourceTextColor, value);
            }
        }

        private double firstTextStrokeThickness = 0.7;

        public double FirstTextStrokeThickness
        {
            get => firstTextStrokeThickness;
            set
            {
                Common.AppSettings.TF_FirstTextStrokeThickness = value;
                SetProperty(ref firstTextStrokeThickness, value);
            }
        }

        private FontWeight firstTextFontWeight = FontWeights.Bold;

        public FontWeight FirstTextFontWeight { get => firstTextFontWeight; set => SetProperty(ref firstTextFontWeight, value); }

        public int FirstTextFontWeightOpenTypeWeight
        {
            get => firstTextFontWeight.ToOpenTypeWeight();
            set
            {
                Common.AppSettings.TF_FirstTextFontWeight = value;
                FirstTextFontWeight = FontWeight.FromOpenTypeWeight(value);
                SetProperty(ref firstTextFontWeight, FirstTextFontWeight);
            }
        }

        private double secondTextStrokeThickness = 0.7;

        public double SecondTextStrokeThickness
        {
            get => secondTextStrokeThickness;
            set
            {
                Common.AppSettings.TF_SecondTextStrokeThickness = value;
                SetProperty(ref secondTextStrokeThickness, value);
            }
        }

        private FontWeight secondTextFontWeight = FontWeights.Bold;

        public FontWeight SecondTextFontWeight { get => secondTextFontWeight; set => SetProperty(ref secondTextFontWeight, value); }

        public int SecondTextFontWeightOpenTypeWeight
        {
            get => secondTextFontWeight.ToOpenTypeWeight();
            set
            {
                Common.AppSettings.TF_SecondTextFontWeight = value;
                SecondTextFontWeight = FontWeight.FromOpenTypeWeight(value);
                SetProperty(ref secondTextFontWeight, SecondTextFontWeight);
            }
        }

        private Thickness textTopMargin = new(0, 0, 0, 0);

        public Thickness TextTopMargin
        {
            get => textTopMargin;
            set => SetProperty(ref textTopMargin, value);
        }
    }

    public static class FontExtension
    {
        public static string GetLocalizedName(this FontFamily font)
        {
            if (font.FamilyNames.TryGetValue(XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name), out string value))
            {
                return value;
            }
            else
            {
                return font.FamilyNames.First().Value;
            }
        }

    }
}