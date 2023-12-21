using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TranslatorLibrary.Translator;

namespace MisakaTranslator_WPF.SettingsPages.TranslatorPages
{
    /// <summary>
    /// IBMTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class IBMTransSettingsPage : Page
    {
        public IBMTransSettingsPage()
        {
            InitializeComponent();
            IBMTransApiKeyBox.Text = Common.AppSettings.IBMApiKey;
            IBMTransURLBox.Text = Common.AppSettings.IBMURL;
        }

        private async void AuthTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.IBMApiKey = IBMTransApiKeyBox.Text;
            Common.AppSettings.IBMURL = IBMTransURLBox.Text;
            ITranslator IBMTrans = new IBMTranslator();
            IBMTrans.TranslatorInit(IBMTransApiKeyBox.Text, IBMTransURLBox.Text);

            if (await IBMTrans.TranslateAsync("apple", "zh", "en") != null)
            {
                HandyControl.Controls.Growl.Success($"IBM {Application.Current.Resources["APITest_Success_Hint"]}");
            }
            else
            {
                HandyControl.Controls.Growl.Error($"IBM {Application.Current.Resources["APITest_Error_Hint"]}\n{IBMTrans.GetLastError()}");
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(IBMTranslator.GetUrl_API()) { UseShellExecute = true });
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(IBMTranslator.GetUrl_Doc()) { UseShellExecute = true });
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(IBMTranslator.GetUrl_Bill()) { UseShellExecute = true });
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            ITranslator IBMTrans = new IBMTranslator();
            IBMTrans.TranslatorInit(IBMTransApiKeyBox.Text, IBMTransURLBox.Text);
            string res = await IBMTrans.TranslateAsync(TestSrcText.Text, TestDstLang.Text, TestSrcLang.Text);

            if (res != null)
            {
                HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
            }
            else
            {
                HandyControl.Controls.Growl.Error(
                    $"IBM {Application.Current.Resources["APITest_Error_Hint"]}\n{IBMTrans.GetLastError()}");
            }
        }
    }
}
