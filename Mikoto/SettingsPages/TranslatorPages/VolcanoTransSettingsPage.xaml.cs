using Mikoto.Translators;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto.SettingsPages.TranslatorPages
{
    /// <summary>
    /// VolcanoTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class VolcanoTransSettingsPage : Page
    {
        public VolcanoTransSettingsPage()
        {
            InitializeComponent();
            VolcanoTransIdBox.Text = Common.AppSettings.VolcanoId;
            VolcanoTransKeyBox.Text = Common.AppSettings.VolcanoKey;
        }

        private async void AuthTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.VolcanoId = VolcanoTransIdBox.Text;
            Common.AppSettings.VolcanoKey = VolcanoTransKeyBox.Text;

            ITranslator VolcanoTrans = VolcanoTranslator.TranslatorInit(VolcanoTransIdBox.Text, VolcanoTransKeyBox.Text);

            if (await VolcanoTrans.TranslateAsync("apple", "zh", "en") != null)
            {
                HandyControl.Controls.Growl.Success($"火山翻译{Application.Current.Resources["APITest_Success_Hint"]}");
            }
            else
            {
                HandyControl.Controls.Growl.Error($"火山翻译{Application.Current.Resources["APITest_Error_Hint"]}\n{VolcanoTrans.GetLastError()}");
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(VolcanoTranslator.GetUrl_API()) { UseShellExecute = true });
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(VolcanoTranslator.GetUrl_Doc()) { UseShellExecute = true });
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(VolcanoTranslator.GetUrl_Bill()) { UseShellExecute = true });
        }
        private void VolcanoLangCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(VolcanoTranslator.GetUrl_lang()) { UseShellExecute = true });
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            ITranslator VolcanoTrans = VolcanoTranslator.TranslatorInit(Common.AppSettings.VolcanoId, Common.AppSettings.VolcanoKey);
            string? res = await VolcanoTrans.TranslateAsync(TestSrcText.Text, TestDstLang.Text, TestSrcLang.Text);

            if (res != null)
            {
                HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
            }
            else
            {
                HandyControl.Controls.Growl.Error(
                    $"火山翻译{Application.Current.Resources["APITest_Error_Hint"]}\n{VolcanoTrans.GetLastError()}");
            }
        }
    }
}