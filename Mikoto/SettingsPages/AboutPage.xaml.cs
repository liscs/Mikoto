using Mikoto.ProcessInterop;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto.SettingsPages
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page
    {
        private AboutPageViewModel _viewModel = new();
        public AboutPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void IssueButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.ShellStart("https://github.com/liscs/Mikoto/issues");
        }

        private void GithubButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.ShellStart("https://github.com/liscs/Mikoto");
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
