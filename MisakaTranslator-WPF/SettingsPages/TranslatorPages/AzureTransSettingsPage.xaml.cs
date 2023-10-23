using System.Windows;
using System.Windows.Controls;
using TranslatorLibrary;

namespace MisakaTranslator_WPF.SettingsPages.TranslatorPages
{
    /// <summary>
    /// AzureTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class AzureTransSettingsPage : Page
    {
        public AzureTransSettingsPage()
        {
            InitializeComponent();
            AzureTransSecretKeyBox.Text = Common.appSettings.AzureSecretKey;
            AzureTransLocationBox.Text = Common.appSettings.AzureLocation;
        }

        private async void AuthTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.appSettings.AzureSecretKey = AzureTransSecretKeyBox.Text;
            Common.appSettings.AzureLocation = AzureTransLocationBox.Text;

            ITranslator AzureTrans = new AzureTranslator();
            AzureTrans.TranslatorInit(AzureTransSecretKeyBox.Text, AzureTransLocationBox.Text);

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
            System.Diagnostics.Process.Start(AzureTranslator.GetUrl_allpyAPI());
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(AzureTranslator.GetUrl_Doc());
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(AzureTranslator.GetUrl_bill());
        }
        private void AzureLangCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(AzureTranslator.GetUrl_lang());
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            ITranslator AzureTrans = new AzureTranslator();
            AzureTrans.TranslatorInit(Common.appSettings.AzureSecretKey, Common.appSettings.AzureLocation);
            string res = await AzureTrans.TranslateAsync(TestSrcText.Text, TestDstLang.Text, TestSrcLang.Text);

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