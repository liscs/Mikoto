using FontAwesome.WPF;
using MisakaTranslator.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

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

        public SuppressibleObservableCollection<string> FontList { get; set; } = new();


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
    }
}