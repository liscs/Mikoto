using CommunityToolkit.Mvvm.ComponentModel;
using Mikoto.Core.Models.AddGame;

namespace Mikoto.Core.ViewModels.AddGame;

public partial class LanguageViewModel : ObservableObject
{
    // 支持的语言列表
    private List<LanguageItem> GetLangList()
    {
        var list = new List<LanguageItem>()
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

        return list;
    }

    public List<LanguageItem> LangList { get; }
    [ObservableProperty]
    public partial LanguageItem SelectedSourceLanguage { get; set; }

    [ObservableProperty]
    public partial LanguageItem SelectedTargetLanguage { get; set; }

    public LanguageViewModel()
    {
        LangList = GetLangList();
        SelectedSourceLanguage = LangList.First(x => x.LanguageCode == "ja");
        SelectedTargetLanguage = LangList.First(x => x.LanguageCode == "zh");
    }
}
