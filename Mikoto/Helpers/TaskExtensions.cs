namespace Mikoto.Helpers
{
    public static class TaskExtensions
    {
        public static void FireAndForget(this Task task)
            => _ = task.ConfigureAwait(false);
    }
}
