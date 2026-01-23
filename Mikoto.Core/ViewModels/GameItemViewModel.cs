using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mikoto.Core.Models;
using Mikoto.DataAccess;
using Mikoto.TextHook;
using System.Diagnostics;

namespace Mikoto.Core.ViewModels;

public partial class GameItemViewModel() : ObservableObject
{
    [ObservableProperty]
    public partial GameInfo GameInfo { get; set; }

    [ObservableProperty]
    public partial object? GameIcon { get; set; }


    [RelayCommand]
    public void StartGame()
    {
        string hookPath = HookFileHelper.ToEntranceFilePath(GameInfo.FilePath);

        Process.Start(hookPath);

        //打开之后切换到翻译页面
        WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(TranslateViewModel), GameInfo));
    }

    [RelayCommand]
    public void LEStartGame()
    {

    }

    [RelayCommand]
    public void EditGame()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(EditGameViewModel), this));
    }
}