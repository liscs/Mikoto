using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TranslatorLibrary.Translator;

namespace MisakaTranslator.SettingsPages.TranslatorPages
{
    /// <summary>
    /// AzureTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class AzureTransSettingsPage : Page
    {
        public AzureTransSettingsPage()
        {
            InitializeComponent();
            AzureTransSecretKeyBox.Text = Common.AppSettings.AzureSecretKey;
            AzureTransLocationBox.Text = Common.AppSettings.AzureLocation;
        }

        private async void AuthTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.AzureSecretKey = AzureTransSecretKeyBox.Text;
            Common.AppSettings.AzureLocation = AzureTransLocationBox.Text;

            ITranslator AzureTrans = AzureTranslator.TranslatorInit(AzureTransSecretKeyBox.Text, AzureTransLocationBox.Text);

            if (await AzureTrans.TranslateAsync("apple", "zh", "en") != null)
            {
                HandyControl.Controls.Growl.Success($"Azure翻译{Application.Current.Resources["APITest_Success_Hint"]}");
            }
            else
            {
                HandyControl.Controls.Growl.Error($"Azure翻译{Application.Current.Resources["APITest_Error_Hint"]}\n{AzureTrans.GetLastError()}");
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AzureTranslator.GetUrl_API()) { UseShellExecute = true });
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AzureTranslator.GetUrl_Doc()) { UseShellExecute = true });
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AzureTranslator.GetUrl_Bill()) { UseShellExecute = true });
        }
        private void AzureLangCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AzureTranslator.GetUrl_lang()) { UseShellExecute = true });
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            ITranslator AzureTrans = AzureTranslator.TranslatorInit(Common.AppSettings.AzureSecretKey, Common.AppSettings.AzureLocation);
            string? res = await AzureTrans.TranslateAsync(TestSrcText.Text, TestDstLang.Text, TestSrcLang.Text);

            if (res != null)
            {
                HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
            }
            else
            {
                HandyControl.Controls.Growl.Error(
                    $"Azure翻译{Application.Current.Resources["APITest_Error_Hint"]}\n{AzureTrans.GetLastError()}");
            }
        }
    }
}