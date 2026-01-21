namespace Mikoto.Core.Messages
{
    public record NavigationMessage(Type ViewModelType, object? Parameter = null);
}
