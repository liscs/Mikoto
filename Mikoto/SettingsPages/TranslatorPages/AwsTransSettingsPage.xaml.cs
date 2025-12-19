using Mikoto.ProcessInterop;
using Mikoto.Translators.Implementations;
using Mikoto.Translators.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto.SettingsPages.TranslatorPages
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

            ITranslator AwsTrans = new AwsTranslator((string)Application.Current.Resources[nameof(AwsTranslator)], AwsTransIdBox.Text, AwsTransKeyBox.Text);

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
            ProcessHelper.ShellStart(AwsTranslator.GetUrl_API());
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.ShellStart(AwsTranslator.GetUrl_ErrorCode());
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.ShellStart(AwsTranslator.GetUrl_Bill());
        }
        private void AwsLangCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.ShellStart(AwsTranslator.GetUrl_Lang());
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            ITranslator AwsTrans = new AwsTranslator((string)Application.Current.Resources[nameof(AwsTranslator)], Common.AppSettings.AwsAccessKey, Common.AppSettings.AwsSecretKey);
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