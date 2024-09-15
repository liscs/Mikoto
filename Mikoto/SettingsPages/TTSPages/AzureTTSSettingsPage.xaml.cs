using HandyControl.Controls;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Scripting.Utils;
using Mikoto.TTS;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto.SettingsPages.TTSPages
{
    /// <summary>
    /// AzureTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class AzureTTSSettingsPage : Page
    {
        private AzureTTSSettingsPageViewModel _viewModel = new();

        private AzureTTS azureTTS;

        public AzureTTSSettingsPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
            AzureTTSSecretKeyBox.Text = Common.AppSettings.AzureTTSSecretKey;
            AzureTTSLocationBox.Text = Common.AppSettings.AzureTTSLocation;
            HttpProxyBox.Text = Common.AppSettings.AzureTTSProxy;
            _viewModel.Volume = Common.AppSettings.AzureTTSVoiceVolume;
            _viewModel.EnableAutoSpeak = Common.AppSettings.AzureEnableAutoSpeak;

            azureTTS = new(Common.AppSettings.AzureTTSSecretKey, Common.AppSettings.AzureTTSLocation, Common.AppSettings.AzureTTSVoice, Common.AppSettings.AzureTTSVoiceVolume, Common.AppSettings.AzureTTSVoiceStyle, Common.AppSettings.AzureTTSProxy);
            GetVoices(this, null);
        }


        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AzureTTS.GetUrl_API()) { UseShellExecute = true });
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AzureTTS.GetUrl_Bill()) { UseShellExecute = true });
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.AzureTTSSecretKey = AzureTTSSecretKeyBox.Text;
            Common.AppSettings.AzureTTSLocation = AzureTTSLocationBox.Text;
            Common.AppSettings.AzureTTSVoice = _viewModel.SelectedVoice?.Name ?? Common.AppSettings.AzureTTSVoice;
            Common.AppSettings.AzureTTSVoiceStyle = _viewModel.SelectedVoiceStyle ?? Common.AppSettings.AzureTTSVoiceStyle;
            Common.AppSettings.AzureTTSVoiceVolume = _viewModel.Volume;

            azureTTS = new(Common.AppSettings.AzureTTSSecretKey, Common.AppSettings.AzureTTSLocation, Common.AppSettings.AzureTTSVoice, Common.AppSettings.AzureTTSVoiceVolume, Common.AppSettings.AzureTTSVoiceStyle, Common.AppSettings.AzureTTSProxy);
            await azureTTS.SpeakAsync(TestSrcText.Text);
            if (!string.IsNullOrEmpty(azureTTS.ErrorMessage))
            {
                Growl.Error(azureTTS.ErrorMessage);
            }
        }

        private void HttpProxyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string text = HttpProxyBox.Text.Trim();
            Common.AppSettings.AzureTTSProxy = text;
        }

        private void VoiceNameComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Voices.Count == 0) { return; }
        }

        private void VoiceNameQuery_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AzureTTS.GetUrl_VoiceList()) { UseShellExecute = true });
        }


        private async void GetVoices(object sender, RoutedEventArgs? e)
        {
            SynthesisVoicesResult? synthesisVoicesResult = await azureTTS.GetVoices();
            if (synthesisVoicesResult == null)
            {
                Growl.Info(Application.Current.Resources["TTSSettingsPage_AzureSettingErrorInfo"].ToString());
                return;
            }
            _viewModel.Voices = synthesisVoicesResult.Voices.ToList();
            SetSavedVoice(Common.AppSettings.AzureTTSVoice, Common.AppSettings.AzureTTSVoiceStyle);
        }

        private void SetSavedVoice(string savedVoice, string savedStyle)
        {
            _viewModel.SelectedVoice = _viewModel.Voices.First(p => p.Name == savedVoice);
            _viewModel.SelectedVoiceStyle = savedStyle;
        }

        private void VoiceLocaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                _viewModel.VoiceNames = _viewModel.Voices.Where(p => p.Locale == e.AddedItems[0] as string).Select(p => p.LocalName);
            }
        }

        private void VoiceNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                _viewModel.SelectedVoice = _viewModel.Voices.First(p => p.LocalName == e.AddedItems[0] as string);
                _viewModel.VoiceStyles = _viewModel.SelectedVoice.StyleList;
            }
        }

        private void VoiceStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                _viewModel.SelectedVoiceStyle = e.AddedItems[0] as string ?? string.Empty;
            }
        }
    }
}