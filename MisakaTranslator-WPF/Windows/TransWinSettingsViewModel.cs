using MisakaTranslator.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MisakaTranslator
{
    public class TransWinSettingsViewModel : INotifyPropertyChanged
    {
        readonly Window _translateWindow;
        public TransWinSettingsViewModel(Window translateWindow)
        {
            _translateWindow = translateWindow;
        }

        public ObservableCollection<string> FontList { get; set; } = new();

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
    }
}