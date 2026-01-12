using Mikoto.TextHook;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto.GuidePages.Hook
{
    /// <summary>
    /// ChooseTextRepairFuncPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseTextRepairFuncPage : Page
    {
        GameInfoBuilder _gameInfoBuilder;
        public ChooseTextRepairFuncPage(GameInfoBuilder gameInfoBuilder)
        {
            InitializeComponent();
            _gameInfoBuilder = gameInfoBuilder;
            RepairFuncComboBox.ItemsSource = TextRepair.RepairFunctionNameList.Value;
            TextRepair.RepairFunctionNameList.Value.CollectionChanged += delegate
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    repairedTextBox.Text = TextRepair.PreProcessSrc(TextRepair.DisplayNameToNameDict.Value[RepairFuncComboBox.SelectedValue.ToString()!], sourceTextBox.Text ?? string.Empty);
                });
            };
            RepairFuncComboBox.SelectedIndex = 0;
            App.Env.TextHookService.MeetHookAddressMessageReceived += FilterAndDisplayData;
        }

        public void FilterAndDisplayData(object sender, SolvedDataReceivedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                sourceTextBox.Text = e.Data.Data;
                repairedTextBox.Text = TextRepair.PreProcessSrc(TextRepair.DisplayNameToNameDict.Value[RepairFuncComboBox.SelectedValue.ToString()!], sourceTextBox.Text ?? string.Empty);
            });
        }

        private void RepairFuncComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RepairFuncComboBox.SelectedValue == null)
            {
                RepairFuncComboBox.SelectedIndex = 0;
            }
            string selectedItem = RepairFuncComboBox.SelectedValue!.ToString()!;

            switch (TextRepair.DisplayNameToNameDict.Value[selectedItem])
            {
                case nameof(TextRepair.RepairFun_RemoveSingleWordRepeat):
                    Single_InputDrawer.IsOpen = true;
                    break;
                case nameof(TextRepair.RepairFun_RemoveSentenceRepeat):
                    Sentence_InputDrawer.IsOpen = true;
                    break;
                case nameof(TextRepair.RepairFun_RegexReplace):
                    Regex_InputDrawer.IsOpen = true;
                    break;
            }

            repairedTextBox.Text = TextRepair.PreProcessSrc(TextRepair.DisplayNameToNameDict.Value[selectedItem], sourceTextBox.Text);
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            string selectedItem = RepairFuncComboBox.SelectedValue!.ToString()!;
            _gameInfoBuilder.GameInfo.RepairFunc = TextRepair.DisplayNameToNameDict.Value[selectedItem];

            //使用路由事件机制通知窗口来完成下一步操作
            PageChangeRoutedEventArgs args = new(PageChange.PageChangeRoutedEvent, this)
            {
                Page = new ChooseLanguagePage(_gameInfoBuilder),
            };
            this.RaiseEvent(args);
        }

        private void SingleConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(Single_TextBox.Text, out int times))
                return;
            _gameInfoBuilder.GameInfo.RepairParamA = times.ToString();
            repairedTextBox.Text = TextRepair.RepairFun_RemoveSingleWordRepeat(sourceTextBox.Text, times);
            Single_InputDrawer.IsOpen = false;
        }

        private void SentenceConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(Sentence_TextBox.Text, out int num))
                return;
            _gameInfoBuilder.GameInfo.RepairParamA = num.ToString();
            repairedTextBox.Text = TextRepair.RepairFun_RemoveSentenceRepeat(sourceTextBox.Text, num);
            Sentence_InputDrawer.IsOpen = false;
        }

        private void RegexConfirm_Click(object sender, RoutedEventArgs e)
        {
            _gameInfoBuilder.GameInfo.RepairParamA = Regex_TextBox.Text;
            _gameInfoBuilder.GameInfo.RepairParamB = Replace_TextBox.Text;
            repairedTextBox.Text = TextRepair.RepairFun_RegexReplace(sourceTextBox.Text, Regex_TextBox.Text, Replace_TextBox.Text);
            Regex_InputDrawer.IsOpen = false;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Single_InputDrawer.IsOpen = false;
            Sentence_InputDrawer.IsOpen = false;
            Regex_InputDrawer.IsOpen = false;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            PageChangeRoutedEventArgs args = new(PageChange.PageChangeRoutedEvent, this)
            {
                IsBack = true
            };
            this.RaiseEvent(args);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.NavigationService.CanGoBack)
            {
                BackButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}
