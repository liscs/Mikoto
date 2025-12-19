using Mikoto.SettingsPages;
using Mikoto.SettingsPages.DictionaryPages;
using Mikoto.SettingsPages.TranslatorPages;
using Mikoto.SettingsPages.TranslatorPages.Models;
using Mikoto.SettingsPages.TTSPages;
using Mikoto.Translators;
using Mikoto.Translators.Implementations;
using System.Windows;

namespace Mikoto
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow
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
            var config = new ApiConfigDefinition
            {
                Introduce = App.Env.ResourceService.Get("ChatGPTTransSettingsPage_Introduce"),
                SecretKey = new ApiFieldDefinition
                {
                    Title = App.Env.ResourceService.Get("ChatGPTTransSettingsPage_secretKey"),
                    Value = Common.AppSettings.ChatGPTapiKey,
                },
                Endpoint = new ApiFieldDefinition
                {
                    Title = App.Env.ResourceService.Get("ChatGPTTransSettingsPage_apiUrl"),
                    Value = Common.AppSettings.ChatGPTapiUrl,
                },
                Model = new ApiFieldDefinition
                {
                    Title = App.Env.ResourceService.Get("ChatGPTTransSettingsPage_apiModel"),
                    Value = Common.AppSettings.ChatGPTapiModel,
                },
                ApplyUrl = ChatGPTTranslator.GetUrl_API(),
                DocUrl = ChatGPTTranslator.GetUrl_Doc(),
                BillingUrl = ChatGPTTranslator.GetUrl_Bill(),
                ConstructeTranslator = () =>
                {
                    return new ChatGPTTranslator(App.Env.ResourceService.Get(nameof(ChatGPTTranslator)), Common.AppSettings.ChatGPTapiKey, Common.AppSettings.ChatGPTapiUrl, Common.AppSettings.ChatGPTapiModel, TranslateHttpClient.Instance);
                },
                SaveConfig = (fields) =>
                {
                    Common.AppSettings.ChatGPTapiKey = fields.SecretKey?.Value??string.Empty;
                    Common.AppSettings.ChatGPTapiUrl = fields.Endpoint?.Value??string.Empty;
                    Common.AppSettings.ChatGPTapiModel = fields.Model?.Value??string.Empty;
                }
            };
            this.SettingFrame.Navigate(new CommonTranslatorSettingsPage(config));
        }

        private void Item_AzureTrans_Selected(object sender, RoutedEventArgs e)
        {
            var config = new ApiConfigDefinition
            {
                Introduce = App.Env.ResourceService.Get("AzureTransSettingsPage_Introduce"),
                SecretKey = new ApiFieldDefinition
                {
                    Title = App.Env.ResourceService.Get("AzureTransSettingsPage_secretKey"),
                    Value = Common.AppSettings.AzureSecretKey,
                },
                Region = new ApiFieldDefinition
                {
                    Title = App.Env.ResourceService.Get("AzureTransSettingsPage_location"),
                    Value = Common.AppSettings.AzureLocation,
                },
                ApplyUrl = AzureTranslator.GetUrl_API(),
                DocUrl = AzureTranslator.GetUrl_Doc(),
                BillingUrl = AzureTranslator.GetUrl_Bill(),
                LangCodeUrl = AzureTranslator.GetUrl_lang(),
                ConstructeTranslator = () =>
                {
                    return new AzureTranslator(App.Env.ResourceService.Get(nameof(AzureTranslator)), Common.AppSettings.AzureSecretKey, Common.AppSettings.AzureLocation, TranslateHttpClient.Instance);
                },
                SaveConfig = (fields) =>
                {
                    Common.AppSettings.AzureSecretKey = fields.SecretKey?.Value??string.Empty;
                    Common.AppSettings.AzureLocation = fields.Region?.Value??string.Empty;
                }
            };
            this.SettingFrame.Navigate(new CommonTranslatorSettingsPage(config));
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
            var config = new ApiConfigDefinition
            {
                Introduce = App.Env.ResourceService.Get("XiaoniuTransSettingsPage_Introduce"),
                SecretKey = new ApiFieldDefinition
                {
                    Title = App.Env.ResourceService.Get("XiaoniuTransSettingsPage_apikey"),
                    Value = Common.AppSettings.XiaoniuApiKey,
                },
                ApplyUrl = XiaoniuTranslator.GetUrl_API(),
                DocUrl = XiaoniuTranslator.GetUrl_Doc(),
                BillingUrl = XiaoniuTranslator.GetUrl_Bill(),
                ConstructeTranslator = () =>
                {
                    return new XiaoniuTranslator(App.Env.ResourceService.Get(nameof(XiaoniuTranslator)), Common.AppSettings.XiaoniuApiKey, TranslateHttpClient.Instance);
                },
                SaveConfig = (fields) =>
                {
                    Common.AppSettings.XiaoniuApiKey = fields.SecretKey?.Value??string.Empty;
                }
            };
            this.SettingFrame.Navigate(new CommonTranslatorSettingsPage(config));
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
            var config = new ApiConfigDefinition
            {
                Introduce = App.Env.ResourceService.Get("VolcanoTransSettingsPage_Introduce"),
                AccessKey = new ApiFieldDefinition
                {
                    Title = "Access Key ID",
                    Value = Common.AppSettings.VolcanoId,
                },
                SecretKey = new ApiFieldDefinition
                {
                    Title = "Secret Access Key",
                    Value = Common.AppSettings.VolcanoKey,
                },
                LangCodeUrl = VolcanoTranslator.GetUrl_lang(),
                ApplyUrl = VolcanoTranslator.GetUrl_API(),
                DocUrl = VolcanoTranslator.GetUrl_Doc(),
                BillingUrl = VolcanoTranslator.GetUrl_Bill(),
                ConstructeTranslator = () =>
                {
                    return new VolcanoTranslator(App.Env.ResourceService.Get(nameof(VolcanoTranslator)), Common.AppSettings.VolcanoId, Common.AppSettings.VolcanoKey, TranslateHttpClient.Instance);
                },
                SaveConfig = (fields) =>
                {
                    Common.AppSettings.VolcanoId = fields.AccessKey?.Value??string.Empty;
                    Common.AppSettings.VolcanoKey = fields.SecretKey?.Value??string.Empty;
                }
            };
            this.SettingFrame.Navigate(new CommonTranslatorSettingsPage(config));
        }

        private void Item_Amazon_Selected(object sender, RoutedEventArgs e)
        {
            this.SettingFrame.Navigate(new AwsTransSettingsPage());

        }

        private void Item_GoogleTrans_Selected(object sender, RoutedEventArgs e)
        {
            var config = new ApiConfigDefinition
            {
                Introduce = App.Env.ResourceService.Get("GoogleCloudTranslator_Introduce"),
                SecretKey = new ApiFieldDefinition
                {
                    Title = "Key",
                    Value = Common.AppSettings.GoogleSecretKey,
                },
                ApplyUrl = GoogleCloudTranslator.GetUrl_API(),
                DocUrl = GoogleCloudTranslator.GetUrl_Doc(),
                BillingUrl = GoogleCloudTranslator.GetUrl_Bill(),
                ConstructeTranslator = () =>
                {
                    return new GoogleCloudTranslator(App.Env.ResourceService.Get(nameof(GoogleCloudTranslator)), Common.AppSettings.GoogleSecretKey, TranslateHttpClient.Instance);
                },
                SaveConfig = (fields) =>
                {
                    Common.AppSettings.GoogleSecretKey = fields.SecretKey?.Value??string.Empty;
                }
            };
            this.SettingFrame.Navigate(new CommonTranslatorSettingsPage(config));

        }
    }
}