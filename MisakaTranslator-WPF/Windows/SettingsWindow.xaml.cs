using System.Windows;

namespace MisakaTranslator
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void Item_About_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/AboutPage.xaml", UriKind.Relative));
        }

        private void Item_TransGeneral_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/TranslatorGeneralSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_BaiduTrans_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/BaiduTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_DeepLTrans_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/DeepLTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_ChatGPTTrans_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/ChatGPTTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_AzureTrans_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/AzureTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_TXOTrans_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/TencentOldTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_YDZYTrans_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/YoudaoZhiyunTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_Caiyun_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/CaiyunTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_JBeijing_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/JbeijingTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_BaiduOCR_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/OCRPages/BaiduOCRSettingsPage.xaml", UriKind.Relative));
        }
        private void Item_BaiduFanyiOCR_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/OCRPages/BaiduFanyiOCRSettingsPage.xaml", UriKind.Relative));
        }
        private void Item_TencentOCR_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/OCRPages/TencentOCRSettingsPage.xaml", UriKind.Relative));
        }
        private void Item_TesseractCli_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/OCRPages/TesseractCliSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_OCRGeneral_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/OCRPages/OCRGeneralSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_HookSettings_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/HookSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_SoftwareSettings_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/SoftwareSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_LESettings_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/LESettingsPage.xaml", UriKind.Relative));
        }

        private void Item_EBWinSettings_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/DictionaryPages/EBwinDictPage.xaml", UriKind.Relative));
        }

        private void Item_MeCabSettings_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/DictionaryPages/MecabDictPage.xaml", UriKind.Relative));
        }

        private void Item_KingsoftFAIT_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/KingsoftFAITTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_Dreye_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/DreyeTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_ChooseTTS_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TTSPages/TTSGeneralSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_LocalTTS_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TTSPages/LocalTTSSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_AzureTTS_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TTSPages/AzureTTSSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_ATSettings_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/ArtificialTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_Xiaoniu_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/XiaoniuTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_IBM_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/IBMTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_Yandex_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/YandexTransSettingsPage.xaml", UriKind.Relative));
        }

        private void Item_Volcano_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new Uri("/SettingsPages/TranslatorPages/VolcanoTransSettingsPage.xaml", UriKind.Relative));
        }
    }
}