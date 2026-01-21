using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Mikoto.Core.ViewModels.AddGamePages;

public record LanguageItem(string DisplayName, string LanguageCode);

public partial class LanguageViewModel : ObservableObject
{
    // 支持的语言列表
    public ObservableCollection<LanguageItem> SupportedLanguages { get; } = new()
    {
        new("简体中文", "zh"),
        new("繁體中文", "zh-Hant"),
        new("English", "en"),
        new("日本語", "ja"),
        new("한국어", "ko"),
        new("Русскийязык", "ru"),
        new("Français", "fr"),
        new("Español", "es"),
        new("Português", "pt"),
        new("Deutsch", "de"),
        new("Italiano", "it")
    };

    [ObservableProperty]
    public partial LanguageItem SelectedSourceLanguage { get; set; }

    [ObservableProperty]
    public partial LanguageItem SelectedTargetLanguage { get; set; }

    public LanguageViewModel()
    {
        // 默认设置
        SelectedSourceLanguage = SupportedLanguages.First(x => x.LanguageCode == "ja");
        SelectedTargetLanguage = SupportedLanguages.First(x => x.LanguageCode == "zh");
    }
}
