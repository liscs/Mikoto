using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MisakaTranslator_WPF.SettingsPages.OCRPages
{
    public partial class TesseractCliSettingsPage : Page
    {
        static Dictionary<string, string> modeLst = new Dictionary<string, string>()
        {
            { Application.Current.Resources["TesseractCliSettingsPage_JapaneseHorizontal"].ToString(), "jpn" },
            { Application.Current.Resources["TesseractCliSettingsPage_JapaneseVertical"].ToString(), "jpn_vert" },
            { Application.Current.Resources["TesseractCliSettingsPage_English"].ToString(), "eng" },
            { Application.Current.Resources["TesseractCliSettingsPage_Custom"].ToString(), "custom" }
        };
        static List<string> itemList = modeLst.Keys.ToList();
        static List<string> valueList = new List<string>();
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
            PathBox.Text = Common.appSettings.TesseractCli_Path;
            ArgsBox.Text = Common.appSettings.TesseractCli_Args;
            SelectBox.ItemsSource = itemList;
            SelectBox.SelectedIndex = valueList.IndexOf(Common.appSettings.TesseractCli_Mode);
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
                    Common.appSettings.TesseractCli_Path = dialog.FileName;
                }
            }
        }

        private void SelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectBox.SelectedValue != null)
            {
                Common.appSettings.TesseractCli_Mode = modeLst[(string)SelectBox.SelectedValue];
                SyncModeAndArgs();
            }
        }

        private void SyncModeAndArgs()
        {
            switch (Common.appSettings.TesseractCli_Mode)
            {
                case "jpn":
                    Common.appSettings.TesseractCli_Args = "-l jpn --psm 6";
                    ArgsBox.Text = Common.appSettings.TesseractCli_Args;
                    ArgsBox.IsEnabled = false;
                    break;
                case "jpn_vert":
                    Common.appSettings.TesseractCli_Args = "-l jpn_vert --psm 5";
                    ArgsBox.Text = Common.appSettings.TesseractCli_Args;
                    ArgsBox.IsEnabled = false;
                    break;
                case "eng":
                    Common.appSettings.TesseractCli_Args = ArgsBox.Text = "--psm 6";
                    ArgsBox.IsEnabled = false;
                    break;
                default:
                    Common.appSettings.TesseractCli_Args = ArgsBox.Text;
                    ArgsBox.IsEnabled = true;
                    break;
            }
        }

        private void ArgsBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Common.appSettings.TesseractCli_Args = ArgsBox.Text;
        }

        private void PathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Common.appSettings.TesseractCli_Path = PathBox.Text;
        }
    }
}
