using CommunityToolkit.Mvvm.ComponentModel;
using Mikoto.Core.Interfaces;
using Mikoto.Core.Models;
using Mikoto.DataAccess;
using Mikoto.TextHook;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Mikoto.Core.ViewModels.AddGamePages;

public partial class HookSettingsViewModel : ObservableObject
{
    private readonly IMainThreadService _mainThreadService;
    private IAppEnvironment _env;
    public HookSettingsViewModel(IAppEnvironment env, IMainThreadService mainThreadService)
    {
        _env = env;
        // 检查权限状态
        IsAdminWarningVisible = !Environment.IsPrivilegedProcess;
        _mainThreadService = mainThreadService;
    }

    public ObservableCollection<HookFuncItem> HookFunctions { get; } = new();

    [ObservableProperty]
    public partial HookFuncItem? SelectedFunction { get; set; }

    [ObservableProperty]
    public partial bool IsAdminWarningVisible { get; set; }

    /// <summary>
    /// 启动 Hook 服务
    /// </summary>
    public async Task StartHookingAsync(GameInfo config)
    {
        HookFunctions.Clear();

        // 订阅事件
        _env.TextHookService.HookMessageReceived += AllHook_Output;

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

    private void AllHook_Output(object sender, HookReceivedEventArgs e)
    {
        // WinUI 3 的 ObservableCollection 必须在 UI 线程修改
        // 使用 Microsoft.UI.Dispatching.DispatcherQueue
        _mainThreadService.RunOnMainThread(() =>
        {
            TextHookData? data = e.Data;
            HookFuncItem hookFuncItem = new()
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
