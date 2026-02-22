using CommunityToolkit.Mvvm.ComponentModel;
using Mikoto.Core.Models.Setting;
using Mikoto.Translators.Interfaces;

namespace Mikoto.Core.ViewModels.TranslatorSetting;

public partial class CommonTranslatorSettingsViewModel(ApiConfigDefinition def) : ObservableObject
{
    public ApiConfigDefinition Definition { get; } = def;

    public bool AccessKeyVisible => Definition.AccessKey != null;
    public bool SecretKeyVisible => Definition.SecretKey != null;
    public bool RegionVisible => Definition.Region != null;
    public bool EndpointVisible => Definition.Endpoint != null;
    public bool ModelVisible => Definition.Model != null;


    // 创建翻译器实例
    public ITranslator? CreateTranslatorInstance()
    {
        return Definition.ConstructeTranslator();
    }


    public bool LangCodeVisible => !string.IsNullOrWhiteSpace(Definition.LangCodeUrl);


    public bool DocVisible => !string.IsNullOrWhiteSpace(Definition.DocUrl);


    public bool BillVisible => !string.IsNullOrWhiteSpace(Definition.BillingUrl);

    public bool DescriptionVisible => !string.IsNullOrWhiteSpace(Definition.Introduce);

    public bool ApplyVisible => !string.IsNullOrWhiteSpace(Definition.ApplyUrl);

}
