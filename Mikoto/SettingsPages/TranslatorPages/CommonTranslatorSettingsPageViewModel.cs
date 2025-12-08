using Mikoto.Helpers.ViewModel;
using Mikoto.SettingsPages.TranslatorPages.Models;
using Mikoto.Translators.Interfaces;
using System.Windows;

namespace Mikoto.SettingsPages.TranslatorPages
{
    internal class CommonTranslatorSettingsPageViewModel(ApiConfigDefinition def) : ViewModelBase
    {
        public ApiConfigDefinition Definition { get; } = def;

        public Visibility AccessKeyVisibility => Definition.AccessKey != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SecretKeyVisibility => Definition.SecretKey != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility RegionVisibility => Definition.Region != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EndpointVisibility => Definition.Endpoint != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ModelVisibility => Definition.Model != null ? Visibility.Visible : Visibility.Collapsed;


        // 创建翻译器实例
        public ITranslator? CreateTranslatorInstance()
        {
            return Definition.ConstructeTranslator();
        }


        public Visibility LangCodeVisibility => string.IsNullOrWhiteSpace(Definition.LangCodeUrl) ? Visibility.Collapsed : Visibility.Visible;


        public Visibility DocVisibility => string.IsNullOrWhiteSpace(Definition.DocUrl) ? Visibility.Collapsed : Visibility.Visible;


        public Visibility BillVisibility => string.IsNullOrWhiteSpace(Definition.BillingUrl) ? Visibility.Collapsed : Visibility.Visible;

        public Visibility DescriptionVisibility => string.IsNullOrWhiteSpace(Definition.Introduce) ? Visibility.Collapsed : Visibility.Visible;

        public Visibility ApplyVisibility => string.IsNullOrWhiteSpace(Definition.ApplyUrl) ? Visibility.Collapsed : Visibility.Visible;

    }
}
