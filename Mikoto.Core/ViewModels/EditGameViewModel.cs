using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mikoto.Core.Interfaces;
using Mikoto.Core.Models;
using Mikoto.Core.ViewModels.AddGame;

namespace Mikoto.Core.ViewModels
{
    public partial class EditGameViewModel(IAppEnvironment env) : ObservableObject
    {
        [ObservableProperty]
        public partial GameItemViewModel GameItem { get; set; }

        [ObservableProperty]
        public partial RepairFunctionViewModel RepairFunctionViewModel { get; set; }

        [ObservableProperty]
        public partial LanguageViewModel LanguageViewModel { get; set; } = new();

        [ObservableProperty]
        public partial bool ShowSuccessInfo { get; set; }

        [RelayCommand]
        public void Cancel()
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(HomeViewModel)));
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            // 先同步 ComboBox 的选择项到 GameInfo
            this.GameItem.GameInfo.SrcLang = this.LanguageViewModel.SelectedSourceLanguage.LanguageCode;
            this.GameItem.GameInfo.DstLang = this.LanguageViewModel.SelectedTargetLanguage.LanguageCode;
            this.GameItem.GameInfo.RepairFunc = this.RepairFunctionViewModel.SelectedRepairFunction.MethodName;

            env.GameInfoService.SaveGameInfo(this.GameItem.GameInfo);

            ShowSuccessInfo = true;
        }
    }
}
