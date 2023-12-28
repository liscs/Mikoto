using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MisakaTranslator.SettingsPages
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/liscs/MisakaTranslator/issues") { UseShellExecute = true });
        }

        private void BtnGithub_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/liscs/MisakaTranslator") { UseShellExecute = true });
        }
    }
}
