using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        [NotifyPropertyChangedFor(nameof(CanGoNext))]
        public partial int CurrentStepIndex { get; set; }

        public bool CanGoBack => CurrentStepIndex > 0;
        public bool CanGoNext => CurrentStepIndex < Steps.Count - 1;

        [ObservableProperty]
        public partial object? CurrentStepTitle { get; set; }

        // 获取当前展示的面包屑列表（只显示到当前步）
        public ObservableCollection<string> BreadcrumbItems { get; } = new();

        [RelayCommand]
        public void MoveNext()
        {
            if (CurrentStepIndex < Steps.Count - 1)
            {
                BreadcrumbItems.Add(Steps[CurrentStepIndex+1].Title);
                CurrentStepIndex++;
            }
        }

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
