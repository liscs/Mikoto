using Mikoto.SettingsPages.TranslatorPages.Models;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto.SettingsPages.TranslatorPages
{
    /// <summary>
    /// CommonTranslatorSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class CommonTranslatorSettingsPage : Page
    {
        private readonly CommonTranslatorSettingsPageViewModel _viewModel;
        public CommonTranslatorSettingsPage(ApiConfigDefinition def)
        {
            InitializeComponent();
            _viewModel= new CommonTranslatorSettingsPageViewModel(def);
            DataContext =_viewModel;
        }

        // 打开 API 文档
        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            var url = _viewModel?.Definition?.DocUrl;
            if (!string.IsNullOrEmpty(url))
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        // 打开计费页面
        private void BillingBtn_Click(object sender, RoutedEventArgs e)
        {
            var url = _viewModel?.Definition?.BillingUrl;
            if (!string.IsNullOrEmpty(url))
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        // 测试翻译
        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            var translator = _viewModel?.CreateTranslatorInstance();
            if (translator == null) return;

            string? result = await translator.TranslateAsync(
                TestSrcText.Text,
                TestDstLang.Text,
                TestSrcLang.Text
            );

            if (result != null)
            {
                HandyControl.Controls.MessageBox.Show(result, App.Env.ResourceService.Get("MessageBox_Result").ToString());
            }
            else
            {
                HandyControl.Controls.Growl.Error(translator?.GetLastError() ?? "Unknown error");
            }
        }

        // 打开申请/激活页面
        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            var url = _viewModel?.Definition?.ApplyUrl;
            if (!string.IsNullOrEmpty(url))
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        // 查看语言代码
        private void LangCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            var url = _viewModel?.Definition?.LangCodeUrl;
            if (!string.IsNullOrEmpty(url))
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }

}
