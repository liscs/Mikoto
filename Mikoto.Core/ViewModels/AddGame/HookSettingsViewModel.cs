using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Mikoto.Core.Interfaces;
using Mikoto.DataAccess;
using Mikoto.TextHook;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Mikoto.Core.ViewModels.AddGame;

public partial class HookSettingsViewModel : ObservableObject
{
    private IAppEnvironment _env;
    public HookSettingsViewModel(IAppEnvironment env)
    {
        _env = env;
        // 检查权限状态
        IsAdminWarningVisible = !Environment.IsPrivilegedProcess;
    }

    public ObservableCollection<HookFuncItemViewModel> HookFunctions { get; } = new();

    [ObservableProperty]
    public partial HookFuncItemViewModel? SelectedFunction { get; set; }

    [ObservableProperty]
    public partial bool IsAdminWarningVisible { get; set; }

    /// <summary>
    /// 启动 Hook 服务
    /// </summary>
    public async Task StartHookingAsync(GameInfo config)
    {
        HookFunctions.Clear();

        // 订阅事件
        WeakReferenceMessenger.Default.Register<HookMessage>(this, (r, m) =>
        {
            AllHook_Output(m.HookReceivedEventArgs);
        });

        // 异步启动，FireAndForget 模式
        string? textractorPath = _env.AppSettings.Textractor_Path32;
        if (config.Isx64)
        {
            textractorPath = _env.AppSettings.Textractor_Path64;
        }
        // hook
        // TODO 这里需要加进程未启动的失败处理
        Task hookTask = _env.TextHookService.AutoStartAsync(textractorPath, ProcessInterop.ProcessHelper.GetPid(config.FilePath), config);
    }

    private void AllHook_Output(HookReceivedEventArgs e)
    {
        // WinUI 3 的 ObservableCollection 必须在 UI 线程修改
        // 使用 Microsoft.UI.Dispatching.DispatcherQueue
        _env.MainThreadService.RunOnMainThread(() =>
        {
            TextHookData? data = e.Data;
            HookFuncItemViewModel hookFuncItem = new()
            {
                Data = data.Data??string.Empty,
                GamePID = data.GamePID,
                MisakaHookCode = data.MisakaHookCode,
                HookCode = data.HookCode,
                HookFunc = data.HookFunc,
            };

            // 正则过滤
            if (InvalidCodeRegex().IsMatch(data.MisakaHookCode))
            {
                data.MisakaHookCode = string.Empty;
            }

            if (e.Index < HookFunctions.Count)
            {
                // 更新现有项
                HookFunctions[e.Index] = hookFuncItem;
            }
            else
            {
                // 添加新项
                HookFunctions.Add(hookFuncItem);
            }
        });
    }

    [GeneratedRegex(@"【0:FFFFFFFFFFFFFFFF:FFFFFFFFFFFFFFFF】|【FFFFFFFFFFFFFFFF:FFFFFFFFFFFFFFFF:FFFFFFFFFFFFFFFF】", RegexOptions.Compiled)]
    private static partial Regex InvalidCodeRegex();

    [ObservableProperty]
    public partial bool ShowError { get; set; }
}
