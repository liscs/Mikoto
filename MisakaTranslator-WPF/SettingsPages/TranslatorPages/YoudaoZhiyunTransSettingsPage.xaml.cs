using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TranslatorLibrary.Translator;

namespace MisakaTranslator_WPF.SettingsPages.TranslatorPages
{
    /// <summary>
    /// YoudaoZhiyunTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class YoudaoZhiyunTransSettingsPage : Page
    {
        public YoudaoZhiyunTransSettingsPage()
        {
            InitializeComponent();
            YDZYTransAppIDBox.Text = Common.appSettings.YDZYAppId;
            YDZYTransAppSecretBox.Text = Common.appSettings.YDZYAppSecret;
        }

        private async void AuthTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.appSettings.YDZYAppId = YDZYTransAppIDBox.Text;
            Common.appSettings.YDZYAppSecret = YDZYTransAppSecretBox.Text;
            ITranslator Trans = new YoudaoZhiyun();
            Trans.TranslatorInit(YDZYTransAppIDBox.Text, YDZYTransAppSecretBox.Text);
            if (await Trans.TranslateAsync("apple", "zh", "en") != null)
            {
                HandyControl.Controls.Growl.Success($"有道智云{Application.Current.Resources["APITest_Success_Hint"]}");
            }
            else
            {
                HandyControl.Controls.Growl.Error($"有道智云{Application.Current.Resources["APITest_Error_Hint"]}\n{Trans.GetLastError()}");
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(YoudaoZhiyun.GetUrl_allpyAPI()) { UseShellExecute = true });
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(YoudaoZhiyun.GetUrl_Doc()) { UseShellExecute = true });
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(YoudaoZhiyun.GetUrl_bill()) { UseShellExecute = true });
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            ITranslator Trans = new YoudaoZhiyun();
            Trans.TranslatorInit(Common.appSettings.YDZYAppId, Common.appSettings.YDZYAppSecret);
            string res = await Trans.TranslateAsync(TestSrcText.Text, TestDstLang.Text, TestSrcLang.Text);
            if (res != null)
            {
                HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
            }
            else
            {
                HandyControl.Controls.Growl.Error($"有道智云{Application.Current.Resources["APITest_Error_Hint"]}\n{Trans.GetLastError()}");
            }
        }
    }
}
