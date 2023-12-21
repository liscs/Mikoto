using Microsoft.CognitiveServices.Speech;
using Microsoft.Scripting.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private AzureTTS azureTTS = new AzureTTS();
        private const string DEFAULT_VOICE = "ja-JP-NanamiNeural";

        public AzureTTSSettingsPage()
        {
            InitializeComponent();
            AzureTTSSecretKeyBox.Text = Common.AppSettings.AzureTTSSecretKey;
            AzureTTSLocationBox.Text = Common.AppSettings.AzureTTSLocation;
            HttpProxyBox.Text = Common.AppSettings.AzureTTSProxy;
            azureTTS.TTSInit(Common.AppSettings.AzureTTSSecretKey, Common.AppSettings.AzureTTSLocation, Common.AppSettings.AzureTTSVoice);
            SetSavedVoice();
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
            azureTTS.ProxyString = Common.AppSettings.AzureTTSProxy;
            azureTTS.TTSInit(Common.AppSettings.AzureTTSSecretKey, Common.AppSettings.AzureTTSLocation, Common.AppSettings.AzureTTSVoice);
            await azureTTS.SpeakAsync(TestSrcText.Text);
            if (azureTTS.ErrorMessage != string.Empty)
            {
                HandyControl.Controls.Growl.Error(azureTTS.ErrorMessage);
            }
        }

        private void HttpProxyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string text = HttpProxyBox.Text.Trim();
            Common.AppSettings.AzureTTSProxy = text;
        }

        private void VoiceNameComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (Voices.Count == 0) { return; }
            Common.AppSettings.AzureTTSVoice = $"{VoiceLocalComboBox.SelectedItem}-{VoiceNameComboBox.SelectedItem}";
        }

        private void VoiceNameQuery_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AzureTTS.GetUrl_VoiceList()) { UseShellExecute = true });
        }

        public List<VoiceInfo> Voices { get; set; } = new();

        private async void GetVoices(object sender, RoutedEventArgs e)
        {
            SynthesisVoicesResult synthesisVoicesResult = await azureTTS.GetVoices();
            Voices = synthesisVoicesResult.Voices.ToList();
            if (Voices.Count != 0)
            {
                VoiceLocalComboBox.ItemsSource = Voices.Select(p => p.Locale).Distinct();
                UpdateVoiceNameComboBox(VoiceLocalComboBox.SelectedItem.ToString());
                SelectSavedVoice();
            }
        }

        private void SelectSavedVoice()
        {
            if (string.IsNullOrEmpty(Common.AppSettings.AzureTTSVoice))
            {
                VoiceLocalComboBox.SelectedItem = GetVoiceLocale(DEFAULT_VOICE);
                VoiceNameComboBox.SelectedItem = GetVoiceName(DEFAULT_VOICE);
            }
            else
            {
                VoiceLocalComboBox.SelectedItem = GetVoiceLocale(Common.AppSettings.AzureTTSVoice);
                VoiceNameComboBox.SelectedItem = GetVoiceName(Common.AppSettings.AzureTTSVoice);
            }
        }

        void SetSavedVoice()
        {
            if (string.IsNullOrEmpty(Common.AppSettings.AzureTTSVoice))
            {
                VoiceLocalComboBox.ItemsSource = new List<string> { GetVoiceLocale(DEFAULT_VOICE) };
                VoiceNameComboBox.ItemsSource = new List<string> { GetVoiceName(DEFAULT_VOICE) };
            }
            else
            {
                VoiceLocalComboBox.ItemsSource = new List<string> { GetVoiceLocale(Common.AppSettings.AzureTTSVoice) };
                VoiceNameComboBox.ItemsSource = new List<string> { GetVoiceName(Common.AppSettings.AzureTTSVoice) };
            }
            VoiceLocalComboBox.SelectedIndex = 0;
            VoiceNameComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// 筛选指定地区的语音
        /// </summary>
        private void UpdateVoiceNameComboBox(string locale)
        {
            if (VoiceNameComboBox == null || Voices.Count == 0) { return; }
            VoiceNameComboBox.ItemsSource = Voices.Where(p => p.Locale == locale).Select(p => GetVoiceName(p.ShortName));
            VoiceNameComboBox.SelectedIndex = 0;
        }

        private static string GetVoiceLocale(string voiceString) => voiceString.Substring(0, voiceString.LastIndexOf('-'));
        private static string GetVoiceName(string voiceString) => voiceString.Substring(voiceString.LastIndexOf('-') + 1);

        private void VoiceLocalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateVoiceNameComboBox(e.AddedItems[0].ToString());
        }
    }
}