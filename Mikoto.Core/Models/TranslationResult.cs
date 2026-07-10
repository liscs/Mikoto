using CommunityToolkit.Mvvm.ComponentModel;

namespace Mikoto.Core.Models;

public partial class TranslationResult : ObservableObject
{
    public string TranslatorName { get; init; } = string.Empty;
    public string TranslatorDisplayName { get; init; } = string.Empty;
    [ObservableProperty] public partial string? ResultText { get; set; }
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
}