using HandyControl.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto.SettingsPages
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page
    {
        AboutPageViewModel _viewModel = new();
        public AboutPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void IssueButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/liscs/Mikoto/issues") { UseShellExecute = true });
        }

        private void GithubButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/liscs/Mikoto") { UseShellExecute = true });
        }

        private async void CheckUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.IsEnabled = false;
                await Common.CheckUpdateAsync(true);
                button.IsEnabled = true;
            }

        }
    }
}
