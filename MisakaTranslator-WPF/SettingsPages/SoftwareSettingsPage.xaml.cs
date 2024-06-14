using HandyControl.Controls;
using System.Windows;
using System.Windows.Controls;

namespace MisakaTranslator.SettingsPages
{
    /// <summary>
    /// SoftwareSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SoftwareSettingsPage : Page
    {
        private SoftwareSettingsViewModel _viewModel = new();
        public SoftwareSettingsPage()
        {
            InitializeComponent();
            DataContext = _viewModel;

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
            if (sender is not RadioButton radioButton) { return; }
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.IsEnabled = false;
                LoadingCircle.Visibility = Visibility.Visible;
                var result = await Common.CheckUpdateAsync();
                switch (result.Item1)
                {
                    case CheckUpdateResult.CanUpdate:
                        Common.ShowUpdateMessageBox(result.Item2!);
                        break;
                    case CheckUpdateResult.AlreadyLatest:
                        Growl.InfoGlobal(Application.Current.Resources["SoftwareSettingsPage_AlreadyLatest"].ToString());
                        break;
                    case CheckUpdateResult.RequestError:
                        Growl.InfoGlobal(Application.Current.Resources["SoftwareSettingsPage_RequestUpdateError"].ToString());
                        break;
                    default:
                        break;
                }
                LoadingCircle.Visibility = Visibility.Hidden;
                button.IsEnabled = true;
            }

        }
    }
}