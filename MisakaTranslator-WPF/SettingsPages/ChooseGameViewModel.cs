using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MisakaTranslator.GuidePages.Hook
{
    public class ChooseGameViewModel : INotifyPropertyChanged
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

        private string _focusingProcess = string.Empty;

        public string FocusingProcess
        {
            get
            {
                return _focusingProcess;
            }
            set
            {
                SetProperty(ref _focusingProcess, value);
            }
        }

        private bool enableSelectFocusButton;

        public bool EnableSelectFocusButton { get => enableSelectFocusButton; set => SetProperty(ref enableSelectFocusButton, value); }
    }
}