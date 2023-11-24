using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TTSHelperLibrary;


namespace MisakaTranslator_WPF.SettingsPages.TTSPages
{
    /// <summary>
    /// LocalTTSSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class LocalTTSSettingsPage : Page
    {
        LocalTTS localTTS = new LocalTTS();
        public LocalTTSSettingsPage()
        {
            InitializeComponent();

            List<string> lst = localTTS.GetAllTTSEngine();
            TTSSourceCombox.ItemsSource = lst;

            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i] == Common.appSettings.LocalTTSVoice)
                {
                    TTSSourceCombox.SelectedIndex = i;
                    break;
                }
            }
            VolumeBar.Value = Common.appSettings.LoaclTTSVolume;
            RateBar.Value = Common.appSettings.LocaTTSRate;
        }

        private void TTSSourceCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            localTTS.SetTTSVoice((string)TTSSourceCombox.SelectedValue);
        }

        private void TestBtn_Click(object sender, RoutedEventArgs e)
        {
            localTTS.SpeakAsync(TestSrcText.Text);
            Common.appSettings.LocalTTSVoice = (string)TTSSourceCombox.SelectedValue;
            Common.appSettings.LoaclTTSVolume = (int)VolumeBar.Value;
            Common.appSettings.LocaTTSRate = (int)RateBar.Value;
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