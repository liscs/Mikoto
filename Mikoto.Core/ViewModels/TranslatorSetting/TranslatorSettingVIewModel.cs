using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mikoto.Core.Interfaces;
using Mikoto.Core.Models.Setting;
using Mikoto.Translators;
using Mikoto.Translators.Implementations;

namespace Mikoto.Core.ViewModels.TranslatorSetting;

public partial class TranslatorSettingViewModel(IAppEnvironment env) : ObservableObject
{
    // 存储已生成的 VM
    [ObservableProperty]
    public partial CommonTranslatorSettingsViewModel ChatGPTVM { get; set; }

    [ObservableProperty]
    public partial CommonTranslatorSettingsViewModel GoogleVM { get; set; }

    [RelayCommand]
    private void InitProvider(string provider)
    {
        // 使用 C# 模式匹配，将赋值逻辑压缩
        _ = provider switch
        {
            "ChatGPT" => ChatGPTVM ??= new(CreateChatGPTConfig()),
            "Google Cloud" => GoogleVM ??= new(CreateGoogleCloudConfig()),
            _ => null
        };
    }

    private ApiConfigDefinition CreateGoogleCloudConfig()
    {
        return new ApiConfigDefinition
        {
            Introduce = env.ResourceService.Get("GoogleCloudTranslator_Introduce"),
            SecretKey = new ApiFieldDefinition
            {
                Title = "Key",
                Value = env.AppSettings.GoogleSecretKey,
            },
            ApplyUrl = GoogleCloudTranslator.GetUrl_API(),
            DocUrl = GoogleCloudTranslator.GetUrl_Doc(),
            BillingUrl = GoogleCloudTranslator.GetUrl_Bill(),
            ConstructeTranslator = () =>
            {
                return new GoogleCloudTranslator(env.ResourceService.Get(nameof(GoogleCloudTranslator)), env.AppSettings.GoogleSecretKey, TranslateHttpClient.Instance);
            },
            SaveConfig = (fields) =>
            {
                env.AppSettings.GoogleSecretKey = fields.SecretKey?.Value??string.Empty;
            }
        };
    }

    private ApiConfigDefinition CreateChatGPTConfig()
    {
        return new ApiConfigDefinition
        {
            Introduce = env.ResourceService.Get("ChatGPTTransSettingsPage_Introduce"),
            SecretKey = new ApiFieldDefinition
            {
                Title = env.ResourceService.Get("ChatGPTTransSettingsPage_secretKey"),
                Value = env.AppSettings.ChatGPTapiKey,
            },
            Endpoint = new ApiFieldDefinition
            {
                Title = env.ResourceService.Get("ChatGPTTransSettingsPage_apiUrl"),
                Value = env.AppSettings.ChatGPTapiUrl,
            },
            Model = new ApiFieldDefinition
            {
                Title = env.ResourceService.Get("ChatGPTTransSettingsPage_apiModel"),
                Value = env.AppSettings.ChatGPTapiModel,
            },
            ApplyUrl = ChatGPTTranslator.GetUrl_API(),
            DocUrl = ChatGPTTranslator.GetUrl_Doc(),
            BillingUrl = ChatGPTTranslator.GetUrl_Bill(),
            ConstructeTranslator = () =>
            {
                return new ChatGPTTranslator(env.ResourceService.Get(nameof(ChatGPTTranslator)), env.AppSettings.ChatGPTapiKey, env.AppSettings.ChatGPTapiUrl, env.AppSettings.ChatGPTapiModel, TranslateHttpClient.Instance);
            },

            SaveConfig = (fields) =>
            {
                env.AppSettings.ChatGPTapiKey = fields.SecretKey?.Value??string.Empty;
                env.AppSettings.ChatGPTapiUrl = fields.Endpoint?.Value??string.Empty;
                env.AppSettings.ChatGPTapiModel = fields.Model?.Value??string.Empty;
            }
        };
    }
}
