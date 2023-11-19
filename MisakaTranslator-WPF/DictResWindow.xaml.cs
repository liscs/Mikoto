using Castle.Components.DictionaryAdapter;
using DictionaryHelperLibrary;
using HandyControl.Controls;
using System;
using System.ComponentModel;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using TTSHelperLibrary;

namespace MisakaTranslator_WPF
{
    /// <summary>
    /// DictResWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DictResWindow : System.Windows.Window
    {
        private string sourceWord;
        private LocalTTS _textSpeechHelper;
        static private EbwinHelper _ebwinHelper = new EbwinHelper();

        public DictResWindow(string word, string kana = "----", LocalTTS tsh = null)
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
            else
            {
                _textSpeechHelper.SetTTSVoice(Common.appSettings.ttsVoice);
                _textSpeechHelper.SetVolume(Common.appSettings.ttsVolume);
                _textSpeechHelper.SetRate(Common.appSettings.ttsRate);
            }

            string ret = _ebwinHelper.Search(sourceWord);
            SourceWord.Text = sourceWord;
            Kana.Text = kana;
            this.Topmost = true;
            DicResText.Text = HttpUtility.HtmlDecode(ret);
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
            Dispatcher.BeginInvoke(new Action(() =>
            {
                string ret = _ebwinHelper.Search(s);
                this.SourceWord.Text = s;
                this.Kana.Text = string.Empty;
                this.Topmost = true;
                this.DicResText.Text = HttpUtility.HtmlDecode(ret);
                if (string.IsNullOrWhiteSpace(DicResText.Text))
                {
                    DicResText.Text = (string)FindResource("TranslateWin_DictError_Hint") ;
                }
            }));

        }
        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }
    }
}
