using SQLHelperLibrary;
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

            _langList = TranslatorCommon.LanguageList.Keys.ToList();
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
                Common.UsingSrcLang = TranslatorCommon.LanguageList[_langList[SrcLangCombox.SelectedIndex]];
                Common.UsingDstLang = TranslatorCommon.LanguageList[_langList[DstLangCombox.SelectedIndex]];

                //写数据库信息
                if (Common.GameID != -1)
                {
                    GameLibraryHelper.sqlHelper.ExecuteSql(
                        $"UPDATE game_library SET src_lang = '{Common.UsingSrcLang}' WHERE gameid = {Common.GameID};");
                    GameLibraryHelper.sqlHelper.ExecuteSql(
                        $"UPDATE game_library SET dst_lang = '{Common.UsingDstLang}' WHERE gameid = {Common.GameID};");
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
