using MisakaTranslator.Translators;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MisakaTranslator.SettingsPages.TranslatorPages
{
    /// <summary>
    /// AwsTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class AwsTransSettingsPage : Page
    {
        public AwsTransSettingsPage()
        {
            InitializeComponent();
            AwsTransIdBox.Text = Common.AppSettings.AwsAccessKey;
            AwsTransKeyBox.Text = Common.AppSettings.AwsSecretKey;
        }

        private async void AuthTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.AwsAccessKey = AwsTransIdBox.Text;
            Common.AppSettings.AwsSecretKey = AwsTransKeyBox.Text;

            ITranslator AwsTrans = AwsTranslator.TranslatorInit(AwsTransIdBox.Text, AwsTransKeyBox.Text);

            if (await AwsTrans.TranslateAsync("apple", "zh", "en") != null)
            {
                HandyControl.Controls.Growl.Success($"Amazon Translate{Application.Current.Resources["APITest_Success_Hint"]}");
            }
            else
            {
                HandyControl.Controls.Growl.Error($"Amazon Translate{Application.Current.Resources["APITest_Error_Hint"]}\n{AwsTrans.GetLastError()}");
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AwsTranslator.GetUrl_API()) { UseShellExecute = true });
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AwsTranslator.GetUrl_ErrorCode()) { UseShellExecute = true });
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AwsTranslator.GetUrl_Bill()) { UseShellExecute = true });
        }
        private void AwsLangCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(AwsTranslator.GetUrl_Lang()) { UseShellExecute = true });
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            ITranslator AwsTrans = AwsTranslator.TranslatorInit(Common.AppSettings.AwsAccessKey, Common.AppSettings.AwsSecretKey);
            string? res = await AwsTrans.TranslateAsync(TestSrcText.Text, TestDstLang.Text, TestSrcLang.Text);

            if (res != null)
            {
                HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
            }
            else
            {
                HandyControl.Controls.Growl.Error(
                    $"Amazon Translate{Application.Current.Resources["APITest_Error_Hint"]}\n{AwsTrans.GetLastError()}");
            }
        }
    }
}