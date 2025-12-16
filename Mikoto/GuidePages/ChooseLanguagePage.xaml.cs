using Mikoto.Translators;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto.GuidePages
{
    /// <summary>
    /// ChooseLanguagePage.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseLanguagePage : Page
    {
        private readonly GameInfoBuilder _gameInfoBuilder;
        private readonly List<string> _langList;

        public ChooseLanguagePage(GameInfoBuilder gameInfoBuilder)
        {
            InitializeComponent();
            _gameInfoBuilder = gameInfoBuilder;
            _langList = TranslatorCommon.LanguageDict.Keys.ToList();
            SrcLangComboBox.ItemsSource = _langList;
            DstLangComboBox.ItemsSource = _langList;

            SrcLangComboBox.SelectedIndex = 3;
            DstLangComboBox.SelectedIndex = 0;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SrcLangComboBox.SelectedIndex == DstLangComboBox.SelectedIndex)
            {
                HandyControl.Controls.Growl.Error(Application.Current.Resources["ChooseLanguagePage_NextErrorHint"].ToString());
            }
            else
            {
                App.Env.Context.UsingSrcLang = TranslatorCommon.LanguageDict[_langList[SrcLangComboBox.SelectedIndex]];
                App.Env.Context.UsingDstLang = TranslatorCommon.LanguageDict[_langList[DstLangComboBox.SelectedIndex]];

                //写游戏信息
                _gameInfoBuilder.GameInfo.SrcLang = App.Env.Context.UsingSrcLang;
                _gameInfoBuilder.GameInfo.DstLang = App.Env.Context.UsingDstLang;

                //使用路由事件机制通知窗口来完成下一步操作
                PageChangeRoutedEventArgs args = new(PageChange.PageChangeRoutedEvent, this)
                {
                    Page = new CompletationPage()
                };
                this.RaiseEvent(args);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            PageChangeRoutedEventArgs args = new PageChangeRoutedEventArgs(PageChange.PageChangeRoutedEvent, this)
            {
                IsBack = true
            };
            this.RaiseEvent(args);
        }
    }
}
