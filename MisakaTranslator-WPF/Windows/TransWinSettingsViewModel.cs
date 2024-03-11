using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MisakaTranslator
{
    public class TransWinSettingsViewModel : INotifyPropertyChanged
    {
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
    }
}