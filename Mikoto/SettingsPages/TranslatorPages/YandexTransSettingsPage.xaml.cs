using Mikoto.ProcessInterop;
using Mikoto.Translators.Implementations;
using Mikoto.Translators.Interfaces;
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
            ProcessHelper.ShellStart(YandexTranslator.GetUrl_API());
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.ShellStart(YandexTranslator.GetUrl_Doc());
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.ShellStart(YandexTranslator.GetUrl_Bill());
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
