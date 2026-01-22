using CommunityToolkit.Mvvm.ComponentModel;
using Mikoto.Core.Interfaces;
using Mikoto.Core.Models;
using Mikoto.DataAccess;
using Mikoto.Helpers.Text;
using Mikoto.Resource;
using Mikoto.TextHook;
using System.Collections.ObjectModel;

namespace Mikoto.Core.ViewModels.AddGame;

public partial class PreProcessViewModel : ObservableObject
{
    public static List<RepairFunctionItem> GetFunctionList(IResourceService resourceService)
    {
        var list = new List<RepairFunctionItem>
        {
            new(resourceService.Get(nameof(TextProcessor.RepairFun_NoDeal)), nameof(TextProcessor.RepairFun_NoDeal)),
            new(resourceService.Get(nameof(TextProcessor.RepairFun_RemoveSingleWordRepeat)), nameof(TextProcessor.RepairFun_RemoveSingleWordRepeat)),
            new(resourceService.Get(nameof(TextProcessor.RepairFun_RemoveSentenceRepeat)), nameof(TextProcessor.RepairFun_RemoveSentenceRepeat)),
            new(resourceService.Get(nameof(TextProcessor.RepairFun_RemoveLetterNumber)), nameof(TextProcessor.RepairFun_RemoveLetterNumber)),
            new(resourceService.Get(nameof(TextProcessor.RepairFun_RemoveHTML)), nameof(TextProcessor.RepairFun_RemoveHTML)),
            new(resourceService.Get(nameof(TextProcessor.RepairFun_RegexReplace)), nameof(TextProcessor.RepairFun_RegexReplace))
        };

        // 合并用户自定义脚本
        foreach (var key in TextProcessor.CustomMethodsDict.Keys)
        {
            list.Add(new RepairFunctionItem(key, key));
        }

        return list;
    }


    public ObservableCollection<RepairFunctionItem> RepairFunctions { get; }

    IAppEnvironment _env;
    public PreProcessViewModel(IAppEnvironment env)
    {
        _env =env;
        RepairFunctions = new(GetFunctionList(env.ResourceService));
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
    public bool IsParamBVisible => SelectedFuncName == nameof(TextProcessor.RepairFun_RegexReplace);

    public async Task StartHookingAsync(GameInfo draftConfig)
    {
        string? textractorPath = draftConfig.Isx64
            ? _env.AppSettings.Textractor_Path64
            : _env.AppSettings.Textractor_Path32;

        _env.TextHookService.MeetHookAddressMessageReceived += Hook_Output;

        await _env.TextHookService.AutoStartAsync(textractorPath, ProcessInterop.ProcessHelper.GetPid(draftConfig.FilePath), draftConfig);
    }

    private void Hook_Output(object sender, SolvedDataReceivedEventArgs e)
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