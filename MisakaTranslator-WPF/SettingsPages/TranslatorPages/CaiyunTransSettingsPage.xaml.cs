using MisakaTranslator.Translators;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MisakaTranslator.SettingsPages.TranslatorPages
{
    /// <summary>
    /// CaiyunTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class CaiyunTransSettingsPage : Page
    {
        public CaiyunTransSettingsPage()
        {
            InitializeComponent();
            TransTokenBox.Text = Common.AppSettings.CaiyunToken;
        }

        private async void AuthTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.CaiyunToken = TransTokenBox.Text;
            ITranslator Trans = CaiyunTranslator.TranslatorInit(TransTokenBox.Text, "");
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
            ITranslator Trans = CaiyunTranslator.TranslatorInit(Common.AppSettings.CaiyunToken);
            string? res = await Trans.TranslateAsync(TestSrcText.Text, TestDstLang.Text, TestSrcLang.Text);
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