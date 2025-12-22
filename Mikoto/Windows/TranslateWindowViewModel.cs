using Mikoto.Config;
using Mikoto.Helpers.Container;
using Mikoto.Helpers.Graphics;
using Mikoto.Helpers.ViewModel;
using System.Windows;

namespace Mikoto.Windows
{
    internal class TranslateWindowViewModel : ViewModelBase
    {
        private IAppSettings _appSettings = Common.AppSettings;
        public IAppSettings AppSettings
        {
            get
            {
                return _appSettings;
            }

            set
            {
                SetProperty(ref _appSettings, value);
            }
        }

        private Window _translateWindow;
        public TranslateWindowViewModel(Window translateWindow)
        {
            _translateWindow = translateWindow;
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