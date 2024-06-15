using System.Windows;
using System.Windows.Controls;

namespace Mikoto.SettingsPages
{
    /// <summary>
    /// TranslatorGeneralSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class TranslatorGeneralSettingsPage : Page
    {
        private List<string> TranslatorList;

        public TranslatorGeneralSettingsPage()
        {
            InitializeComponent();
            TranslatorList = TranslatorCommon.GetTranslatorList();
            FirstTransComboBox.ItemsSource = TranslatorList;
            SecondTransComboBox.ItemsSource = TranslatorList;

            FirstTransComboBox.SelectedIndex = TranslatorCommon.GetTranslatorIndex(Common.AppSettings.FirstTranslator);
            SecondTransComboBox.SelectedIndex = TranslatorCommon.GetTranslatorIndex(Common.AppSettings.SecondTranslator);

            EachRowTransCheckBox.IsChecked = Common.AppSettings.EachRowTrans;
            HttpProxyBox.Text = Common.AppSettings.HttpProxy;

            TransLimitBox.Value = Common.AppSettings.TransLimitNums;
            // 给TransLimitBox添加Minimum后，初始化它时就会触发一次ValueChanged，导致Settings被设为1，因此只能从设置中读取数据后再添加事件处理函数
            TransLimitBox.ValueChanged += TransLimitBox_ValueChanged;
        }

        private void FirstTransComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Common.AppSettings.FirstTranslator = TranslatorCommon.TranslatorDict[(string)FirstTransComboBox.SelectedValue];
        }

        private void SecondTransComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Common.AppSettings.SecondTranslator = TranslatorCommon.TranslatorDict[(string)SecondTransComboBox.SelectedValue];
        }

        private void EachRowTransCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.EachRowTrans = EachRowTransCheckBox.IsChecked ?? false;
        }

        private void HttpProxyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string text = HttpProxyBox.Text.Trim();
            if (Uri.TryCreate(text, UriKind.RelativeOrAbsolute, out _))
            {
                Common.AppSettings.HttpProxy = text;
            }
            else
            {
                HandyControl.Controls.Growl.Error("Proxy url unsupported.");
            }
        }

        private void TransLimitBox_ValueChanged(object? sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            Common.AppSettings.TransLimitNums = (int)TransLimitBox.Value;
        }
    }
}
