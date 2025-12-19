using Mikoto.ProcessInterop;
using Mikoto.Translators.Implementations;
using Mikoto.Translators.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto.SettingsPages.TranslatorPages
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
            ITranslator Trans = new CaiyunTranslator(TransTokenBox.Text, "");
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
            ProcessHelper.ShellStart(CaiyunTranslator.GetUrl_API());
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.ShellStart(CaiyunTranslator.GetUrl_Doc());
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.ShellStart(CaiyunTranslator.GetUrl_Bill());
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            ITranslator Trans = new CaiyunTranslator((string)Application.Current.Resources[nameof(CaiyunTranslator)], Common.AppSettings.CaiyunToken);
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