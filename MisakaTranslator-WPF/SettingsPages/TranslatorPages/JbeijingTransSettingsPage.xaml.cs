﻿using System.Windows;
using System.Windows.Controls;
using TranslatorLibrary;

namespace MisakaTranslator_WPF.SettingsPages.TranslatorPages {
    /// <summary>
    /// JbeijingTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class JbeijingTransSettingsPage : Page {
        public JbeijingTransSettingsPage() {
            InitializeComponent();
            PathBox.Text = Common.appSettings.JBJCTDllPath;
        }

        private async void TransTestBtn_Click(object sender, RoutedEventArgs e) {
            ITranslator Trans = new JBeijingTranslator();
            Trans.TranslatorInit(Common.appSettings.JBJCTDllPath, "");
            string res = await Trans.TranslateAsync(TestSrcText.Text, "", "");
            if (res != null) {
                HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
            } else {
                HandyControl.Controls.Growl.Error(
                    $"JBeijing翻译{Application.Current.Resources["APITest_Error_Hint"]}\n{Trans.GetLastError()}");
            }
        }

        private void ChoosePathBtn_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = Application.Current.Resources["JbeijingTransSettingsPage_ChoosePathHint"].ToString();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                if (string.IsNullOrEmpty(dialog.SelectedPath)) {
                    HandyControl.Controls.Growl.Error(Application.Current.Resources["FilePath_Null_Hint"].ToString());
                } else {
                    PathBox.Text = dialog.SelectedPath;
                    Common.appSettings.JBJCTDllPath = PathBox.Text;
                }
            }
        }
    }
}