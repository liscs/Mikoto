using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;
using Mikoto.DataAccess;
using Mikoto.Fluent.TextProcess;
using Mikoto.Helpers.Text;
using Mikoto.TextHook;
using System.Collections.ObjectModel;

namespace Mikoto.Fluent.AddGamePages;

public partial class PreProcessViewModel : ObservableObject
{
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    public ObservableCollection<RepairFunctionItem> RepairFunctions { get; }

    public PreProcessViewModel()
    {
        RepairFunctions = new(TextProcessorUI.GetFunctionList());
        SourceText = "这这这是一是一段测试测测试文本。";
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewResult))]
    public partial string SourceText { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewResult))]
    [NotifyPropertyChangedFor(nameof(IsParamBVisible))]
    public partial string SelectedFuncName { get; set; } = nameof(TextProcessor.RepairFun_NoDeal);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewResult))]
    public partial string ParamA { get; set; } = "0";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewResult))]
    public partial string ParamB { get; set; } = "";

    // 实时计算预览结果（依赖属性改变会自动触发刷新）
    public string PreviewResult => TextProcessor.PreProcessSrc(SelectedFuncName, SourceText, ParamA, ParamB);

    // 只有选择正则替换时才显示参数B的输入框
    public Visibility IsParamBVisible => SelectedFuncName == nameof(TextProcessor.RepairFun_RegexReplace)
        ? Visibility.Visible
        : Visibility.Collapsed;

    internal async Task StartHookingAsync(GameInfo draftConfig)
    {
        string? textractorPath = draftConfig.Isx64
            ? App.Env.AppSettings.Textractor_Path64
            : App.Env.AppSettings.Textractor_Path32;

        App.Env.TextHookService.MeetHookAddressMessageReceived += Hook_Output;

        await App.Env.TextHookService.AutoStartAsync(textractorPath, GameProcessHelper.GetGamePid(draftConfig), draftConfig);
    }

    private void Hook_Output(object sender, SolvedDataReceivedEventArgs e)
    {
        string? currentData = e.Data.Data;
        if (string.IsNullOrEmpty(currentData)) return;

        // 回到 UI 线程更新 SourceText
        // 一旦 SourceText 更新，PreviewResult 会因为 NotifyPropertyChangedFor 自动重新计算
        _dispatcherQueue.TryEnqueue(() =>
        {
            SourceText = currentData;
        });
    }
}