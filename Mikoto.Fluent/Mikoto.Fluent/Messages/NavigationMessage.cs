namespace Mikoto.Fluent.Messages
{
    public record NavigationMessage(Type PageType, object? Parameter = null);
}
