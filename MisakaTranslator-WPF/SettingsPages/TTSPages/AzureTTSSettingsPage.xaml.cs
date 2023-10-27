using System.Windows;
using System.Windows.Controls;
using TTSHelperLibrary.TTSGenerator;

namespace MisakaTranslator_WPF.SettingsPages.TTSPages
{
    /// <summary>
    /// AzureTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class AzureTTSSettingsPage : Page
    {
        public AzureTTSSettingsPage()
        {
            InitializeComponent();
            AzureTTSSecretKeyBox.Text = Common.appSettings.AzureTTSSecretKey;
            AzureTTSLocationBox.Text = Common.appSettings.AzureTTSLocation;
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(AzureTTS.GetUrl_allpyAPI());
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(AzureTTS.GetUrl_bill());
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            AzureTTS azureTTS = new AzureTTS();
            azureTTS.TTSInit(Common.appSettings.AzureTTSSecretKey, Common.appSettings.AzureTTSLocation);
            await azureTTS.TextToSpeechAsync(TestSrcText.Text, TestDstVoice.Text);
            if(azureTTS.ErrorMessage != null)
            {
                HandyControl.Controls.Growl.Error(azureTTS.ErrorMessage);
            }
        }
    }
}