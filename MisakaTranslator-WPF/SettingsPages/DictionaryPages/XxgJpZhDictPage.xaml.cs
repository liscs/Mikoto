using System.Windows;
using System.Windows.Controls;
using DictionaryHelperLibrary;

namespace MisakaTranslator_WPF.SettingsPages.DictionaryPages {
    /// <summary>
    /// XxgJpZhDictPage.xaml 的交互逻辑
    /// </summary>
    public partial class XxgJpZhDictPage : Page {
        public XxgJpZhDictPage() {
            InitializeComponent();
            PathBox.Text = Common.appSettings.xxgPath;
        }

        private void ChoosePathBtn_Click(object sender, RoutedEventArgs e) {

            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog {
                Multiselect = false,
                Title = Application.Current.Resources["XxgJpZhDictPage_ChoosePathHint"].ToString(),
                Filter = "所有文件(*.*)|*.*"
            };
            if (dialog.ShowDialog() == true) {
                if (string.IsNullOrEmpty(dialog.FileName)) {
                    HandyControl.Controls.Growl.Error(Application.Current.Resources["FilePath_Null_Hint"].ToString());
                } else {
                    PathBox.Text = dialog.FileName;
                    Common.appSettings.xxgPath = PathBox.Text;
                }
            }
        }

        private void TestBtn_Click(object sender, RoutedEventArgs e) {
            IDict dict = new XxgJpzhDict();

            dict.DictInit(Common.appSettings.xxgPath, "");

            string ret = dict.SearchInDict(TestSrcText.Text);
            if (ret != null) {
                DictResWindow dictResWindow = new DictResWindow(TestSrcText.Text);
                dictResWindow.Show();
            } else {
                HandyControl.Controls.Growl.Error($"查询错误！{dict.GetLastError()}");
            }

        }
    }
}