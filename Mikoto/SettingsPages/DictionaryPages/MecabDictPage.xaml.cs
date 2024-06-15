using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Mikoto.SettingsPages.DictionaryPages
{
    /// <summary>
    /// Interaction logic for MecabDictPage.xaml
    /// </summary>
    public partial class MecabDictPage : Page
    {
        public MecabDictPage()
        {
            InitializeComponent();
        }

        private void ChoosePathBtn_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                PathBox.Text = dialog.SelectedPath;
                Common.AppSettings.Mecab_DicPath = PathBox.Text;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            PathBox.Text = Common.AppSettings.Mecab_DicPath;
        }

        private void PathBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.Mecab_DicPath = PathBox.Text;
        }
    }
}
