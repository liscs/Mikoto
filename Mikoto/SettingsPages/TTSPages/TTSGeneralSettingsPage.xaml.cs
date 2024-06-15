using System.Windows;
using System.Windows.Controls;
using static Mikoto.Common;

namespace Mikoto.SettingsPages.TTSPages
{
    /// <summary>
    /// AzureTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class TTSGeneralSettingsPage : Page
    {
        public TTSGeneralSettingsPage()
        {
            InitializeComponent();
            switch (AppSettings.SelectedTTS)
            {
                case TTSMode.Azure:
                    AzureTTSRadio.IsChecked = true;
                    break;
                case TTSMode.Local:
                    LocalTTSRadio.IsChecked = true;
                    break;
            }
        }

        private void AzureTTSRadio_Checked(object sender, RoutedEventArgs e)
        {
            AppSettings.SelectedTTS = TTSMode.Azure;
        }

        private void LocalTTSRadio_Checked(object sender, RoutedEventArgs e)
        {
            AppSettings.SelectedTTS = TTSMode.Local;
        }
    }
}