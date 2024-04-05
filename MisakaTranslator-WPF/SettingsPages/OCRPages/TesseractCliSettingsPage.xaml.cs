using System.Windows;
using System.Windows.Controls;

namespace MisakaTranslator.SettingsPages.OCRPages
{
    public partial class TesseractCliSettingsPage : Page
    {
        private static Dictionary<string, string> modeLst = new Dictionary<string, string>()
        {
            { Application.Current.Resources["TesseractCliSettingsPage_JapaneseHorizontal"].ToString()!, "jpn" },
            { Application.Current.Resources["TesseractCliSettingsPage_JapaneseVertical"].ToString()!, "jpn_vert" },
            { Application.Current.Resources["TesseractCliSettingsPage_English"].ToString()!, "eng" },
            { Application.Current.Resources["TesseractCliSettingsPage_Custom"].ToString()!, "custom" }
        };
        private static List<string> itemList = modeLst.Keys.ToList();
        private static List<string> valueList = new List<string>();
        static TesseractCliSettingsPage()
        {
            foreach (var k in itemList)
            {
                valueList.Add(modeLst[k]);
            }
        }
        public TesseractCliSettingsPage()
        {
            InitializeComponent();
            PathBox.Text = Common.AppSettings.TesseractCli_Path;
            ArgsBox.Text = Common.AppSettings.TesseractCli_Args;
            SelectBox.ItemsSource = itemList;
            SelectBox.SelectedIndex = valueList.IndexOf(Common.AppSettings.TesseractCli_Mode);
            SyncModeAndArgs();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = Application.Current.Resources["TesseractCliSettingsPage_Exe"].ToString();
            if (dialog.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(dialog.FileName))
                {
                    HandyControl.Controls.Growl.Error(Application.Current.Resources["FilePath_Null_Hint"].ToString());
                }
                else
                {
                    PathBox.Text = dialog.FileName;
                    Common.AppSettings.TesseractCli_Path = dialog.FileName;
                }
            }
        }

        private void SelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectBox.SelectedValue != null)
            {
                Common.AppSettings.TesseractCli_Mode = modeLst[(string)SelectBox.SelectedValue];
                SyncModeAndArgs();
            }
        }

        private void SyncModeAndArgs()
        {
            switch (Common.AppSettings.TesseractCli_Mode)
            {
                case "jpn":
                    Common.AppSettings.TesseractCli_Args = "-l jpn --psm 6";
                    ArgsBox.Text = Common.AppSettings.TesseractCli_Args;
                    ArgsBox.IsEnabled = false;
                    break;
                case "jpn_vert":
                    Common.AppSettings.TesseractCli_Args = "-l jpn_vert --psm 5";
                    ArgsBox.Text = Common.AppSettings.TesseractCli_Args;
                    ArgsBox.IsEnabled = false;
                    break;
                case "eng":
                    Common.AppSettings.TesseractCli_Args = ArgsBox.Text = "--psm 6";
                    ArgsBox.IsEnabled = false;
                    break;
                default:
                    Common.AppSettings.TesseractCli_Args = ArgsBox.Text;
                    ArgsBox.IsEnabled = true;
                    break;
            }
        }

        private void ArgsBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Common.AppSettings.TesseractCli_Args = ArgsBox.Text;
        }

        private void PathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Common.AppSettings.TesseractCli_Path = PathBox.Text;
        }
    }
}
