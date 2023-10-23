using System.Windows;
using System.Windows.Controls;

namespace MisakaTranslator_WPF.SettingsPages
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
            System.Diagnostics.Process.Start("https://github.com/hanmin0822/MisakaTranslator/issues");
        }

        private void BtnGithub_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/hanmin0822/MisakaTranslator");
        }
    }
}
