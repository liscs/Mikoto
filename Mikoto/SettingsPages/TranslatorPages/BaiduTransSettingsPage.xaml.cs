using Mikoto.ProcessInterop;
using Mikoto.Translators.Implementations;
using Mikoto.Translators.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto.SettingsPages.TranslatorPages
{
    /// <summary>
    /// BaiduTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class BaiduTransSettingsPage : Page
    {
        public BaiduTransSettingsPage()
        {
            InitializeComponent();
            BDTransAppIDBox.Text = Common.AppSettings.BDappID;
            BDTransSecretKeyBox.Text = Common.AppSettings.BDsecretKey;
        }

        private async void AuthTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.BDappID = BDTransAppIDBox.Text;
            Common.AppSettings.BDsecretKey = BDTransSecretKeyBox.Text;

            if (BDTransAppIDBox.Text.Length == 24)
            {
                HandyControl.Controls.Growl.Error($"百度翻译{Application.Current.Resources["APITest_Error_Hint"]}\nDo not use ai.baidu.com endpoint.");
                return;
            }

            ITranslator BDTrans = new BaiduTranslator((string)Application.Current.Resources[nameof(BaiduTranslator)], BDTransAppIDBox.Text, BDTransSecretKeyBox.Text);

            if (await BDTrans.TranslateAsync("apple", "zh", "en") != null)
            {
                HandyControl.Controls.Growl.Success($"百度翻译{Application.Current.Resources["APITest_Success_Hint"]}");
            }
            else
            {
                HandyControl.Controls.Growl.Error($"百度翻译{Application.Current.Resources["APITest_Error_Hint"]}\n{BDTrans.GetLastError()}");
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.ShellStart(BaiduTranslator.GetUrl_API());
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.ShellStart(BaiduTranslator.GetUrl_Doc());
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.ShellStart(BaiduTranslator.GetUrl_Bill());
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            ITranslator BDTrans = new BaiduTranslator((string)Application.Current.Resources[nameof(BaiduTranslator)], Common.AppSettings.BDappID, Common.AppSettings.BDsecretKey);
            string? res = await BDTrans.TranslateAsync(TestSrcText.Text, TestDstLang.Text, TestSrcLang.Text);

            if (res != null)
            {
                HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
            }
            else
            {
                HandyControl.Controls.Growl.Error(
                    $"百度翻译{Application.Current.Resources["APITest_Error_Hint"]}\n{BDTrans.GetLastError()}");
            }
        }
    }
}