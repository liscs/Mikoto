using System.Windows;
using TransOptimizationLibrary;

namespace MisakaTranslator
{
    /// <summary>
    /// AddOptWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddOptWindow : Window
    {
        public AddOptWindow(string src = "")
        {
            InitializeComponent();
            this.Topmost = true;

            List<string> wordtype =
            [
                Application.Current.Resources["AddOptWindow_PersonName"].ToString()!,
                Application.Current.Resources["AddOptWindow_PlaceName"].ToString()!,
            ];

            srcText.Text = src;
            wordTypeCombox.ItemsSource = wordtype;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (srcWord.Text != "" && dstWord.Text != "" && wordTypeCombox.SelectedIndex != -1)
            {
                NounTransOptimization opt = new NounTransOptimization("" + Common.GameID, Common.UsingSrcLang, Common.UsingDstLang);
                bool res = opt.AddNounTrans(srcWord.Text, wordTypeCombox.SelectedIndex + 1, dstWord.Text);
                if (res)
                {
                    HandyControl.Controls.Growl.InfoGlobal(Application.Current.Resources["AddOptWin_Success_Hint"].ToString());
                }
                else
                {
                    HandyControl.Controls.Growl.ErrorGlobal(Application.Current.Resources["AddOptWin_Error_Hint"].ToString());
                }
            }
        }
    }
}