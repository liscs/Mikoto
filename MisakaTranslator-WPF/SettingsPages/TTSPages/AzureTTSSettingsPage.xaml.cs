using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TTSHelperLibrary;

namespace MisakaTranslator_WPF.SettingsPages.TTSPages
{
    /// <summary>
    /// AzureTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class AzureTTSSettingsPage : Page
    {
        AzureTTS azureTTS = new AzureTTS();
        public AzureTTSSettingsPage()
        {
            InitializeComponent();
            AzureTTSSecretKeyBox.Text = Common.appSettings.AzureTTSSecretKey;
            AzureTTSLocationBox.Text = Common.appSettings.AzureTTSLocation;
            HttpProxyBox.Text = Common.appSettings.AzureTTSProxy;
            TestDstVoice.Text = Common.appSettings.AzureTTSVoice;
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AzureTTS.GetUrl_allpyAPI()) { UseShellExecute = true });
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AzureTTS.GetUrl_bill()) { UseShellExecute = true });
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.appSettings.AzureTTSSecretKey = AzureTTSSecretKeyBox.Text;
            Common.appSettings.AzureTTSLocation = AzureTTSLocationBox.Text;
            azureTTS.ProxyString = Common.appSettings.AzureTTSProxy;
            azureTTS.TTSInit(Common.appSettings.AzureTTSSecretKey, Common.appSettings.AzureTTSLocation, Common.appSettings.AzureTTSVoice);
            await azureTTS.SpeakAsync(TestSrcText.Text);
            if (azureTTS.ErrorMessage != string.Empty)
            {
                HandyControl.Controls.Growl.Error(azureTTS.ErrorMessage);
            }
        }

        private void HttpProxyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string text = HttpProxyBox.Text.Trim();
            Common.appSettings.AzureTTSProxy = text;
        }

        private void TestDstVoice_LostFocus(object sender, RoutedEventArgs e)
        {
            Common.appSettings.AzureTTSVoice = TestDstVoice.Text;
        }

        private void VoiceNameQuery_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AzureTTS.GetUrl_VoiceList()) { UseShellExecute = true });
        }
    }
}