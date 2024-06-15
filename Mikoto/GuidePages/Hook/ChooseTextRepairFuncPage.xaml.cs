using System.Windows;
using System.Windows.Controls;
using TextHookLibrary;
using TextRepairLibrary;

namespace Mikoto.GuidePages.Hook
{
    /// <summary>
    /// ChooseTextRepairFuncPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseTextRepairFuncPage : Page
    {
        private List<string> lstRepairFun = TextRepair.LstRepairFun.Keys.ToList();

        public ChooseTextRepairFuncPage()
        {
            InitializeComponent();

            RepairFuncComboBox.ItemsSource = lstRepairFun;
            RepairFuncComboBox.SelectedIndex = 0;

            Common.TextHooker!.MeetHookAddressMessageReceived += FilterAndDisplayData;
        }

        public void FilterAndDisplayData(object sender, SolvedDataReceivedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                sourceTextBox.Text = e.Data.Data;
                repairedTextBox.Text = TextRepair.RepairFun_Auto(TextRepair.LstRepairFun[lstRepairFun[RepairFuncComboBox.SelectedIndex]], sourceTextBox.Text ?? string.Empty);
            });
        }

        private void RepairFuncComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (TextRepair.LstRepairFun[lstRepairFun[RepairFuncComboBox.SelectedIndex]])
            {
                case "RepairFun_RemoveSingleWordRepeat":
                    Single_InputDrawer.IsOpen = true;
                    break;
                case "RepairFun_RemoveSentenceRepeat":
                    Sentence_InputDrawer.IsOpen = true;
                    break;
                case "RepairFun_RegexReplace":
                    Regex_InputDrawer.IsOpen = true;
                    break;
            }

            repairedTextBox.Text = TextRepair.RepairFun_Auto(TextRepair.LstRepairFun[lstRepairFun[RepairFuncComboBox.SelectedIndex]], sourceTextBox.Text);
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Common.TextHooker != null)
            {
                Common.TextHooker.MeetHookAddressMessageReceived -= FilterAndDisplayData;
            }
            Common.UsingRepairFunc = TextRepair.LstRepairFun[lstRepairFun[RepairFuncComboBox.SelectedIndex]];

            //写入去重方法

            switch (TextRepair.LstRepairFun[lstRepairFun[RepairFuncComboBox.SelectedIndex]])
            {
                case "RepairFun_RemoveSingleWordRepeat":
                    GameInfoBuilder.GameInfo.RepairFunc = Common.UsingRepairFunc;
                    GameInfoBuilder.GameInfo.RepairParamA = Common.RepairSettings.SingleWordRepeatTimes.ToString();
                    break;
                case "RepairFun_RemoveSentenceRepeat":
                    GameInfoBuilder.GameInfo.RepairFunc = Common.UsingRepairFunc;
                    GameInfoBuilder.GameInfo.RepairParamA = Common.RepairSettings.SentenceRepeatFindCharNum.ToString();
                    break;
                case "RepairFun_RegexReplace":
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
            Common.RepairFuncInit();
            repairedTextBox.Text = TextRepair.RepairFun_RemoveSingleWordRepeat(sourceTextBox.Text);
            Single_InputDrawer.IsOpen = false;
        }

        private void SentenceConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(Sentence_TextBox.Text, out int num))
                return;
            Common.RepairSettings.SentenceRepeatFindCharNum = num;
            Common.RepairFuncInit();
            repairedTextBox.Text = TextRepair.RepairFun_RemoveSentenceRepeat(sourceTextBox.Text);
            Sentence_InputDrawer.IsOpen = false;
        }

        private void RegexConfirm_Click(object sender, RoutedEventArgs e)
        {
            Common.RepairSettings.Regex = Regex_TextBox.Text;
            Common.RepairSettings.Regex_Replace = Replace_TextBox.Text;
            Common.RepairFuncInit();
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
