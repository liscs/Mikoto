using CommunityToolkit.Mvvm.ComponentModel;

namespace Mikoto.Core.Models;

public partial class HookFuncItem : ObservableObject
{
    public int GamePID { get; set; }
    public string HookFunc { get; set; } = string.Empty;
    public string HookCode { get; set; } = string.Empty;
    public string MisakaHookCode { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}
