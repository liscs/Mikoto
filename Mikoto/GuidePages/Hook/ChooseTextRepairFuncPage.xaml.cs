using System.Windows;
using System.Windows.Controls;
using TextHookLibrary;

namespace Mikoto.GuidePages.Hook
{
    /// <summary>
    /// ChooseTextRepairFuncPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseTextRepairFuncPage : Page
    {

        public ChooseTextRepairFuncPage()
        {
            InitializeComponent();

            RepairFuncComboBox.ItemsSource = TextRepair.RepairFunctionNameList.Value;
            RepairFuncComboBox.SelectedIndex = 0;

            Common.TextHooker.MeetHookAddressMessageReceived += FilterAndDisplayData;
        }

        public void FilterAndDisplayData(object sender, SolvedDataReceivedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                sourceTextBox.Text = e.Data.Data;
                repairedTextBox.Text = TextRepair.RepairFun_Auto(TextRepair.DisplayNameToNameDict.Value[TextRepair.DisplayNameToNameDict.Value.Keys.ElementAt(RepairFuncComboBox.SelectedIndex)], sourceTextBox.Text ?? string.Empty);
            });
        }

        private void RepairFuncComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedItem = RepairFuncComboBox.SelectedItem.ToString() ?? throw new NullReferenceException();

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

            repairedTextBox.Text = TextRepair.RepairFun_Auto(TextRepair.DisplayNameToNameDict.Value[selectedItem], sourceTextBox.Text);
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.TextHooker.MeetHookAddressMessageReceived -= FilterAndDisplayData;
            string selectedItem = RepairFuncComboBox.SelectedItem.ToString() ?? throw new NullReferenceException();


            Common.UsingRepairFunc = TextRepair.DisplayNameToNameDict.Value[selectedItem];

            //写入去重方法

            switch (TextRepair.DisplayNameToNameDict.Value[selectedItem])
            {
                case nameof(TextRepair.RepairFun_RemoveSingleWordRepeat):
                    GameInfoBuilder.GameInfo.RepairFunc = Common.UsingRepairFunc;
                    GameInfoBuilder.GameInfo.RepairParamA = Common.RepairSettings.SingleWordRepeatTimes.ToString();
                    break;
                case nameof(TextRepair.RepairFun_RemoveSentenceRepeat):
                    GameInfoBuilder.GameInfo.RepairFunc = Common.UsingRepairFunc;
                    GameInfoBuilder.GameInfo.RepairParamA = Common.RepairSettings.SentenceRepeatFindCharNum.ToString();
                    break;
                case nameof(TextRepair.RepairFun_RegexReplace):
                    GameInfoBuilder.GameInfo.RepairFunc = Common.UsingRepairFunc;
                    GameInfoBuilder.GameInfo.RepairParamA = Common.RepairSettings.Regex.ToString();
                    GameInfoBuilder.GameInfo.RepairParamB = Common.RepairSettings.Regex_Replace.ToString();
                    break;
                default:
                    GameInfoBuilder.GameInfo.RepairFunc = Common.UsingRepairFunc;
                    break;
            }
            //使用路由事件机制通知窗口来完成下一步操作
            PageChangeRoutedEventArgs args = new(PageChange.PageChangeRoutedEvent, this)
            {
                Page = new ChooseLanguagePage(),
            };
            this.RaiseEvent(args);
        }

        private void SingleConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(Single_TextBox.Text, out int times))
                return;
            Common.RepairSettings.SingleWordRepeatTimes = times;
            TextRepair.RepairFuncInit();
            repairedTextBox.Text = TextRepair.RepairFun_RemoveSingleWordRepeat(sourceTextBox.Text);
            Single_InputDrawer.IsOpen = false;
        }

        private void SentenceConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(Sentence_TextBox.Text, out int num))
                return;
            Common.RepairSettings.SentenceRepeatFindCharNum = num;
            TextRepair.RepairFuncInit();
            repairedTextBox.Text = TextRepair.RepairFun_RemoveSentenceRepeat(sourceTextBox.Text);
            Sentence_InputDrawer.IsOpen = false;
        }

        private void RegexConfirm_Click(object sender, RoutedEventArgs e)
        {
            Common.RepairSettings.Regex = Regex_TextBox.Text;
            Common.RepairSettings.Regex_Replace = Replace_TextBox.Text;
            TextRepair.RepairFuncInit();
            repairedTextBox.Text = TextRepair.RepairFun_RegexReplace(sourceTextBox.Text);
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
