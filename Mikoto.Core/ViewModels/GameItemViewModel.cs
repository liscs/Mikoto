using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mikoto.Core.Interfaces;
using Mikoto.Core.Models;
using Mikoto.DataAccess;
using Mikoto.TextHook;
using Serilog;
using System.Diagnostics;

namespace Mikoto.Core.ViewModels;

public partial class GameItemViewModel(IAppEnvironment env) : ObservableObject
{
    [ObservableProperty]
    public partial GameInfo GameInfo { get; set; }

    [ObservableProperty]
    public partial object? GameIcon { get; set; }


    [RelayCommand]
    public void StartGame()
    {
        string startPath = HookFileHelper.ToEntranceFilePath(GameInfo.FilePath);
        if (!File.Exists(startPath))
        {
            // 游戏文件不存在
            Log.Error("游戏文件不存在，路径：{Path}", startPath);
            return;
        }
        Process.Start(startPath);
        Log.Information("启动游戏，路径：{Path}", startPath);

        //打开之后切换到翻译页面
        WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(TranslateViewModel), GameInfo));

    }

    [RelayCommand]
    public void LEStartGame()
    {
        string path = HookFileHelper.ToEntranceFilePath(GameInfo.FilePath);
        if (!File.Exists(path))
        {
            // 游戏文件不存在
            Log.Error("游戏文件不存在，路径：{Path}", path);
            return;
        }

        var lePath = env.AppSettings.LEPath;
        if (!Directory.Exists(lePath))
        {
            // LE 文件夹不存在
            Log.Error("LE 文件夹不存在，路径：{Path}", lePath);

            return;
        }

        string leExe = Path.Combine(lePath, "LEProc.exe");
        if (!File.Exists(leExe))
        {
            // LE exe不存在
            Log.Error("LE exe不存在，路径：{Path}", leExe);
            return;
        }

        var p = new ProcessStartInfo
        {
            FileName = leExe,
            UseShellExecute = false,
            WorkingDirectory = lePath
        };
        p.ArgumentList.Add("-run");
        p.ArgumentList.Add(path);

        try
        {
            Process.Start(p);
            Log.Information("通过 LE 启动游戏，路径：{Path}", path);
        }
        catch (Exception ex)
        {
            // LE 启动游戏失败
            Log.Error(ex, "LE 启动游戏失败");
            return;
        }


        WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(TranslateViewModel), GameInfo));
    }

    [RelayCommand]
    public void EditGame()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(EditGameViewModel), this));
    }
}