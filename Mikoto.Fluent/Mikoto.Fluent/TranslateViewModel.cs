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
using System.Diagnostics;

namespace Mikoto.Fluent;

public partial class TranslateViewModel : ObservableObject
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;

    public TranslateViewModel()
    {
        // 获取当前线程的 DispatcherQueue（必须在 UI 线程执行构造时调用）
        _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    }


    public GameInfo CurrentGame { get; internal set; } = new GameInfo();

    [ObservableProperty]
    public partial string OriginalText { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string TranslateText { get; set; } = string.Empty;

    #region Notification Bar Properties
    // 控制是否显示
    [ObservableProperty]
    public partial bool IsNotificationOpen { get; set; }

    // 通知内容
    [ObservableProperty]
    public partial string NotificationMessage { get; set; } = string.Empty;

    // 通知严重程度 (Error, Warning, Informational, Success)
    [ObservableProperty]
    public partial InfoBarSeverity NotificationSeverity { get; set; }
    #endregion

    [RelayCommand]
    public async Task InitializeTranslation()
    {
        Log.Information("正在初始化游戏 {GameName} 的翻译流程, 游戏PID: {PID}",
            CurrentGame.GameName, GameProcessHelper.GetGamePid(CurrentGame));

        try
        {
            string? textractorPath = CurrentGame.Isx64
                ? App.Env.AppSettings.Textractor_Path64
                : App.Env.AppSettings.Textractor_Path32;

            if (string.IsNullOrEmpty(textractorPath))
            {
                Log.Warning("Textractor 路径未配置，初始化可能失败");
            }

            // Hook 启动日志
            Log.Debug("正在启动 TextHookService, 路径: {Path}", textractorPath);
            Task hookTask = App.Env.TextHookService.AutoStartAsync(textractorPath, GameProcessHelper.GetGamePid(CurrentGame), CurrentGame);
            App.Env.TextHookService.MeetHookAddressMessageReceived += Hook_Output;

            // 翻译器初始化日志
            _translator = TranslatorCommon.TranslatorFactory.GetTranslator(
                    App.Env.AppSettings.FirstTranslator,
                    App.Env.AppSettings,
                    App.Env.AppSettings.FirstTranslator);


            await hookTask;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "初始化翻译流程时发生异常");
            ShowNotification("初始化失败", InfoBarSeverity.Error);
        }
    }

    private SolvedDataReceivedEventArgs _lastSolvedDataReceivedEventArgs = new();
    private ITranslator? _translator;
    private readonly AsyncLwwTask _translationTask = new();

    private async void Hook_Output(object sender, SolvedDataReceivedEventArgs e)
    {
        string? currentData = e.Data.Data;

        await _translationTask.ExecuteAsync(async () =>
        {
            // 过滤重复数据
            if (currentData == _lastSolvedDataReceivedEventArgs.Data?.Data || _translator == null)
            {
                return;
            }

            _lastSolvedDataReceivedEventArgs = e;

            // 1. 文本预处理
            string preProcessedText = PreProcessText(currentData);

            // 2. 开始翻译并计时
            var sw = Stopwatch.StartNew();
            try
            {
                string? translatedText = await _translator.TranslateAsync(
                    preProcessedText,
                    CurrentGame.DstLang,
                    CurrentGame.SrcLang);

                sw.Stop();

                if (translatedText != null)
                {
                    // 记录成功日志，包含耗时
                    Log.Debug("翻译成功 [耗时: {Elapsed}ms] 原文: {Original} 译文: {Translated}",
                        sw.ElapsedMilliseconds, preProcessedText, translatedText);

                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        UpdateUI(preProcessedText, translatedText, string.Empty);
                    });
                }
                else
                {
                    string error = _translator.GetLastError();
                    Log.Warning("翻译失败 [耗时: {Elapsed}ms] 错误原因: {Error}", sw.ElapsedMilliseconds, error);

                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        UpdateUI(preProcessedText, null, error);
                    });
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                Log.Error(ex, "翻译过程抛出未处理异常 [原文: {Text}]", preProcessedText);
            }
        });
    }

    private void UpdateUI(string preProcessedText, string? translatedText, string error)
    {
        OriginalText = preProcessedText;
        if (translatedText != null)
        {
            TranslateText = translatedText;
        }
        else
        {
            ShowNotification("翻译发生错误：" + error, InfoBarSeverity.Warning);
        }
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