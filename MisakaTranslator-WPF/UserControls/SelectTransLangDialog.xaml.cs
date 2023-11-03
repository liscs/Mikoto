using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TranslatorLibrary;

namespace MisakaTranslator_WPF.UserControls
{
    /// <summary>
    /// SelectTransLangDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SelectTransLangDialog : UserControl
    {
        private readonly List<string> _langList;
        ComicTranslator.ComicTransMainWindow _win;

        public SelectTransLangDialog(ComicTranslator.ComicTransMainWindow win)
        {
            InitializeComponent();

            _win = win;

            _langList = TranslatorCommon.LanguageList.Keys.ToList();
            SrcLangCombox.ItemsSource = _langList;
            DstLangCombox.ItemsSource = _langList;

            SrcLangCombox.SelectedIndex = 2;
            DstLangCombox.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _win.SrcLang = TranslatorCommon.LanguageList[_langList[SrcLangCombox.SelectedIndex]];
            _win.DstLang = TranslatorCommon.LanguageList[_langList[DstLangCombox.SelectedIndex]];
            _win.DstLang = TranslatorCommon.LanguageList[_langList[DstLangCombox.SelectedIndex]];

            if (_win.SrcLang == "" || _win.DstLang == "" || _win.SrcLang == _win.DstLang)
            {
                HandyControl.Controls.Growl.ErrorGlobal(Application.Current.Resources["ChooseLanguagePage_NextErrorHint"].ToString());
                _win.Close();
            }
        }
    }
}
