namespace Mikoto.Helpers.Async;

public static class TaskExtensions
{
    public static void FireAndForget(this Task task)
        => _ = task.ConfigureAwait(false);
}
