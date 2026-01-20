using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Mikoto.DataAccess;
using Mikoto.Helpers.Async;
using Mikoto.TextHook;
using Mikoto.Translators;
using Mikoto.Translators.Interfaces;
using Serilog;
using System.Diagnostics;
using System.Xml.Linq;

namespace Mikoto.Fluent;

public partial class TranslateViewModel : ObservableObject
{
    public GameInfo CurrentGame { get; internal set; } = new GameInfo();

    [ObservableProperty]
    public partial string OriginalText { get; set; }
    [ObservableProperty]
    public partial string TranslateText { get; set; }

    #region Notification Bar Properties
    // 控制是否显示
    [ObservableProperty]
    public partial bool IsNotificationOpen { get; set; }

    // 通知内容
    [ObservableProperty]
    public partial string NotificationMessage { get; set; }

    // 通知严重程度 (Error, Warning, Informational, Success)
    [ObservableProperty]
    public partial InfoBarSeverity NotificationSeverity { get; set; }
    #endregion

    [RelayCommand]
    public void InitializeTranslation()
    {
        // hook
        Task hookTask = App.Env.TextHookService.StartAsync(App.Env.AppSettings.Textractor_Path64, GetGamePid(CurrentGame), CurrentGame);
        App.Env.TextHookService.MeetHookAddressMessageReceived += Hook_Output;

        // translatorinit
        _translator = TranslatorCommon.TranslatorFactory.GetTranslator(
                App.Env.AppSettings.FirstTranslator, App.Env.AppSettings,
                "");

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
            ||_translator==null)
            {
                return;
            }
            _lastSolvedDataReceivedEventArgs = e;
            string preProcessedText = PreProcessText(currentData);
            string? translatedText =
            await _translator.TranslateAsync(preProcessedText,
                                              CurrentGame.SrcLang,
                                              CurrentGame.DstLang);
            //取得原文和翻译后的文本后，触发UI更新
            Log.Debug("原文：{OriginalText}\n译文：{TranslatedText}", preProcessedText, translatedText);
            UpdateUI(preProcessedText, translatedText, _translator.GetLastError());
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

    private static string PreProcessText(string? currentData)
    {
        if (string.IsNullOrWhiteSpace(currentData))
        {
            return string.Empty;
        }
        return currentData.Trim();
    }

    private static int GetGamePid(GameInfo currentGame)
    {
        string name;
        if (Path.GetExtension(currentGame.FilePath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
        {
            name = Path.GetFileNameWithoutExtension(currentGame.FilePath);
        }
        else
        {
            name = Path.GetFileName(currentGame.FilePath);
        }

        List<Process> gameProcessList = Process.GetProcessesByName(name).ToList();
        if (gameProcessList.Count == 0)
        {
            throw new Exception("Game process not found.");
        }
        return gameProcessList[0].Id;
    }
}