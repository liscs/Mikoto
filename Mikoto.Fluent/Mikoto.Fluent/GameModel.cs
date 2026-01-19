using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Mikoto.Fluent
{
    public partial class GameModel : ObservableObject
    {
        // 在加载游戏列表时，把 ViewModel 的引用塞给每个模型
        public HomeViewModel? Parent { get; init; }

        [ObservableProperty]
        public partial string? GameName { get; set; }

        [ObservableProperty]
        public partial ImageSource? GameIcon { get; set; }

        [ObservableProperty]
        public partial DateTime LastPlayAt { get; set; }

        [ObservableProperty]
        public partial string ExePath { get; set; }
    }
}