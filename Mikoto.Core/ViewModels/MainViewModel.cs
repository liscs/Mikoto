using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mikoto.DataAccess;
using System.Collections.ObjectModel;
namespace Mikoto.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    public ObservableCollection<GameInfo> Games { get; } = new();

    // 自动生成 SearchCommand
    [RelayCommand]
    private void Search()
    {
        // 处理搜索逻辑
    }

}

