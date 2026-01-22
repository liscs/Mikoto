using CommunityToolkit.Mvvm.ComponentModel;
using Mikoto.DataAccess;

namespace Mikoto.Core.ViewModels;

public partial class GameItemViewModel() : ObservableObject
{
    public required HomeViewModel Parent { get; init; }

    [ObservableProperty]
    public partial GameInfo GameInfo { get; set; }

    [ObservableProperty]
    public partial object? GameIcon { get; set; }
}