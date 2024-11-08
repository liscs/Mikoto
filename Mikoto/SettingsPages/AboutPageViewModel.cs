using Mikoto.Helpers.ViewModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace Mikoto.SettingsPages
{
    internal class AboutPageViewModel : ViewModelBase
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
                return $"{Application.Current.Resources["Version"]} {Common.CurrentVersion} {RuntimeInformation.ProcessArchitecture}";
            }
        }
    }
}
