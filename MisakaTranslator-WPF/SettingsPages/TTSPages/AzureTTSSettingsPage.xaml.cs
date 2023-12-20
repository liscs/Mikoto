using Microsoft.CognitiveServices.Speech;
using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Speech.Recognition;
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
            azureTTS.TTSInit(Common.appSettings.AzureTTSSecretKey, Common.appSettings.AzureTTSLocation, Common.appSettings.AzureTTSVoice);
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

        private void VoiceNameComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (Voices.Count == 0) { return; }
            Common.appSettings.AzureTTSVoice = $"{VoiceLocalComboBox.SelectedItem}-{VoiceNameComboBox.SelectedItem}";
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

        private const string DEFAULT_VOICE = "ja-JP-NanamiNeural";

        private void SelectSavedVoice()
        {
            if (string.IsNullOrEmpty(Common.appSettings.AzureTTSVoice))
            {
                VoiceLocalComboBox.SelectedItem = GetVoiceLocal(DEFAULT_VOICE);
                VoiceNameComboBox.SelectedItem = GetVoiceName(DEFAULT_VOICE);
            }
            else
            {
                VoiceLocalComboBox.SelectedItem = GetVoiceLocal(Common.appSettings.AzureTTSVoice);
                VoiceNameComboBox.SelectedItem = GetVoiceName(Common.appSettings.AzureTTSVoice);
            }
        }

        void SetSavedVoice()
        {
            if (string.IsNullOrEmpty(Common.appSettings.AzureTTSVoice))
            {
                VoiceLocalComboBox.ItemsSource = new List<string> { GetVoiceLocal(DEFAULT_VOICE) };
                VoiceNameComboBox.ItemsSource = new List<string> { GetVoiceName(DEFAULT_VOICE) };
            }
            else
            {
                VoiceLocalComboBox.ItemsSource = new List<string> { GetVoiceLocal(Common.appSettings.AzureTTSVoice) };
                VoiceNameComboBox.ItemsSource = new List<string> { GetVoiceName(Common.appSettings.AzureTTSVoice) };
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

        private static string GetVoiceLocal(string voiceString) => voiceString.Substring(0, voiceString.LastIndexOf('-'));
        private static string GetVoiceName(string voiceString) => voiceString.Substring(voiceString.LastIndexOf('-') + 1);

        private void VoiceLocalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateVoiceNameComboBox(e.AddedItems[0].ToString());
        }
    }
}