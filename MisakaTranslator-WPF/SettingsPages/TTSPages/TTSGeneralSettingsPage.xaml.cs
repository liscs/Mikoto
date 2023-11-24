using System;
using System.Windows;
using System.Windows.Controls;
using static MisakaTranslator_WPF.Common;

namespace MisakaTranslator_WPF.SettingsPages.TTSPages
{
    /// <summary>
    /// AzureTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class TTSGeneralSettingsPage : Page
    {
        public TTSGeneralSettingsPage()
        {
            InitializeComponent();
            switch (appSettings.SelectedTTS)
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
            appSettings.SelectedTTS = TTSMode.Azure;
        }

        private void LocalTTSRadio_Checked(object sender, RoutedEventArgs e)
        {
            appSettings.SelectedTTS = TTSMode.Local;
        }
    }
}