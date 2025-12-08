using Mikoto.Translators.Interfaces;

namespace Mikoto.SettingsPages.TranslatorPages.Models
{
    public class ApiConfigDefinition
    {
        public string? Introduce { get; set; }
        // 通用字段
        public ApiFieldDefinition? AccessKey { get; set; }
        public ApiFieldDefinition? SecretKey { get; set; }
        public ApiFieldDefinition? Region { get; set; }
        public ApiFieldDefinition? Endpoint { get; set; }
        public ApiFieldDefinition? Model { get; set; }

        // 相关链接
        public string? ApplyUrl { get; set; }
        public string? DocUrl { get; set; }
        public string? BillingUrl { get; set; }
        public string? LangCodeUrl { get; set; }

        // 翻译器工厂方法
        public required Func<ITranslator> ConstructeTranslator { get; set; }
        public required Action<ApiConfigDefinition> SaveConfig { get; set; }
    }

}
