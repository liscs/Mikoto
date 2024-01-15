using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MisakaTranslator.SettingsPages
{
    class SoftwareSettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        private bool? enableAutoUpdateCheck;
        public bool? EnableAutoUpdateCheck
        {
            get
            {
                enableAutoUpdateCheck = Common.AppSettings.UpdateCheckEnabled;
                return enableAutoUpdateCheck;
            }
            set
            {
                Common.AppSettings.UpdateCheckEnabled = value ?? true;
                SetProperty(ref enableAutoUpdateCheck, value);
            }
        }

    }
}
