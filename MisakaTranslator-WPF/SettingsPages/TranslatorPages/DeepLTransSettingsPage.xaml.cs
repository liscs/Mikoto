using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TranslatorLibrary.Translator;

namespace MisakaTranslator_WPF.SettingsPages.TranslatorPages
{
    /// <summary>
    /// DeepLTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class DeepLTransSettingsPage : Page
    {
        public DeepLTransSettingsPage()
        {
            InitializeComponent();
            DeepLTransSecretKeyBox.Text = Common.AppSettings.DeepLsecretKey;
        }

        private async void AuthTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.DeepLsecretKey = DeepLTransSecretKeyBox.Text;
            ITranslator deepLTrans = new DeepLTranslator();
            deepLTrans.TranslatorInit(DeepLTransSecretKeyBox.Text, DeepLTransSecretKeyBox.Text);

            if (await deepLTrans.TranslateAsync("apple", "zh", "en") != null)
            {
                HandyControl.Controls.Growl.Success($"DeepL {Application.Current.Resources["APITest_Success_Hint"]}");
            }
            else
            {
                HandyControl.Controls.Growl.Error($"DeepL {Application.Current.Resources["APITest_Error_Hint"]}\n{deepLTrans.GetLastError()}");
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(DeepLTranslator.SIGN_UP_URL) { UseShellExecute = true });
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(DeepLTranslator.DOCUMENT_URL) { UseShellExecute = true });
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(DeepLTranslator.BILL_URL) { UseShellExecute = true });
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            ITranslator deepLTrans = new DeepLTranslator();
            deepLTrans.TranslatorInit(DeepLTransSecretKeyBox.Text, DeepLTransSecretKeyBox.Text);
            string? res = await deepLTrans.TranslateAsync(TestSrcText.Text, TestDstLang.Text, TestSrcLang.Text);

            if (res != null)
            {
                HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
            }
            else
            {
                HandyControl.Controls.Growl.Error(
                    $"DeepL {Application.Current.Resources["APITest_Error_Hint"]}\n{deepLTrans.GetLastError()}");
            }
        }
    }
}
