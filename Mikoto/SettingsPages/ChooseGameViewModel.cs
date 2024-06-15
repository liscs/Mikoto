namespace Mikoto.GuidePages.Hook
{
    public class ChooseGameViewModel : ViewModelBase
    {
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

        private bool enableSelectFocusButton = true;

        public bool EnableSelectFocusButton { get => enableSelectFocusButton; set => SetProperty(ref enableSelectFocusButton, value); }
    }
}