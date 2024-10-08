﻿using Mikoto.TTS;
using System.Windows;
using System.Windows.Controls;


namespace Mikoto.SettingsPages.TTSPages
{
    /// <summary>
    /// LocalTTSSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class LocalTTSSettingsPage : Page
    {
        private LocalTTS localTTS = new LocalTTS();
        public LocalTTSSettingsPage()
        {
            InitializeComponent();

            List<string> lst = localTTS.GetAllTTSEngine();
            TTSSourceComboBox.ItemsSource = lst;

            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i] == Common.AppSettings.LocalTTSVoice)
                {
                    TTSSourceComboBox.SelectedIndex = i;
                    break;
                }
            }
            VolumeBar.Value = Common.AppSettings.LoaclTTSVolume;
            RateBar.Value = Common.AppSettings.LocaTTSRate;
        }

        private void TTSSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            localTTS.SetTTSVoice((string)TTSSourceComboBox.SelectedValue);
        }

        private void TestBtn_Click(object sender, RoutedEventArgs e)
        {
            localTTS.SpeakAsync(TestSrcText.Text);
            Common.AppSettings.LocalTTSVoice = (string)TTSSourceComboBox.SelectedValue;
            Common.AppSettings.LoaclTTSVolume = (int)VolumeBar.Value;
            Common.AppSettings.LocaTTSRate = (int)RateBar.Value;
        }

        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            localTTS.SetVolume((int)VolumeBar.Value);
        }

        private void RateBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            localTTS.SetRate((int)RateBar.Value);
        }
    }
}