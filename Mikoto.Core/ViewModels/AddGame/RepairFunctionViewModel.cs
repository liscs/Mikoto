using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Mikoto.Core.Interfaces;
using Mikoto.Core.Models;
using Mikoto.Core.Models.AddGame;
using Mikoto.Helpers.Text;
using Mikoto.Resource;

namespace Mikoto.Core.ViewModels.AddGame
{
    public partial class RepairFunctionViewModel : ObservableObject
    {
        public RepairFunctionViewModel(IAppEnvironment env)
        {
            resourceService= env.ResourceService;
            FunctionList = GetFunctionList();
            SelectedRepairFunction = FunctionList.First();
        }
        private readonly IResourceService resourceService;

        public List<RepairFunctionItem> FunctionList { get; }

        private List<RepairFunctionItem> GetFunctionList()
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

        [ObservableProperty]
        public partial RepairFunctionItem SelectedRepairFunction { get; set; }

        partial void OnSelectedRepairFunctionChanged(RepairFunctionItem value)
        {
            WeakReferenceMessenger.Default.Send(new RepairFunctionChangedMessage(value));
        }
    }
}
