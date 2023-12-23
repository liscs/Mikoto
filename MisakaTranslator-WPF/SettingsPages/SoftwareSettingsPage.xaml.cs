﻿using System.Windows;
using System.Windows.Controls;

namespace MisakaTranslator_WPF.SettingsPages
{
    /// <summary>
    /// SoftwareSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SoftwareSettingsPage : Page
    {
        public SoftwareSettingsPage()
        {
            InitializeComponent();

            var appSettingsOnClickCloseButton = Common.AppSettings.OnClickCloseButton;
            switch (appSettingsOnClickCloseButton)
            {
                case "Minimization":
                    MinimizationRadioButton.IsChecked = true;
                    break;
                case "Exit":
                    ExitRadioButton.IsChecked = true;
                    break;
            }

            GrowlEnabledCheckBox.IsChecked = Common.AppSettings.GrowlEnabled;
        }

        private void RadioButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton == null) { return; }
            switch (radioButton.Name)
            {
                case "MinimizationRadioButton":
                    Common.AppSettings.OnClickCloseButton = "Minimization";
                    break;
                case "ExitRadioButton":
                    Common.AppSettings.OnClickCloseButton = "Exit";
                    break;
            }
        }

        private void GrowlEnabledCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.GrowlEnabled = GrowlEnabledCheckBox.IsChecked ?? true;
        }
    }
}