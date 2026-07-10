using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Mikoto.Core.Interfaces;
using Mikoto.DataAccess;
using Mikoto.TextHook;
using Serilog;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Mikoto.Core.ViewModels.AddGame;

public partial class HookSettingsViewModel : ObservableObject
{
    private IAppEnvironment _env;
    public HookSettingsViewModel(IAppEnvironment env)
    {
        _env = env;
    }

    public ObservableCollection<HookFuncItemViewModel> HookFunctions { get; } = new();

    [ObservableProperty]
    public partial HookFuncItemViewModel? SelectedFunction { get; set; }

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
        Task hookTask = _env.TextHookService.AutoStartAsync(textractorPath, config);
    }

    private void AllHook_Output(HookReceivedEventArgs e)
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

        _env.MainThreadService.RunOnMainThread(() =>
        {
            try
            {
                // 检查 VM 是否已经被清理
                if (HookFunctions == null) return;

                if (e.Index < HookFunctions.Count)
                {
                    // 这里最容易报 ObjectDisposedException
                    HookFunctions[e.Index] = hookFuncItem;
                }
                else
                {
                    HookFunctions.Add(hookFuncItem);
                }
            }
            catch (ObjectDisposedException ex)
            {
                Log.Debug(ex, "UI 对象已销毁，停止更新集合 {HookFunctions} 。", HookFunctions);
            }
        });
    }

    [GeneratedRegex(@"【0:FFFFFFFFFFFFFFFF:FFFFFFFFFFFFFFFF】|【FFFFFFFFFFFFFFFF:FFFFFFFFFFFFFFFF:FFFFFFFFFFFFFFFF】", RegexOptions.Compiled)]
    private static partial Regex InvalidCodeRegex();

    [ObservableProperty]
    public partial bool ShowError { get; set; }
}
