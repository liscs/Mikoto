namespace Mikoto.Core.Models
{
    /// <summary>
    /// 主页导航到特定的View
    /// </summary>
    public record NavigationMessage(Type ViewModelType, object? Parameter = null);
}
