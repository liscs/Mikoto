using DictionaryHelperLibrary;
using HandyControl.Controls;
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
        private LocalTTS _textSpeechHelper;
        private IDict _dict;

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

                /* 项目“MisakaTranslator-WPF (netcoreapp7.0-windows10.0.22621.0)”的未合并的更改
                在此之前:
                            }


                            if (Common.appSettings.ttsVoice == "")
                在此之后:
                            }


                            if (Common.appSettings.ttsVoice == "")
                */
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

            if (Common.appSettings.xxgPath != string.Empty)
            {
                _dict = new XxgJpzhDict();
                _dict.DictInit(Common.appSettings.xxgPath, string.Empty);
            }

            string ret = _dict.SearchInDict(sourceWord);

            SourceWord.Text = sourceWord;

            Kana.Text = kana;

            this.Topmost = true;
            DicResText.Text = XxgJpzhDict.RemoveHTML(ret);
        }

        ~DictResWindow()
        {
            _textSpeechHelper = null;
            _dict = null;
        }

        private void TTS_Btn_Click(object sender, RoutedEventArgs e)
        {
            _textSpeechHelper.SpeakAsync(sourceWord);
        }

        private void Search_Btn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.baidu.com/s?wd=" + sourceWord);
        }
    }
}
