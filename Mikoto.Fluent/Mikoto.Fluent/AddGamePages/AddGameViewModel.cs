using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mikoto.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.ServerSentEvents;
using System.Text;

namespace Mikoto.Fluent.AddGamePages
{
    public partial class AddGameViewModel : ObservableObject
    {
        // 这个就是我们要初始化的对象，所有步骤都会往这里写数据
        public GameInfo DraftConfig { get; } = new();

        public AddGameViewModel()
        {
            // 初始化面包屑
            if (Steps.Count > 0)
            {
                BreadcrumbItems.Add(Steps[0].Title);
            }
        }

        // 定义所有的步骤
        public List<StepItem> Steps { get; } = new()
    {
        new StepItem { Title = "选择进程", PageType = typeof(SelectProcessPage) },
        new StepItem { Title = "Hook配置", PageType = typeof(HookSettingsPage) },
        new StepItem { Title = "文本处理", PageType = typeof(PreProcessPage) },
        new StepItem { Title = "语言设置", PageType = typeof(LanguagePage) }
    };

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanGoBack))]
        [NotifyPropertyChangedFor(nameof(IsLastStep))]
        [NotifyPropertyChangedFor(nameof(NextButtonText))]
        public partial int CurrentStepIndex { get; set; }

        public bool CanGoBack => CurrentStepIndex > 0;

        [ObservableProperty]
        public partial object? CurrentStepTitle { get; set; }

        // 获取当前展示的面包屑列表（只显示到当前步）
        public ObservableCollection<string> BreadcrumbItems { get; } = new();

        [RelayCommand]
        private void MoveNext()
        {
            // 无论当前是哪一步，都需要先保存当前页面的数据到 DraftConfig
            WeakReferenceMessenger.Default.Send(new RequestSaveDataMessage(DraftConfig, async (isSuccess) =>
            {
                if (!isSuccess) return; // 子页面校验失败（如未选路径），拦截跳转

                if (CurrentStepIndex < Steps.Count - 1)
                {
                    // --- 情况 A: 还在中间步骤，继续下一步 ---
                    CurrentStepIndex++;
                    // 同步更新面包屑
                    BreadcrumbItems.Add(Steps[CurrentStepIndex].Title);
                }
                else
                {
                    // --- 情况 B: 已经是最后一步，点击了“完成” ---
                    await Task.Run(() => App.Env.GameInfoService.SaveGameInfo(DraftConfig));
                }
            }));
        }

        // 是否处于最后一步
        public bool IsLastStep => CurrentStepIndex == Steps.Count - 1;

        // 动态按钮文字
        public string NextButtonText => IsLastStep ? "完成" : "下一步";

        [RelayCommand]
        public void MoveBack()
        {
            if (CurrentStepIndex > 0)
            {
                BreadcrumbItems.Remove(Steps[CurrentStepIndex].Title);
                CurrentStepIndex--;
            }
        }
    }
}
