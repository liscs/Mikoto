namespace Mikoto.Core.Models
{
    /// <summary>
    /// 主页设置到特定的View，不存储本次跳转的历史返回堆栈，通常用于重置页面状态，或是不再需要返回的导航。
    /// </summary>
    public record SetNavigationViewMessage(Type ViewModelType, object? Parameter = null);
}
