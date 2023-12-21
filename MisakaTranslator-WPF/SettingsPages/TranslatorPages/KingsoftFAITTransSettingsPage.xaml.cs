using System.Windows;
using System.Windows.Controls;
using TranslatorLibrary.Translator;

namespace MisakaTranslator_WPF.SettingsPages.TranslatorPages
{
    /// <summary>
    /// KingsoftFAITTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class KingsoftFAITTransSettingsPage : Page
    {
        public KingsoftFAITTransSettingsPage()
        {
            InitializeComponent();
            PathBox.Text = Common.AppSettings.KingsoftFastAITPath;
        }

        private void ChoosePathBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = Application.Current.Resources["KingsoftFAITTransSettingsPage_ChoosePathHint"]
                .ToString()
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
                    Common.AppSettings.KingsoftFastAITPath = PathBox.Text;
                }
            }
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {
            KingsoftFastAITTranslator Trans = new KingsoftFastAITTranslator();
            Trans.TranslatorInit(Common.AppSettings.KingsoftFastAITPath, "");
            string res = await Trans.TranslateAsync(TestSrcText.Text, "zh", TestSrcLang.Text);
            if (res != null)
            {
                HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
            }
            else
            {
                HandyControl.Controls.Growl.Error(
                    $"金山快译翻译{Application.Current.Resources["APITest_Error_Hint"]}\n{Trans.GetLastError()}");
            }
        }
    }
}