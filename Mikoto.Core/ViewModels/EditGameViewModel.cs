using CommunityToolkit.Mvvm.ComponentModel;
using Mikoto.Core.ViewModels.AddGame;

namespace Mikoto.Core.ViewModels
{
    public partial class EditGameViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial GameItemViewModel GameItem { get; set; }

        [ObservableProperty]
        public partial RepairFunctionViewModel RepairFunctionViewModel { get; set; }

        [ObservableProperty]
        public partial LanguageViewModel LanguageViewModel { get; set; } = new();
    }
}
