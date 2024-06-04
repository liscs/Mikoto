using MisakaTranslator.SettingsPages;
using MisakaTranslator.SettingsPages.DictionaryPages;
using MisakaTranslator.SettingsPages.TranslatorPages;
using MisakaTranslator.SettingsPages.TTSPages;
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
            this.SettingFrame.Navigate(new AboutPage());
        }

        private void Item_TransGeneral_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new TranslatorGeneralSettingsPage());
        }

        private void Item_BaiduTrans_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new BaiduTransSettingsPage());
        }

        private void Item_DeepLTrans_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new DeepLTransSettingsPage());
        }

        private void Item_ChatGPTTrans_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new ChatGPTTransSettingsPage());
        }

        private void Item_AzureTrans_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new AzureTransSettingsPage());
        }

        private void Item_TXOTrans_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new TencentOldTransSettingsPage());
        }

        private void Item_YDZYTrans_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new YoudaoZhiyunTransSettingsPage());
        }

        private void Item_Caiyun_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new CaiyunTransSettingsPage());
        }

        private void Item_JBeijing_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new JbeijingTransSettingsPage());
        }

        private void Item_HookSettings_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new HookSettingsPage());
        }

        private void Item_SoftwareSettings_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new SoftwareSettingsPage());
        }

        private void Item_LESettings_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new LESettingsPage());
        }

        private void Item_EBWinSettings_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new ManageDictionariesPage());
        }

        private void Item_MeCabSettings_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new MecabDictPage());
        }

        private void Item_KingsoftFAIT_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new KingsoftFAITTransSettingsPage());
        }

        private void Item_Dreye_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new DreyeTransSettingsPage());
        }

        private void Item_ChooseTTS_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new TTSGeneralSettingsPage());
        }

        private void Item_LocalTTS_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new LocalTTSSettingsPage());
        }

        private void Item_AzureTTS_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new AzureTTSSettingsPage());
        }

        private void Item_ATSettings_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new ArtificialTransSettingsPage());
        }

        private void Item_Xiaoniu_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new XiaoniuTransSettingsPage());
        }

        private void Item_IBM_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new IBMTransSettingsPage());
        }

        private void Item_Yandex_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new YandexTransSettingsPage());
        }

        private void Item_Volcano_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new VolcanoTransSettingsPage());
        }

        private void Item_Amazon_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new AwsTransSettingsPage());

        }
    }
}