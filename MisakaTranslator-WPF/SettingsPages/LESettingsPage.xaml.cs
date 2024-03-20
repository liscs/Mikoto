using System.Windows;
using System.Windows.Controls;

namespace MisakaTranslator.SettingsPages
{
    /// <summary>
    /// LESettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class LESettingsPage : Page
    {
        public LESettingsPage()
        {
            InitializeComponent();
            PathBox.Text = Common.AppSettings.LEPath;
        }

        private void ChoosePathBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new()
            {
                Description = Application.Current.Resources["LESettingsPage_ChooseFilePathHint"].ToString()!,
                UseDescriptionForTitle = true,
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    HandyControl.Controls.Growl.Error(Application.Current.Resources["FilePath_Null_Hint"].ToString());
                }
                else
                {
                    PathBox.Text = dialog.SelectedPath;
                    UpdateLEPath();
                }
            }
        }

        private void PathBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateLEPath();
        }

        private void UpdateLEPath()
        {
            Common.AppSettings.LEPath = PathBox.Text;
            MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.RefreshLEStartButton();
        }
    }
}