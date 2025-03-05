using Mikoto.Translators.Implementations;
using Mikoto.Translators.Interfaces;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto.SettingsPages.TranslatorPages
{
    /// <summary>
    /// ChatGPTTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChatGPTTransSettingsPage : Page
    {
        public ChatGPTTransSettingsPage()
        {
            InitializeComponent();
            ChatGPTTransSecretKeyBox.Text = Common.AppSettings.ChatGPTapiKey;
            ChatGPTTransUrlBox.Text = Common.AppSettings.ChatGPTapiUrl;
            ChatGPTModelBox.Text = Common.AppSettings.ChatGPTapiModel;
        }

        private async void AuthTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.ChatGPTapiKey = ChatGPTTransSecretKeyBox.Text;
            Common.AppSettings.ChatGPTapiUrl = ChatGPTTransUrlBox.Text;
            Common.AppSettings.ChatGPTapiModel = ChatGPTModelBox.Text;

            ITranslator chatGPTTrans = ChatGPTTranslator.TranslatorInit(ChatGPTTransSecretKeyBox.Text, ChatGPTTransUrlBox.Text, ChatGPTModelBox.Text);

            if (await chatGPTTrans.TranslateAsync("apple", "zh", "en") != null)
            {
                HandyControl.Controls.Growl.Success($"ChatGPT {Application.Current.Resources["APITest_Success_Hint"]}");
            }
            else
            {
                HandyControl.Controls.Growl.Error($"ChatGPT {Application.Current.Resources["APITest_Error_Hint"]}{Environment.NewLine}{chatGPTTrans.GetLastError()}");
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(ChatGPTTranslator.SIGN_UP_URL) { UseShellExecute = true });
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(ChatGPTTranslator.DOCUMENT_URL) { UseShellExecute = true });
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(ChatGPTTranslator.BILL_URL) { UseShellExecute = true });
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            ITranslator chatGPTTrans = ChatGPTTranslator.TranslatorInit(ChatGPTTransSecretKeyBox.Text, ChatGPTTransUrlBox.Text, ChatGPTModelBox.Text);
            string? res = await chatGPTTrans.TranslateAsync(TestSrcText.Text, TestDstLang.Text, TestSrcLang.Text);

            if (res != null)
            {
                HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
            }
            else
            {
                HandyControl.Controls.Growl.Error(
                    $"ChatGPT {Application.Current.Resources["APITest_Error_Hint"]}\n{chatGPTTrans.GetLastError()}");
            }
        }
    }
}