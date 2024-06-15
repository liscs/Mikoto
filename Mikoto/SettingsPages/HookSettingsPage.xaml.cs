using System.IO;
using System.Windows;
using System.Windows.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mikoto.SettingsPages
{
    /// <summary>
    /// HookSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class HookSettingsPage : Page
    {
        public HookSettingsPage()
        {
            InitializeComponent();

            AutoHookCheckBox.IsChecked = Common.AppSettings.AutoHook;
            AutoDetachCheckBox.IsChecked = Common.AppSettings.AutoDetach;

            Path32Box.Text = Common.AppSettings.Textractor_Path32;
            Path64Box.Text = Common.AppSettings.Textractor_Path64;
        }

        private void AutoHookCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.AutoHook = AutoHookCheckBox.IsChecked ?? false;
        }

        private void AutoDetachCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.AutoDetach = AutoDetachCheckBox.IsChecked ?? true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Common.ExportTextractorHistory())
            {
                HandyControl.Controls.Growl.Success(Application.Current.Resources["HookSettingsPage_SuccessHint"].ToString());
            }
            else
            {
                HandyControl.Controls.Growl.Error(Application.Current.Resources["HookSettingsPage_ErrorHint"].ToString());
            }
        }

        private void ChoosePath32Btn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CheckCLI(dialog.SelectedPath, true);
            }
        }

        private void ChoosePath64Btn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CheckCLI(dialog.SelectedPath, false);
            }
        }

        private void CheckCLI(string path, bool isX86)
        {
            if (File.Exists($@"{path}\TextractorCLI.exe"))
            {
                if (path.Contains("x86"))
                {
                    Path32Box.Text = $@"{path}\TextractorCLI.exe";
                    Common.AppSettings.Textractor_Path32 = Path32Box.Text;
                    if (File.Exists($@"{path.Replace("x86", "x64")}\TextractorCLI.exe"))
                    {
                        Path64Box.Text = $@"{path.Replace("x86", "x64")}\TextractorCLI.exe";
                        Common.AppSettings.Textractor_Path64 = Path64Box.Text;
                    }
                }
                else if (path.Contains("x64"))
                {
                    Path64Box.Text = $@"{path}\TextractorCLI.exe";
                    Common.AppSettings.Textractor_Path64 = Path64Box.Text;
                    if (File.Exists($@"{path.Replace("x64", "x86")}\TextractorCLI.exe"))
                    {
                        Path32Box.Text = $@"{path.Replace("x64", "x86")}\TextractorCLI.exe";
                        Common.AppSettings.Textractor_Path32 = Path32Box.Text;
                    }
                }
                else
                {
                    if (isX86)
                    {
                        Path32Box.Text = $@"{path}\TextractorCLI.exe";
                        Common.AppSettings.Textractor_Path32 = Path32Box.Text;
                    }
                    else
                    {
                        Path64Box.Text = $@"{path}\TextractorCLI.exe";
                        Common.AppSettings.Textractor_Path64 = Path64Box.Text;
                    }
                }
            }
            else if (File.Exists($@"{path}\x86\TextractorCLI.exe"))
            {
                Path32Box.Text = $@"{path}\x86\TextractorCLI.exe";
                Common.AppSettings.Textractor_Path32 = Path32Box.Text;
                if (File.Exists($@"{path}\x64\TextractorCLI.exe"))
                {
                    Path64Box.Text = $@"{path}\x64\TextractorCLI.exe";
                    Common.AppSettings.Textractor_Path64 = Path64Box.Text;
                }
            }
            else if (File.Exists($@"{path}\x64\TextractorCLI.exe"))
            {
                Path64Box.Text = $@"{path}\x64\TextractorCLI.exe";
                Common.AppSettings.Textractor_Path64 = Path64Box.Text;
                if (File.Exists($@"{path}\x86\TextractorCLI.exe"))
                {
                    Path32Box.Text = $@"{path}\x86\TextractorCLI.exe";
                    Common.AppSettings.Textractor_Path32 = Path32Box.Text;
                }
            }
            else
            {
                MessageBox.Show(messageBoxText: Application.Current.Resources["HookSettingsPage_TextractorCLINotExistError"].ToString(), caption: Application.Current.Resources["MessageBox_Error"].ToString(), icon: MessageBoxImage.Error);
            }
        }
    }
}