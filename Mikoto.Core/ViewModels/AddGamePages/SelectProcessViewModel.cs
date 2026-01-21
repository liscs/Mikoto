using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mikoto.Core.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Mikoto.Core.ViewModels.AddGamePages;

public partial class SelectProcessViewModel : ObservableObject
{
    // 存储所有发现的进程
    public ObservableCollection<ProcessItem> Processes { get; } = new();

    [ObservableProperty]
    public partial ProcessItem? SelectedProcess { get; set; }

    [RelayCommand]
    public async Task RefreshProcessesAsync()
    {
        Processes.Clear();

        // 在后台线程获取进程，避免 UI 卡死
        var list = await Task.Run(() =>
        {
            return Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)) // 只保留有窗口的进程（通常是游戏）
                .Select(p => new ProcessItem(
                    p.ProcessName,
                    p.MainWindowTitle,
                    p.Id,
                    p.MainModule?.FileName ?? string.Empty))
                .OrderBy(x => x.Title)
                .ToList();
        });

        foreach (var item in list)
        {
            Processes.Add(item);
        }
    }

    [ObservableProperty]
    public partial bool ShowError { get; set; }
}