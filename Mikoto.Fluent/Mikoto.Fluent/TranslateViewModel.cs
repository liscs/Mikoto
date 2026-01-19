using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mikoto.DataAccess;
using Mikoto.TextHook;
using Mikoto.Translators;
using Mikoto.Translators.Interfaces;
using Serilog;
using System.Diagnostics;
using System.Xml.Linq;

namespace Mikoto.Fluent;

public partial class TranslateViewModel : ObservableObject
{
    public GameInfo CurrentGame { get; internal set; }


    [RelayCommand]
    public void InitializeTranslation()
    {
        // hook
        Task hookTask = App.Env.TextHookService.StartAsync(App.Env.AppSettings.Textractor_Path64, GetGamePid(CurrentGame), CurrentGame);
        App.Env.TextHookService.MeetHookAddressMessageReceived += FilterAndDisplayData;

        // translatorinit
        ITranslator? translator = TranslatorCommon.TranslatorFactory.GetTranslator(
                App.Env.AppSettings.FirstTranslator, App.Env.AppSettings,
                "");

    }

    private void FilterAndDisplayData(object sender, SolvedDataReceivedEventArgs e)
    {
        //1.得到原句
        string? currentData = e.Data.Data; // 局部变量捕获当前快照
                                           //LWW逻辑处理短时间并发调用
                                           //只处理最新的一次调用
        await _translationTask.ExecuteAsync(async () =>
        {
            if (currentData == _lastSolvedDataReceivedEventArgs.Data?.Data) return;
            _lastSolvedDataReceivedEventArgs = e;
            string repairedText = CleanText(currentData);
            Dispatcher.Invoke(SetWindowTopMost);

            if (Common.AppSettings.AzureEnableAutoSpeak)
            {
                SpeakIfNoGameVoiceAsync(repairedText).FireAndForget();
            }
            TranslateText(repairedText);
        });

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