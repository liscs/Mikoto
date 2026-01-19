using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mikoto.DataAccess;
using Mikoto.Fluent.AddGamePages;
using System.Collections.ObjectModel;
namespace Mikoto.Fluent
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _searchQuery;

        public ObservableCollection<GameInfo> Games { get; } = new();

        // 自动生成 SearchCommand
        [RelayCommand]
        private void Search()
        {
            // 处理搜索逻辑
        }

    }
}
