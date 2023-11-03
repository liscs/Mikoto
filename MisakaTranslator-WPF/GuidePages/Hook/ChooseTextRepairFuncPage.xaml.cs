using GameLibraryAccessHelper;
using SQLHelperLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TextHookLibrary;
using TextRepairLibrary;

namespace MisakaTranslator_WPF.GuidePages.Hook
{
    /// <summary>
    /// ChooseTextRepairFuncPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseTextRepairFuncPage : Page
    {
        private List<string> lstRepairFun = TextRepair.lstRepairFun.Keys.ToList();

        public ChooseTextRepairFuncPage()
        {
            InitializeComponent();

            RepairFuncCombox.ItemsSource = lstRepairFun;
            RepairFuncCombox.SelectedIndex = 0;

            Common.textHooker.MeetHookAddressMessageReceived += FilterAndDisplayData;
        }

        public void FilterAndDisplayData(object sender, SolvedDataReceivedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                sourceTextBox.Text = e.Data.Data;
                repairedTextBox.Text = TextRepair.RepairFun_Auto(TextRepair.lstRepairFun[lstRepairFun[RepairFuncCombox.SelectedIndex]], sourceTextBox.Text);

            });
        }

        private void RepairFuncCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (TextRepair.lstRepairFun[lstRepairFun[RepairFuncCombox.SelectedIndex]])
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

            repairedTextBox.Text = TextRepair.RepairFun_Auto(TextRepair.lstRepairFun[lstRepairFun[RepairFuncCombox.SelectedIndex]], sourceTextBox.Text);
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.textHooker.MeetHookAddressMessageReceived -= FilterAndDisplayData;

            Common.UsingRepairFunc = TextRepair.lstRepairFun[lstRepairFun[RepairFuncCombox.SelectedIndex]];

            //写入数据库的去重方法
            if (Common.GameID != null)
            {
                List<GameInfo> allGames = GameLibraryHelper.GetAllGameLibrary();
                GameInfo targetGame = null;
                foreach (GameInfo game in allGames)
                {
                    if (game.GameID == Common.GameID)
                    {
                        targetGame = game;
                        break;
                    }
                }
                switch (TextRepair.lstRepairFun[lstRepairFun[RepairFuncCombox.SelectedIndex]])
                {
                    case "RepairFun_RemoveSingleWordRepeat":
                        targetGame.RepairFunc = Common.UsingRepairFunc;
                        targetGame.RepairParamA = Common.repairSettings.SingleWordRepeatTimes.ToString();
                        GameLibraryHelper.SaveGameInfo(targetGame);
                        break;
                    case "RepairFun_RemoveSentenceRepeat":
                        targetGame.RepairFunc = Common.UsingRepairFunc;
                        targetGame.RepairParamA = Common.repairSettings.SentenceRepeatFindCharNum.ToString();
                        GameLibraryHelper.SaveGameInfo(targetGame);
                        break;
                    case "RepairFun_RegexReplace":
                        targetGame.RepairFunc = Common.UsingRepairFunc;
                        targetGame.RepairParamA = Common.repairSettings.Regex.ToString();
                        targetGame.RepairParamB = Common.repairSettings.Regex_Replace.ToString();
                        GameLibraryHelper.SaveGameInfo(targetGame);
                        break;
                    default:
                        targetGame.RepairFunc = Common.UsingRepairFunc;
                        GameLibraryHelper.SaveGameInfo(targetGame);
                        break;
                }

            }

            //使用路由事件机制通知窗口来完成下一步操作
            PageChangeRoutedEventArgs args = new PageChangeRoutedEventArgs(PageChange.PageChangeRoutedEvent, this);
            args.XamlPath = "GuidePages/ChooseLanguagePage.xaml";
            this.RaiseEvent(args);
        }

        private void SingleConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(Single_TextBox.Text, out int times))
                return;
            Common.repairSettings.SingleWordRepeatTimes = times;
            Common.RepairFuncInit();
            repairedTextBox.Text = TextRepair.RepairFun_RemoveSingleWordRepeat(sourceTextBox.Text);
            Single_InputDrawer.IsOpen = false;
        }

        private void SentenceConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(Sentence_TextBox.Text, out int num))
                return;
            Common.repairSettings.SentenceRepeatFindCharNum = num;
            Common.RepairFuncInit();
            repairedTextBox.Text = TextRepair.RepairFun_RemoveSentenceRepeat(sourceTextBox.Text);
            Sentence_InputDrawer.IsOpen = false;
        }

        private void RegexConfirm_Click(object sender, RoutedEventArgs e)
        {
            Common.repairSettings.Regex = Regex_TextBox.Text;
            Common.repairSettings.Regex_Replace = Replace_TextBox.Text;
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
    }
}
