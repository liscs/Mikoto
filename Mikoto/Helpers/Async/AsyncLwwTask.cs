namespace Mikoto.Helpers.Async;

public class AsyncLwwTask
{
    private long _currentVersion = 0;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// 执行 Last Write Wins 任务
    /// </summary>
    /// <param name="action">需要执行的逻辑</param>
    /// <param name="useYield">是否允许短暂让出，给后续的高频触发留出覆盖机会</param>
    public async Task ExecuteAsync(Func<Task> action, bool useYield = true)
    {
        // 1. 发放新版本号（票据）
        long myVersion = Interlocked.Increment(ref _currentVersion);

        if (useYield)
        {
            // 2. 给潜在的“重叠调用”留出更新版本号的机会
            await Task.Yield();
        }

        // 3. 预检：如果此时已经有更新的任务进来了，老任务直接结束
        if (Interlocked.Read(ref _currentVersion) != myVersion) return;

        // 4. 排队进入执行区
        await _lock.WaitAsync();
        try
        {
            // 5. 复检：拿到锁后再次确认我依然是最新版本
            if (Interlocked.Read(ref _currentVersion) != myVersion) return;

            // 执行核心逻辑
            await action();
        }
        finally
        {
            _lock.Release();
        }
    }
}
