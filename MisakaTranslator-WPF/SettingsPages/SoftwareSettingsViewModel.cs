namespace MisakaTranslator.SettingsPages
{
    internal class SoftwareSettingsViewModel : ViewModelBase
    {
        private bool? _enableAutoUpdateCheck;
        public bool? EnableAutoUpdateCheck
        {
            get
            {
                _enableAutoUpdateCheck = Common.AppSettings.UpdateCheckEnabled;
                return _enableAutoUpdateCheck;
            }
            set
            {
                Common.AppSettings.UpdateCheckEnabled = value ?? true;
                SetProperty(ref _enableAutoUpdateCheck, value);
            }
        }


        public string CurrentVersion => Common.CurrentVersion.ToString(3);

    }
}
