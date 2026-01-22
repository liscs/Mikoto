using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Mikoto.Core.Interfaces;
using Mikoto.Core.Models.AddGame;
using Mikoto.DataAccess;
using Mikoto.Helpers.Text;
using Mikoto.TextHook;

namespace Mikoto.Core.ViewModels.AddGame;

public partial class PreProcessViewModel : ObservableObject
{
    private IAppEnvironment _env;

    public PreProcessViewModel(IAppEnvironment env, RepairFunctionViewModel repairFunctionViewModel)
    {
        _env = env;
        RepairFunctionViewModel = repairFunctionViewModel;
        // 订阅子 ViewModel 的属性变更
        WeakReferenceMessenger.Default.Register<RepairFunctionChangedMessage>(this, (r, m) =>
        {
            OnPropertyChanged(nameof(RepairFunctionViewModel));
            OnPropertyChanged(nameof(PreviewResult));
            OnPropertyChanged(nameof(IsParamBVisible));
        });
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewResult))]
    [NotifyPropertyChangedFor(nameof(IsParamBVisible))]
    public partial RepairFunctionViewModel RepairFunctionViewModel { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewResult))]
    public partial string SourceText { get; set; } = "这这这是一是一段测试测测试文本。";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewResult))]
    public partial string ParamA { get; set; } = "0";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewResult))]
    public partial string ParamB { get; set; } = "";

    // 实时计算预览结果（依赖属性改变会自动触发刷新）
    public string PreviewResult => TextProcessor.PreProcessSrc(RepairFunctionViewModel.SelectedRepairFunction.MethodName, SourceText, ParamA, ParamB);

    // 只有选择正则替换时才显示参数B的输入框
    public bool IsParamBVisible => RepairFunctionViewModel.SelectedRepairFunction.MethodName == nameof(TextProcessor.RepairFun_RegexReplace);

    public async Task StartHookingAsync(GameInfo draftConfig)
    {
        string? textractorPath = draftConfig.Isx64
            ? _env.AppSettings.Textractor_Path64
            : _env.AppSettings.Textractor_Path32;

        WeakReferenceMessenger.Default.Register<MeetHookMessage>(this, (r, m) =>
        {
            Hook_Output(m.SolvedDataReceivedEventArgs);
        });

        await _env.TextHookService.AutoStartAsync(textractorPath, ProcessInterop.ProcessHelper.GetPid(draftConfig.FilePath), draftConfig);
    }

    private void Hook_Output(SolvedDataReceivedEventArgs e)
    {
        string? currentData = e.Data.Data;
        if (string.IsNullOrEmpty(currentData)) return;

        // 回到 UI 线程更新 SourceText
        // 一旦 SourceText 更新，PreviewResult 会因为 NotifyPropertyChangedFor 自动重新计算
        _env.MainThreadService.RunOnMainThread(() =>
        {
            SourceText = currentData;
        });
    }
}