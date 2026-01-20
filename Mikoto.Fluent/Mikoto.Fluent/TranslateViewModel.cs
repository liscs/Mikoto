using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Mikoto.DataAccess;
using Mikoto.Helpers.Async;
using Mikoto.Helpers.Text;
using Mikoto.TextHook;
using Mikoto.Translators;
using Mikoto.Translators.Interfaces;
using Serilog;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Mikoto.Fluent;

public partial class TranslateViewModel : ObservableObject
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;
    private readonly AsyncLwwTask _translationTask = new();
    private SolvedDataReceivedEventArgs _lastSolvedDataReceivedEventArgs = new();

    // 存储多个翻译结果的集合
    [ObservableProperty]
    public partial ObservableCollection<TranslationResult> MultiTranslateResults { get; set; } = new();

    public GameInfo CurrentGame { get; internal set; } = new GameInfo();

    [ObservableProperty] public partial string OriginalText { get; set; } = string.Empty;

    #region Notification Properties
    [ObservableProperty] public partial bool IsNotificationOpen { get; set; }
    [ObservableProperty] public partial string NotificationMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial InfoBarSeverity NotificationSeverity { get; set; }
    #endregion

    public TranslateViewModel()
    {
        _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    }

    [RelayCommand]
    public async Task InitializeTranslation()
    {
        Log.Information("正在初始化多翻译流程: {GameName}", CurrentGame.GameName);
        try
        {
            // 1. 初始化 Hook (保持原有逻辑)
            string? textractorPath = CurrentGame.Isx64 ? App.Env.AppSettings.Textractor_Path64 : App.Env.AppSettings.Textractor_Path32;
            Task hookTask = App.Env.TextHookService.AutoStartAsync(textractorPath, GameProcessHelper.GetGamePid(CurrentGame), CurrentGame);
            App.Env.TextHookService.MeetHookAddressMessageReceived += Hook_Output;

            // 2. 预先根据配置初始化翻译结果列表 (例如从配置加载选中的翻译器)
            var enabledTranslators = new List<string> {
                App.Env.AppSettings.FirstTranslator,
                App.Env.AppSettings.SecondTranslator,
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
            ShowNotification("初始化失败", InfoBarSeverity.Error);
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
            _dispatcherQueue.TryEnqueue(() => OriginalText = preProcessedText);

            // 3. 并行触发所有翻译任务
            var tasks = MultiTranslateResults.Select(async item =>
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    item.IsLoading = true;
                    item.ErrorMessage = null;
                });

                var sw = Stopwatch.StartNew();
                try
                {
                    // 获取对应的翻译器实例
                    var translator = TranslatorCommon.TranslatorFactory.GetTranslator(item.TranslatorName, App.Env.AppSettings, item.TranslatorName);
                    string? result = await translator.TranslateAsync(preProcessedText, CurrentGame.DstLang, CurrentGame.SrcLang);
                    sw.Stop();

                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        item.IsLoading = false;
                        if (result != null)
                        {
                            item.ResultText = result;
                            Log.Debug("[{Name}] 耗时: {Ms}ms", item.TranslatorName, sw.ElapsedMilliseconds);
                        }
                        else
                        {
                            item.ErrorMessage = translator.GetLastError();
                            Log.Warning("[{Name}] 失败: {Err}", item.TranslatorName, item.ErrorMessage);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "翻译器 {Name} 崩溃", item.TranslatorName);
                    _dispatcherQueue.TryEnqueue(() => { item.IsLoading = false; item.ErrorMessage = "插件异常"; });
                }
            });

            // 同时运行所有翻译，不阻塞 Hook 接收
            await Task.WhenAll(tasks);
        });
    }

    private CancellationTokenSource? _notificationTokenSource;
    private async void ShowNotification(string message, InfoBarSeverity severity, int durationMs = 3000)
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
