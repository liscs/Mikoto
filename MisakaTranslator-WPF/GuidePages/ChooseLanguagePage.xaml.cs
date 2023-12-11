using DataAccessLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TranslatorLibrary;

namespace MisakaTranslator_WPF.GuidePages
{
    /// <summary>
    /// ChooseLanguagePage.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseLanguagePage : Page
    {
        private readonly List<string> _langList;

        public ChooseLanguagePage()
        {
            InitializeComponent();

            _langList = TranslatorCommon.LanguageDict.Keys.ToList();
            SrcLangCombox.ItemsSource = _langList;
            DstLangCombox.ItemsSource = _langList;

            SrcLangCombox.SelectedIndex = 2;
            DstLangCombox.SelectedIndex = 0;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SrcLangCombox.SelectedIndex == DstLangCombox.SelectedIndex)
            {
                HandyControl.Controls.Growl.Error(Application.Current.Resources["ChooseLanguagePage_NextErrorHint"].ToString());
            }
            else
            {
                Common.UsingSrcLang = TranslatorCommon.LanguageDict[_langList[SrcLangCombox.SelectedIndex]];
                Common.UsingDstLang = TranslatorCommon.LanguageDict[_langList[DstLangCombox.SelectedIndex]];

                //写游戏信息
                GameInfo targetGame = GameHelper.GetUncompletedGameById(Common.GameID);
                if (targetGame != null)
                {
                    targetGame.SrcLang = Common.UsingSrcLang;
                    targetGame.DstLang = Common.UsingDstLang;
                    GameHelper.SaveGameInfo(targetGame);
                }
                //使用路由事件机制通知窗口来完成下一步操作
                PageChangeRoutedEventArgs args = new PageChangeRoutedEventArgs(PageChange.PageChangeRoutedEvent, this)
                {
                    XamlPath = "GuidePages/CompletationPage.xaml"
                };
                this.RaiseEvent(args);
            }
        }
    }
}
