using DictionaryHelperLibrary;
using HandyControl.Controls;
using System.ComponentModel;
using System.Web;
using System.Windows;
using TTSHelperLibrary;

namespace MisakaTranslator_WPF
{
    /// <summary>
    /// DictResWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DictResWindow : System.Windows.Window
    {
        private string sourceWord;
        private ITTS _textSpeechHelper;
        static private EbwinHelper _ebwinHelper = new EbwinHelper();

        public DictResWindow(string word, ITTS tsh = null)
        {
            sourceWord = word;
            InitializeComponent();
            if (tsh == null)
            {
                _textSpeechHelper = new LocalTTS();
            }
            else
            {
                _textSpeechHelper = tsh;
            }


            if (Common.appSettings.ttsVoice == "")
            {
                Growl.InfoGlobal(Application.Current.Resources["TranslateWin_NoTTS_Hint"].ToString());
            }
            Dispatcher.BeginInvoke(() => {
                string ret = _ebwinHelper.Search(sourceWord);
                SourceWord.Text = sourceWord;
                this.Topmost = true;
                DicResText.Text = HttpUtility.HtmlDecode(ret);
            });
        }

        private void TTS_Btn_Click(object sender, RoutedEventArgs e)
        {
            _textSpeechHelper.SpeakAsync(sourceWord);
        }

        private void Search_Btn_Click(object sender, RoutedEventArgs e)
        {
            SearchAndShow(SearchBox.Text);
        }
        public void SearchAndShow(string s)
        {
            Dispatcher.BeginInvoke(() =>
            {
                string ret = _ebwinHelper.Search(s);
                this.SourceWord.Text = s;
                this.Topmost = true;
                this.DicResText.Text = HttpUtility.HtmlDecode(ret);
                if (string.IsNullOrWhiteSpace(DicResText.Text))
                {
                    DicResText.Text = (string)FindResource("TranslateWin_DictError_Hint") ;
                }
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }
    }
}
