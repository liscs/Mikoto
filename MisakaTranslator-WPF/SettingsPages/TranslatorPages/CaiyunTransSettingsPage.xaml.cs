using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TranslatorLibrary.Translator;

namespace MisakaTranslator_WPF.SettingsPages.TranslatorPages
{
    /// <summary>
    /// CaiyunTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class CaiyunTransSettingsPage : Page
    {
        public CaiyunTransSettingsPage()
        {
            InitializeComponent();
            TransTokenBox.Text = Common.appSettings.CaiyunToken;
        }

        private async void AuthTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.appSettings.CaiyunToken = TransTokenBox.Text;
            CaiyunTranslator Trans = new CaiyunTranslator();
            Trans.TranslatorInit(TransTokenBox.Text, "");
            if (await Trans.TranslateAsync("apple", "zh", "en") != null)
            {
                HandyControl.Controls.Growl.Success($"彩云小译{Application.Current.Resources["APITest_Success_Hint"]}");
            }
            else
            {
                HandyControl.Controls.Growl.Error($"彩云小译{Application.Current.Resources["APITest_Error_Hint"]}\n{Trans.GetLastError()}");
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(CaiyunTranslator.GetUrl_API()) { UseShellExecute = true });
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(CaiyunTranslator.GetUrl_Doc()) { UseShellExecute = true });
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(CaiyunTranslator.GetUrl_Bill()) { UseShellExecute = true });
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            CaiyunTranslator Trans = new CaiyunTranslator();
            Trans.TranslatorInit(Common.appSettings.CaiyunToken, "");
            string res = await Trans.TranslateAsync(TestSrcText.Text, TestDstLang.Text, TestSrcLang.Text);
            if (res != null)
            {
                HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
            }
            else
            {
                HandyControl.Controls.Growl.Error($"彩云小译{Application.Current.Resources["APITest_Error_Hint"]}\n{Trans.GetLastError()}");
            }
        }
    }
}