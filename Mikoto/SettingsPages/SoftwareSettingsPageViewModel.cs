using Mikoto.Helpers.ViewModel;
using System.Runtime.InteropServices;

namespace Mikoto.SettingsPages
{
    internal class SoftwareSettingsPageViewModel : ViewModelBase
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


        public string CurrentVersion
        {
            get
            {
                return string.Join(' ', "v" + Common.CurrentVersion.ToString(3), RuntimeInformation.ProcessArchitecture);
            }
        }
    }
}
