﻿using DictionaryHelperLibrary;
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

        public DictResWindow(ITTS tsh)
        {
            InitializeComponent();
            _textSpeechHelper = tsh;

            if (tsh == null)
            {
                Growl.InfoGlobal(Application.Current.Resources["TranslateWin_NoTTS_Hint"].ToString());
            }
        }

        private void TTS_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (_textSpeechHelper != null && !string.IsNullOrWhiteSpace(sourceWord))
            {
                _textSpeechHelper.SpeakAsync(sourceWord);
            }
        }

        private void Search_Btn_Click(object sender, RoutedEventArgs e)
        {
            Search(SearchBox.Text);
        }

        public void Search(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) 
                return;
            Dispatcher.BeginInvoke(() =>
            {
                string ret = _ebwinHelper.Search(s);
                this.SourceWord.Text = s;
                this.Topmost = true;
                this.DicResText.Text = HttpUtility.HtmlDecode(ret);
                if (string.IsNullOrWhiteSpace(DicResText.Text))
                {
                    DicResText.Text = (string)FindResource("TranslateWin_DictError_Hint");
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
