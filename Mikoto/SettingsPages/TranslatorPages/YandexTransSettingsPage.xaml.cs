using Mikoto.Translators.Implementations;
using Mikoto.Translators.Interfaces;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto.SettingsPages.TranslatorPages
{
    /// <summary>
    /// YandexTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class YandexTransSettingsPage : Page
    {
        public YandexTransSettingsPage()
        {
            InitializeComponent();
            YandexTransApiKeyBox.Text = Common.AppSettings.YandexApiKey;
        }

        private async void AuthTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.YandexApiKey = YandexTransApiKeyBox.Text;
            ITranslator YandexTrans = new YandexTranslator((string)Application.Current.Resources[nameof(YandexTranslator)], YandexTransApiKeyBox.Text);

            if (await YandexTrans.TranslateAsync("apple", "zh", "en") != null)
            {
                HandyControl.Controls.Growl.Success($"Yandex {Application.Current.Resources["APITest_Success_Hint"]}");
            }
            else
            {
                HandyControl.Controls.Growl.Error($"Yandex {Application.Current.Resources["APITest_Error_Hint"]}\n{YandexTrans.GetLastError()}");
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(YandexTranslator.GetUrl_API()) { UseShellExecute = true });
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(YandexTranslator.GetUrl_Doc()) { UseShellExecute = true });
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(YandexTranslator.GetUrl_Bill()) { UseShellExecute = true });
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            ITranslator YandexTrans = new YandexTranslator((string)Application.Current.Resources[nameof(YandexTranslator)], YandexTransApiKeyBox.Text);
            string? res = await YandexTrans.TranslateAsync(TestSrcText.Text, TestDstLang.Text, TestSrcLang.Text);

            if (res != null)
            {
                HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
            }
            else
            {
                HandyControl.Controls.Growl.Error(
                    $"Yandex {Application.Current.Resources["APITest_Error_Hint"]}\n{YandexTrans.GetLastError()}");
            }
        }
    }
}
