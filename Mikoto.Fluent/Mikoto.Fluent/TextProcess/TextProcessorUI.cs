using Mikoto.Helpers.Text;

namespace Mikoto.Fluent.TextProcess; 

public partial class TextProcessorUI
{
    public static List<RepairFunctionItem> GetFunctionList()
    {
        // 关键点：ResourceLoader 必须在包含 Strings 文件夹的项目中运行
        var loader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();

        var list = new List<RepairFunctionItem>
        {
            new(loader.GetString(nameof(TextProcessor.RepairFun_NoDeal)), nameof(TextProcessor.RepairFun_NoDeal)),
            new(loader.GetString(nameof(TextProcessor.RepairFun_RemoveSingleWordRepeat)), nameof(TextProcessor.RepairFun_RemoveSingleWordRepeat)),
            new(loader.GetString(nameof(TextProcessor.RepairFun_RemoveSentenceRepeat)), nameof(TextProcessor.RepairFun_RemoveSentenceRepeat)),
            new(loader.GetString(nameof(TextProcessor.RepairFun_RemoveLetterNumber)), nameof(TextProcessor.RepairFun_RemoveLetterNumber)),
            new(loader.GetString(nameof(TextProcessor.RepairFun_RemoveHTML)), nameof(TextProcessor.RepairFun_RemoveHTML)),
            new(loader.GetString(nameof(TextProcessor.RepairFun_RegexReplace)), nameof(TextProcessor.RepairFun_RegexReplace))
        };

        // 合并用户自定义脚本
        foreach (var key in TextProcessor.CustomMethodsDict.Keys)
        {
            list.Add(new RepairFunctionItem(key, key));
        }

        return list;
    }
}