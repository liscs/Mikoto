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
            Enum.TryParse(appSettings.SelectedTTS, out TTSMode tTSMode);
            switch (tTSMode)
            {
                case TTSMode.azure:
                    AzureTTSRadio.IsChecked = true;
                    break;
                case TTSMode.local:
                    LocalTTSRadio.IsChecked = true;
                    break;
            }
        }

        private void AzureTTSRadio_Checked(object sender, RoutedEventArgs e)
        {
            appSettings.SelectedTTS = "azure";
        }

        private void LocalTTSRadio_Checked(object sender, RoutedEventArgs e)
        {
            appSettings.SelectedTTS = "local";
        }
    }
}