using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace Mikoto.Fluent
{
    public partial class GameModel : ObservableObject
    {
        [ObservableProperty] private string _gameName;
        [ObservableProperty] private ImageSource _gameIcon;
        [ObservableProperty] private DateTime _lastPlayAt;

        public string ExePath { get; set; } // 路径一般不改，不需要 Observable
    }
}