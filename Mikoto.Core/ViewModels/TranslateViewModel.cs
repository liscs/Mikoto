using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mikoto.Core.Interfaces;
using Mikoto.Core.Models;
using Mikoto.DataAccess;
using Mikoto.Helpers.Async;
using Mikoto.Helpers.Text;
using Mikoto.TextHook;
using Mikoto.Translators;
using Serilog;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Mikoto.Core.ViewModels;

public partial class TranslateViewModel : ObservableObject
{
    private readonly IMainThreadService _mainThreadService;
    private readonly AsyncLwwTask _translationTask = new();
    private SolvedDataReceivedEventArgs _lastSolvedDataReceivedEventArgs = new();

    // 存储多个翻译结果的集合
    [ObservableProperty]
    public partial ObservableCollection<TranslationResult> MultiTranslateResults { get; set; } = new();

    public GameInfo CurrentGame { get; set; } = new GameInfo();

    [ObservableProperty] public partial string OriginalText { get; set; } = string.Empty;

    #region Notification Properties
    [ObservableProperty] public partial bool IsNotificationOpen { get; set; }
    [ObservableProperty] public partial string NotificationMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial InfoSeverity NotificationSeverity { get; set; }
    #endregion

    IAppEnvironment _env;
    public TranslateViewModel(IAppEnvironment env, IMainThreadService mainThreadService)
    {
        _env = env;
        _mainThreadService = mainThreadService;
    }

    [RelayCommand]
    public async Task InitializeTranslation()
    {
        Log.Information("正在初始化多翻译流程: {GameName}", CurrentGame.GameName);
        try
        {
            // 1. 初始化 Hook
            string? textractorPath = CurrentGame.Isx64 ? _env.AppSettings.Textractor_Path64 : _env.AppSettings.Textractor_Path32;
            Task hookTask = _env.TextHookService.AutoStartAsync(textractorPath, ProcessInterop.ProcessHelper.GetPid(CurrentGame.FilePath), CurrentGame);
            _env.TextHookService.MeetHookAddressMessageReceived += Hook_Output;

            // 2. 预先根据配置初始化翻译结果列表 (例如从配置加载选中的翻译器)
            var enabledTranslators = new List<string> {
                _env.AppSettings.FirstTranslator,
                _env.AppSettings.SecondTranslator,
            };
            MultiTranslateResults.Clear();
            foreach (var name in enabledTranslators)
            {
                MultiTranslateResults.Add(new TranslationResult { TranslatorName = name });
            }

            await hookTask;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "初始化失败");
            ShowNotification("初始化失败", InfoSeverity.Error);
        }
    }

    private async void Hook_Output(object sender, SolvedDataReceivedEventArgs e)
    {
        string? currentData = e.Data.Data;

        await _translationTask.ExecuteAsync(async () =>
        {
            if (currentData == _lastSolvedDataReceivedEventArgs.Data?.Data) return;
            _lastSolvedDataReceivedEventArgs = e;

            string preProcessedText = PreProcessText(currentData);

            // 更新 UI 原文
            _mainThreadService.RunOnMainThread(() => OriginalText = preProcessedText);

            // 3. 并行触发所有翻译任务
            var tasks = MultiTranslateResults.Select(async item =>
            {
                _mainThreadService.RunOnMainThread(() =>
                {
                    item.IsLoading = true;
                    item.ErrorMessage = null;
                });

                var sw = Stopwatch.StartNew();
                try
                {
                    // 1. 获取翻译器实例
                    var translator = TranslatorCommon.TranslatorFactory.GetTranslator(item.TranslatorName, _env.AppSettings, item.TranslatorName);

                    // 2. 检查翻译器是否获取成功
                    if (translator == null)
                    {
                        Log.Warning("无法创建翻译器实例: {Name}", item.TranslatorName);
                        _mainThreadService.RunOnMainThread(() =>
                        {
                            item.IsLoading = false;
                            item.ErrorMessage = "翻译器初始化失败";
                        });
                        return; // 提前退出
                    }

                    // 3. 执行异步翻译
                    string? result = await translator.TranslateAsync(preProcessedText, CurrentGame.DstLang, CurrentGame.SrcLang);
                    sw.Stop();

                    _mainThreadService.RunOnMainThread(() =>
                    {
                        item.IsLoading = false;
                        if (result != null)
                        {
                            item.ResultText = result;
                            Log.Debug("[{Name}] 耗时: {Ms}ms", item.TranslatorName, sw.ElapsedMilliseconds);
                        }
                        else
                        {
                            // 使用 ?. 确保 translator 不为空，或者直接复用上面已经校验过的变量
                            item.ErrorMessage = translator?.GetLastError() ?? "未知错误";
                            Log.Warning("[{Name}] 失败: {Err}", item.TranslatorName, item.ErrorMessage);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "翻译器 {Name} 崩溃", item.TranslatorName);
                    _mainThreadService.RunOnMainThread(() => { item.IsLoading = false; item.ErrorMessage = "插件异常"; });
                }
            });

            // 同时运行所有翻译，不阻塞 Hook 接收
            await Task.WhenAll(tasks);
        });
    }

    private CancellationTokenSource? _notificationTokenSource;
    private async void ShowNotification(string message, InfoSeverity severity, int durationMs = 3000)
    {
        // 1. 取消上一次正在进行的“自动关闭”计时任务
        _notificationTokenSource?.Cancel();
        _notificationTokenSource = new CancellationTokenSource();
        var token = _notificationTokenSource.Token;

        // 2. 更新通知内容并显示
        NotificationMessage = message;
        NotificationSeverity = severity;
        IsNotificationOpen = true;
        try
        {
            // 3. 等待指定时间
            await Task.Delay(durationMs, token);
            // 4. 时间到，关闭通知
            IsNotificationOpen = false;
        }
        catch (TaskCanceledException)
        {
            // 如果任务被取消（说明有新通知进来了），不做任何处理，让新任务接管
        }
    }

    private string PreProcessText(string? currentData)
    {
        var funcName = CurrentGame.RepairFunc;
        if (string.IsNullOrWhiteSpace(currentData)||string.IsNullOrWhiteSpace(funcName))
        {
            return currentData??string.Empty;
        }
        var paramA = CurrentGame.RepairParamA;
        var paramB = CurrentGame.RepairParamB;
        return TextProcessor.PreProcessSrc(funcName, currentData, paramA, paramB);
    }

}
