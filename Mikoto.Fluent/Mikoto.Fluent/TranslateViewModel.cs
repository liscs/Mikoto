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
        string? textractorPath = App.Env.AppSettings.Textractor_Path32;
        if (CurrentGame.Isx64)
        {
            textractorPath = App.Env.AppSettings.Textractor_Path64;
        }
        // hook
        Task hookTask = App.Env.TextHookService.AutoStartAsync(textractorPath, GameProcessHelper.GetGamePid(CurrentGame), CurrentGame);
        App.Env.TextHookService.MeetHookAddressMessageReceived += Hook_Output;

        // translatorinit
        _translator = TranslatorCommon.TranslatorFactory.GetTranslator(
                App.Env.AppSettings.FirstTranslator,
                App.Env.AppSettings,
                App.Env.AppSettings.FirstTranslator);
        await hookTask;
    }

    private SolvedDataReceivedEventArgs _lastSolvedDataReceivedEventArgs = new();
    private ITranslator? _translator;
    private readonly AsyncLwwTask _translationTask = new();

    private async void Hook_Output(object sender, SolvedDataReceivedEventArgs e)
    {
        //1.得到原句
        string? currentData = e.Data.Data; // 局部变量捕获当前快照
                                           //LWW逻辑处理短时间并发调用
                                           //只处理最新的一次调用
        await _translationTask.ExecuteAsync(async () =>
        {
            if (currentData == _lastSolvedDataReceivedEventArgs.Data?.Data
                || _translator==null)
            {
                return;
            }
            _lastSolvedDataReceivedEventArgs = e;
            string preProcessedText = PreProcessText(currentData);
            string? translatedText =
            await _translator.TranslateAsync(preProcessedText,
                                              CurrentGame.DstLang,
                                              CurrentGame.SrcLang);
            //取得原文和翻译后的文本后，触发UI更新
            Log.Debug("原文：{OriginalText}\n译文：{TranslatedText}", preProcessedText, translatedText);

            _dispatcherQueue.TryEnqueue(() =>
            {
                UpdateUI(preProcessedText, translatedText, _translator.GetLastError());
            });
        });

    }

    private void UpdateUI(string preProcessedText, string? translatedText, string error)
    {
        OriginalText = preProcessedText;
        if (translatedText!=null)
        {
            TranslateText = translatedText;
        }
        else
        {
            IsNotificationOpen = true;
            NotificationMessage = "翻译发生错误：" + error;
            NotificationSeverity = InfoBarSeverity.Warning;
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