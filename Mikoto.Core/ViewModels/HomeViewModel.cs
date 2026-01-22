using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mikoto.Core.Interfaces;
using Mikoto.Core.Models;
using Mikoto.Core.ViewModels.AddGame;
using Mikoto.DataAccess;
using Serilog;
using System.Collections.ObjectModel;
using System.Diagnostics;
namespace Mikoto.Core.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IAppEnvironment _env;

    public HomeViewModel(IAppEnvironment env)
    {
        _env = env;
    }

    public ObservableCollection<GameItemViewModel> Games { get; } = new();

    [RelayCommand]
    public async Task LoadGamesAsync(Func<string, Task<object?>> getIconFunc)
    {
        // 1. 获取原始数据
        _env.GameInfoService.GetAllCompletedGames();
        var savedData = _env.GameInfoService.AllCompletedGamesIdDict.Values;

        // 2. 并行创建所有任务
        var loadTasks = savedData.Select(async data =>
        {
            var icon = await getIconFunc(data.FilePath);

            return new GameItemViewModel(_env)
            {
                Parent = this, // 这里的 Parent 就是 HomeViewModel 自己
                GameName = data.GameName,
                ExePath = data.FilePath,
                GameIcon = icon,
                LastPlayAt = data.LastPlayAt,
            };
        });

        // 3. 等待异步任务
        var loadedModels = await Task.WhenAll(loadTasks);

        // 4. 清理旧数据并添加新数据
        Games.Clear();
        foreach (var model in loadedModels)
        {
            Games.Add(model);
        }
    }


    [RelayCommand]
    public void AddGame()
    {
        // 收件处：MainWindow.xaml.cs (用于切换内容 Frame)
        WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(AddGameViewModel)));
    }


    [RelayCommand]
    public void StartGame(GameItemViewModel game) // 添加参数
    {
        if (game == null) return;

        string hookPath = game.ExePath; // 直接从参数获取，不再依赖 SelectedGame

        if (Path.GetExtension(hookPath) == ".log")
        {
            hookPath = Path.ChangeExtension(hookPath, ".exe");
        }

        Process.Start(hookPath);

        //打开之后切换到翻译页面
        WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(TranslateViewModel), game.ToEntity()));
    }

    [RelayCommand]
    public void AutoAttachGame()
    {
        GameInfo? gameInfo = _env.GameInfoService.GetRunningGame();
        if (gameInfo!=null)
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(TranslateViewModel), gameInfo));
        }
        else
        {
            Log.Information("没有找到正在运行的已保存游戏，无法自动附加");
        }

    }
}
