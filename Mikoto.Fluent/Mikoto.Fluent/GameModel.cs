using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Mikoto.Fluent
{
    public partial class GameModel : ObservableObject
    {
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